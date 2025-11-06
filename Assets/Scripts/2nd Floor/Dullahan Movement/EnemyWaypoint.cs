using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySmartPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform player;
    public float sightRange = 10f;
    public float patrolWaitTime = 2f;
    public float rotationSpeed = 5f;

    // Audio
    public AudioSource audioSource;
    public AudioClip nearPlayerClip;   // When enemy is near player (warning)
    public AudioClip chaseStartClip;   // When enemy starts chasing
    public AudioClip chaseLoopClip;    // While chasing (looped)

    private NavMeshAgent agent;
    private Animator animator;
    private float patrolTimer;
    private bool chasingPlayer = false;
    private bool wasChasingLastFrame = false;
    private bool wasNearLastFrame = false;
    private List<int> unusedWaypointIndices = new List<int>();
    private int lastWaypoint = -1;
    private bool isMovementPaused = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        ResetUnusedWaypoints();
        GoToRandomWaypoint();
    }

    void Update()
    {
        if (isMovementPaused) return;

        bool isNear = IsPlayerClose();

        // Play "near" sound when entering proximity
        if (isNear && !wasNearLastFrame)
        {
            if (audioSource != null && nearPlayerClip != null)
                audioSource.PlayOneShot(nearPlayerClip);
        }
        wasNearLastFrame = isNear;

        if (isNear)
        {
            chasingPlayer = true;

            // Play chase start sound when chase begins
            if (!wasChasingLastFrame)
            {
                if (audioSource != null && chaseStartClip != null)
                    audioSource.PlayOneShot(chaseStartClip);

                // Start looping chase music
                if (audioSource != null && chaseLoopClip != null)
                {
                    audioSource.clip = chaseLoopClip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }

            agent.SetDestination(player.position);
        }
        else
        {
            if (chasingPlayer)
            {
                chasingPlayer = false;
                GoToRandomWaypoint();

                // Stop looping chase music
                if (audioSource != null && audioSource.clip == chaseLoopClip)
                {
                    audioSource.Stop();
                    audioSource.clip = null;
                    audioSource.loop = false;
                }
            }
            Patrol();
        }

        wasChasingLastFrame = chasingPlayer;

        // Smooth rotation
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(agent.velocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }

    }

    bool IsPlayerClose()
    {
        return Vector3.Distance(player.position, transform.position) <= sightRange;
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime)
            {
                GoToRandomWaypoint();
                patrolTimer = 0f;
            }
        }
    }

    void GoToRandomWaypoint()
    {
        if (unusedWaypointIndices.Count == 0)
            ResetUnusedWaypoints();

        // Prevent immediately repeating the last waypoint
        if (lastWaypoint != -1 && unusedWaypointIndices.Count > 1)
            unusedWaypointIndices.Remove(lastWaypoint);

        if (unusedWaypointIndices.Count == 0)
            return;

        int randomListIndex = Random.Range(0, unusedWaypointIndices.Count);
        int nextWaypoint = unusedWaypointIndices[randomListIndex];
        lastWaypoint = nextWaypoint;
        agent.SetDestination(waypoints[nextWaypoint].position);
        unusedWaypointIndices.RemoveAt(randomListIndex);
    }

    void ResetUnusedWaypoints()
    {
        unusedWaypointIndices.Clear();
        for (int i = 0; i < waypoints.Length; i++)
        {
            unusedWaypointIndices.Add(i);
        }
    }

    public void SetMovementPaused(bool paused)
    {
        isMovementPaused = paused;
        if (paused && agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            Debug.Log($"[EnemySmartPatrol] Movement PAUSED");
        }
        else if (!paused && agent != null)
        {
            agent.isStopped = false;
            Debug.Log($"[EnemySmartPatrol] Movement RESUMED");
        }
    }
}

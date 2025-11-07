using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Kamatayan animation states
/// </summary>
public enum KamatayanAnimationState
{
    Idle,
    Patrolling,
    Looking,
    Chasing,
    Attacking
}

/// <summary>
/// REWRITTEN Kamatayan AI Controller - Simplified and more robust
/// - Smarter patrol system that actually works
/// - Better state management
/// - More reliable waypoint navigation
/// </summary>
public class KamatayanController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] Animator animator;

    [Header("Patrol Behavior")]
    [SerializeField] Transform[] patrolWaypoints;
    [SerializeField] float patrolSpeed = 3.5f;
    [SerializeField] float waypointReachedDistance = 1.5f;
    [SerializeField] float waitTimeAtWaypoint = 2f;
    [SerializeField] bool useRandomPatrol = false;
    [SerializeField] float autoPatrolRadius = 20f;
    [SerializeField] int minWaypointsToGenerate = 4;

    [Header("Looking Behavior")]
    [SerializeField] float lookingInterval = 10f; // How many waypoints before looking
    [SerializeField] float lookingDuration = 4f;
    [SerializeField] float rotationSpeed = 120f; // Degrees per second
    
    [Header("Detection")]
    [SerializeField] float detectionRadius = 15f;
    [SerializeField] float fieldOfViewAngle = 60f;
    [SerializeField] bool requireLineOfSight = true;
    [SerializeField] LayerMask lineOfSightObstructionMask = ~0;

    [Header("Chase Behavior")]
    [SerializeField] float chaseSpeed = 6f;
    [SerializeField] float loseChaseDistance = 25f;
    [SerializeField] float chaseTimeout = 30f;
    [SerializeField] bool persistentChase = true;
    [SerializeField] float attackRange = 2f; // Distance to stop from player (should match EnemyDamageController's attackRange)

    [Header("Animation Parameters")]
    [SerializeField] string animParamIsIdle = "IsIdle";
    [SerializeField] string animParamIsWalking = "IsWalking";
    [SerializeField] string animParamIsLooking = "IsLooking";
    [SerializeField] string animParamIsChasing = "IsChasing";

    [Header("Debug")]
    [SerializeField] bool enableDebugLogs = true;

    // State tracking
    private KamatayanAnimationState currentState = KamatayanAnimationState.Idle;
    private int currentWaypointIndex = -1;
    private int waypointsVisitedSinceLastLook = 0;
    private Vector3 currentDestination;
    private bool hasReachedDestination = false;
    
    // Chase tracking
    private bool isChasing = false;
    private float chaseStartTime;
    private Vector3 lastKnownPlayerPosition;
    
    // Timing
    private float stateStartTime;
    private float waypointArrivalTime;
    
    // Waypoint system
    private List<Vector3> generatedWaypoints = new List<Vector3>();
    private bool useGeneratedWaypoints = false;

    void Reset()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Awake()
    {
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        LogDebug("=== Kamatayan Controller Starting ===");
        
        // Validate components
        if (!ValidateSetup())
            return;

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Setup waypoint system
        SetupWaypoints();
        
        // Configure NavMeshAgent
        navMeshAgent.speed = patrolSpeed;
        navMeshAgent.stoppingDistance = waypointReachedDistance;
        navMeshAgent.autoBraking = true;

        LogDebug($"Initialization complete. Waypoints: {GetWaypointCount()}, Player: {(player != null ? player.name : "Not Found")}");
        
        // Start patrolling
        StartCoroutine(MainStateMachine());
    }

    bool ValidateSetup()
    {
        if (navMeshAgent == null)
        {
            Debug.LogError($"[{name}] NavMeshAgent is missing!");
            enabled = false;
            return false;
        }

        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError($"[{name}] Not on NavMesh! Position: {transform.position}");
            enabled = false;
            return false;
        }

        return true;
    }

    void SetupWaypoints()
    {
        // Check if we have manual waypoints
        if (patrolWaypoints != null && patrolWaypoints.Length > 0)
        {
            // Validate waypoints
            int validCount = 0;
            foreach (var wp in patrolWaypoints)
            {
                if (wp != null) validCount++;
            }
            
            if (validCount > 0)
            {
                useGeneratedWaypoints = false;
                LogDebug($"Using {validCount} manual waypoints");
                return;
            }
        }

        // Generate waypoints
        GeneratePatrolWaypoints();
    }

    void GeneratePatrolWaypoints()
    {
        generatedWaypoints.Clear();
        Vector3 center = transform.position;
        
        int waypointCount = Random.Range(minWaypointsToGenerate, minWaypointsToGenerate + 3);
        
        for (int i = 0; i < waypointCount; i++)
        {
            float angle = (360f / waypointCount) * i;
            float distance = Random.Range(autoPatrolRadius * 0.5f, autoPatrolRadius);
            
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            Vector3 targetPos = center + direction * distance;
            
            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, autoPatrolRadius, NavMesh.AllAreas))
            {
                generatedWaypoints.Add(hit.position);
            }
        }
        
        useGeneratedWaypoints = true;
        LogDebug($"Generated {generatedWaypoints.Count} patrol waypoints");
    }

    /// <summary>
    /// Main state machine - controls all behavior
    /// </summary>
    IEnumerator MainStateMachine()
    {
        while (true)
        {
            // Always check for player detection first
            if (CheckForPlayer() && !isChasing)
            {
                yield return StartCoroutine(ChaseState());
                continue;
            }

            // If we're chasing and lost the player
            if (isChasing)
            {
                yield return StartCoroutine(ReturnToPatrolState());
                continue;
            }

            // Normal patrol cycle
            yield return StartCoroutine(PatrolToWaypointState());

            // Look around immediately at every waypoint
            yield return StartCoroutine(LookAroundState());

            // Wait at waypoint (configurable via waitTimeAtWaypoint field)
            yield return StartCoroutine(WaitAtWaypointState());

            yield return null;
        }
    }

    /// <summary>
    /// State: Patrol to next waypoint
    /// </summary>
    IEnumerator PatrolToWaypointState()
    {
        ChangeState(KamatayanAnimationState.Patrolling);
        navMeshAgent.speed = patrolSpeed;
        navMeshAgent.stoppingDistance = waypointReachedDistance; // Reset stopping distance for patrol
        navMeshAgent.isStopped = false;
        
        // Get next waypoint
        Vector3 destination = GetNextWaypoint();
        
        if (!navMeshAgent.SetDestination(destination))
        {
            LogDebug("Failed to set destination, trying next waypoint");
            yield break;
        }
        
        LogDebug($"Moving to waypoint {currentWaypointIndex} at {destination}");
        
        // Wait until we reach the waypoint
        float timeout = 30f;
        float startTime = Time.time;
        
        while (true)
        {
            // Check for player
            if (CheckForPlayer())
            {
                yield break;
            }
            
            // Check if reached destination
            if (!navMeshAgent.pathPending)
            {
                float distanceToDestination = Vector3.Distance(transform.position, destination);

                // Check if reached waypoint (either by distance OR by NavMesh arrival)
                bool hasArrived = distanceToDestination <= waypointReachedDistance;
                bool navMeshArrived = navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.5f;

                if (hasArrived || navMeshArrived)
                {
                    LogDebug($"Reached waypoint {currentWaypointIndex} (distance: {distanceToDestination:F2}m, remaining: {navMeshAgent.remainingDistance:F2}m)");
                    break;
                }
            }
            
            // Timeout check
            if (Time.time - startTime > timeout)
            {
                LogDebug("Waypoint timeout - moving to next");
                break;
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// State: Wait at waypoint
    /// </summary>
    IEnumerator WaitAtWaypointState()
    {
        if (waitTimeAtWaypoint > 0)
        {
            navMeshAgent.isStopped = true;
            LogDebug($"Waiting at waypoint for {waitTimeAtWaypoint}s");
            
            float waitEnd = Time.time + waitTimeAtWaypoint;
            while (Time.time < waitEnd)
            {
                if (CheckForPlayer())
                {
                    navMeshAgent.isStopped = false;
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            navMeshAgent.isStopped = false;
        }
    }

    /// <summary>
    /// State: Look around for player
    /// </summary>
    IEnumerator LookAroundState()
    {
        ChangeState(KamatayanAnimationState.Looking);
        navMeshAgent.isStopped = true;
        
        LogDebug("Looking around...");
        
        // Look in 4 directions
        float[] angles = { 0f, 90f, 180f, 270f };
        Quaternion startRotation = transform.rotation;
        
        foreach (float angle in angles)
        {
            Quaternion targetRotation = startRotation * Quaternion.Euler(0, angle, 0);
            
            // Rotate to target
            float rotateTime = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < rotateTime)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsed / rotateTime);
                elapsed += Time.deltaTime;
                
                // Check for player while looking
                if (CheckForPlayer())
                {
                    navMeshAgent.isStopped = false;
                    yield break;
                }
                
                yield return null;
            }
            
            // Hold and scan
            yield return new WaitForSeconds(0.5f);
        }
        
        navMeshAgent.isStopped = false;
        LogDebug("Finished looking around");
    }

    /// <summary>
    /// State: Chase the player
    /// </summary>
    IEnumerator ChaseState()
    {
        ChangeState(KamatayanAnimationState.Chasing);
        isChasing = true;
        chaseStartTime = Time.time;
        navMeshAgent.speed = chaseSpeed;
        navMeshAgent.stoppingDistance = attackRange; // Stop at attack range to prevent pushing
        navMeshAgent.isStopped = false;

        LogDebug("Started chasing player!");

        while (isChasing)
        {
            if (player == null)
            {
                isChasing = false;
                break;
            }

            // Check distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Update destination to player position only if not within attack range
            if (distanceToPlayer > attackRange)
            {
                lastKnownPlayerPosition = player.position;
                navMeshAgent.SetDestination(lastKnownPlayerPosition);
            }
            else
            {
                // Stop moving when within attack range
                navMeshAgent.isStopped = true;
            }

            // Check if should stop chasing
            float chaseTime = Time.time - chaseStartTime;

            if (distanceToPlayer > loseChaseDistance || chaseTime > chaseTimeout)
            {
                isChasing = false;
                LogDebug("Lost player, returning to patrol");
                break;
            }

            // Check if still detecting player or within attack range
            if (persistentChase || CheckForPlayer() || distanceToPlayer <= attackRange)
            {
                // Continue chasing or stay in attack range
            }
            else
            {
                isChasing = false;
                LogDebug("Player out of detection, returning to patrol");
                break;
            }

            // Resume movement if player moves out of attack range
            if (distanceToPlayer > attackRange && navMeshAgent.isStopped)
            {
                navMeshAgent.isStopped = false;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// State: Return to patrol after losing player
    /// </summary>
    IEnumerator ReturnToPatrolState()
    {
        LogDebug("Returning to patrol route");

        // Reset stopping distance for patrol
        navMeshAgent.stoppingDistance = waypointReachedDistance;
        navMeshAgent.isStopped = false;

        // Find nearest waypoint
        Vector3 nearestWaypoint = GetNearestWaypoint();
        navMeshAgent.SetDestination(nearestWaypoint);
        
        float timeout = 10f;
        float startTime = Time.time;
        
        while (Vector3.Distance(transform.position, nearestWaypoint) > waypointReachedDistance * 2f)
        {
            if (Time.time - startTime > timeout)
                break;
                
            yield return new WaitForSeconds(0.2f);
        }
        
        waypointsVisitedSinceLastLook = 0;
    }

    /// <summary>
    /// Check if player is detected
    /// </summary>
    bool CheckForPlayer()
    {
        if (player == null)
            return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If chasing and persistent mode
        if (isChasing && persistentChase)
        {
            return distanceToPlayer <= loseChaseDistance;
        }

        // Check distance
        if (distanceToPlayer > detectionRadius)
            return false;

        // Check field of view
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer > fieldOfViewAngle * 0.5f)
            return false;

        // Check line of sight
        if (requireLineOfSight)
        {
            Vector3 origin = transform.position + Vector3.up * 1.6f;
            Vector3 targetPos = player.position + Vector3.up * 1.0f;
            Vector3 direction = targetPos - origin;

            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distanceToPlayer, lineOfSightObstructionMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != player && !hit.transform.IsChildOf(player))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get next waypoint in sequence
    /// </summary>
    Vector3 GetNextWaypoint()
    {
        int waypointCount = GetWaypointCount();
        
        if (waypointCount == 0)
        {
            Debug.LogWarning($"[{name}] No waypoints available!");
            return transform.position;
        }

        if (useRandomPatrol)
        {
            currentWaypointIndex = Random.Range(0, waypointCount);
        }
        else
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypointCount;
        }

        return GetWaypointPosition(currentWaypointIndex);
    }

    /// <summary>
    /// Get nearest waypoint to current position
    /// </summary>
    Vector3 GetNearestWaypoint()
    {
        int waypointCount = GetWaypointCount();
        if (waypointCount == 0)
            return transform.position;

        Vector3 nearest = GetWaypointPosition(0);
        float nearestDist = Vector3.Distance(transform.position, nearest);
        int nearestIndex = 0;

        for (int i = 1; i < waypointCount; i++)
        {
            Vector3 wp = GetWaypointPosition(i);
            float dist = Vector3.Distance(transform.position, wp);
            
            if (dist < nearestDist)
            {
                nearest = wp;
                nearestDist = dist;
                nearestIndex = i;
            }
        }

        currentWaypointIndex = nearestIndex;
        return nearest;
    }

    Vector3 GetWaypointPosition(int index)
    {
        if (useGeneratedWaypoints)
        {
            if (index >= 0 && index < generatedWaypoints.Count)
                return generatedWaypoints[index];
        }
        else
        {
            if (patrolWaypoints != null && index >= 0 && index < patrolWaypoints.Length)
            {
                if (patrolWaypoints[index] != null)
                    return patrolWaypoints[index].position;
            }
        }
        
        return transform.position;
    }

    int GetWaypointCount()
    {
        if (useGeneratedWaypoints)
            return generatedWaypoints.Count;
        
        if (patrolWaypoints != null)
            return patrolWaypoints.Length;
            
        return 0;
    }

    void ChangeState(KamatayanAnimationState newState)
    {
        if (currentState == newState)
            return;

        LogDebug($"State: {currentState} → {newState}");
        currentState = newState;
        stateStartTime = Time.time;
        
        UpdateAnimationParameters();
    }

    void UpdateAnimationParameters()
    {
        if (animator == null)
            return;

        // Reset all
        SetAnimBool(animParamIsIdle, false);
        SetAnimBool(animParamIsWalking, false);
        SetAnimBool(animParamIsLooking, false);
        SetAnimBool(animParamIsChasing, false);

        // Set current state
        switch (currentState)
        {
            case KamatayanAnimationState.Idle:
                SetAnimBool(animParamIsIdle, true);
                break;
            case KamatayanAnimationState.Patrolling:
                SetAnimBool(animParamIsWalking, true);
                break;
            case KamatayanAnimationState.Looking:
                SetAnimBool(animParamIsLooking, true);
                break;
            case KamatayanAnimationState.Chasing:
                SetAnimBool(animParamIsChasing, true);
                break;
        }
    }

    void SetAnimBool(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName) && HasParameter(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    bool HasParameter(string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName))
            return false;

        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[{name}] {message}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (navMeshAgent == null)
            return;

        // Detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Chase distance
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, loseChaseDistance);

        // Draw waypoints
        if (useGeneratedWaypoints && generatedWaypoints.Count > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (var wp in generatedWaypoints)
            {
                Gizmos.DrawWireSphere(wp, 0.8f);
                Gizmos.DrawLine(transform.position, wp);
            }
        }
        else if (patrolWaypoints != null)
        {
            Gizmos.color = Color.green;
            foreach (var wp in patrolWaypoints)
            {
                if (wp != null)
                {
                    Gizmos.DrawWireSphere(wp.position, 1f);
                    Gizmos.DrawLine(transform.position, wp.position);
                }
            }
        }

        // Draw current path
        if (navMeshAgent.hasPath)
        {
            Gizmos.color = Color.red;
            Vector3[] corners = navMeshAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }

        // Field of view
        if (currentState == KamatayanAnimationState.Looking)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * transform.forward;

            Gizmos.DrawRay(transform.position, leftBoundary * detectionRadius);
            Gizmos.DrawRay(transform.position, rightBoundary * detectionRadius);
        }
    }

    // Public API
    public KamatayanAnimationState GetCurrentState() => currentState;
    public bool IsChasing() => isChasing;
    public void SetPatrolWaypoints(Transform[] waypoints) => patrolWaypoints = waypoints;
}


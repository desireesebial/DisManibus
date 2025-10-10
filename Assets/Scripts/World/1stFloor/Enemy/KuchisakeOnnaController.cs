using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class KuchisakeOnnaController : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float chaseTimeout = 30f;
    [SerializeField] private float escapeDistance = 20f;

    [Header("Question Settings")]
    [SerializeField] private float questionTimer = 5f;
    [SerializeField] private float chaseChanceOnYes = 0.7f; // 70% chance she chases even on "Yes"
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scissorsSnipSound;
    [SerializeField] private AudioClip questionVoiceClip;
    [SerializeField] private AudioClip deathScreamClip;
    [SerializeField] private AudioClip angerSound;
    [SerializeField] private AudioClip retreatLaughSound;
    [SerializeField] private float ambientScissorInterval = 5f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject maskObject; // Mask on face
    [SerializeField] private Material normalFaceMaterial;
    [SerializeField] private Material slitMouthMaterial;
    [SerializeField] private Renderer faceRenderer;

    [Header("References")]
    [SerializeField] private KuchisakeQuestionUI questionUI;
    [SerializeField] private Transform player;
    
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private float chaseTimer;
    private float ambientScissorTimer;
    private bool isActive = true;

    public enum EnemyState
    {
        Patrol,
        Question,
        Chase,
        Retreat,
        Disabled
    }

    private EnemyState currentState = EnemyState.Patrol;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (agent != null)
        {
            agent.speed = patrolSpeed;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        if (patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }

        ambientScissorTimer = ambientScissorInterval;
    }

    void Update()
    {
        if (!isActive || player == null) return;

        // Ambient scissor sounds
        ambientScissorTimer -= Time.deltaTime;
        if (ambientScissorTimer <= 0)
        {
            PlayAmbientScissors();
            ambientScissorTimer = ambientScissorInterval;
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Question:
                HandleQuestion();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Retreat:
                HandleRetreat();
                break;
        }
    }

    void HandlePatrol()
    {
        // Check if reached patrol point
        if (agent != null && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }

        // Check for player detection
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            // Check line of sight
            RaycastHit hit;
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
            {
                if (hit.transform == player)
                {
                    StartQuestionSequence();
                }
            }
        }
    }

    void HandleQuestion()
    {
        // Question UI handles the timer and player input
        // This is managed by the UI script
    }

    void HandleChase()
    {
        if (agent != null)
        {
            agent.SetDestination(player.position);
        }

        chaseTimer -= Time.deltaTime;
        
        // Check if caught player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < 2f)
        {
            CatchPlayer();
            return;
        }

        // Check if player escaped
        if (distanceToPlayer > escapeDistance || chaseTimer <= 0)
        {
            currentState = EnemyState.Retreat;
        }
    }

    void HandleRetreat()
    {
        // Teleport to random patrol point
        if (patrolPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, patrolPoints.Length);
            transform.position = patrolPoints[randomIndex].position;
            
            if (retreatLaughSound != null)
            {
                audioSource.PlayOneShot(retreatLaughSound);
            }
        }
        
        currentState = EnemyState.Patrol;
        if (agent != null)
        {
            agent.speed = patrolSpeed;
        }
        GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0 || agent == null) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void StartQuestionSequence()
    {
        currentState = EnemyState.Question;
        
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Play question voice
        if (questionVoiceClip != null)
        {
            audioSource.PlayOneShot(questionVoiceClip);
        }

        // Show question UI
        if (questionUI != null)
        {
            questionUI.ShowQuestion(questionTimer, this);
        }
    }

    public void OnPlayerAnswered(KuchisakeQuestionUI.Answer answer)
    {
        switch (answer)
        {
            case KuchisakeQuestionUI.Answer.Yes:
                // 70% chance she still chases, 30% she lets you go
                if (Random.value < chaseChanceOnYes)
                {
                    RemoveMask();
                    StartCoroutine(DelayedChase(1.5f));
                }
                else
                {
                    // She's pleased - retreat
                    currentState = EnemyState.Retreat;
                }
                break;

            case KuchisakeQuestionUI.Answer.No:
                // Immediate anger and chase
                if (angerSound != null)
                {
                    audioSource.PlayOneShot(angerSound);
                }
                RemoveMask();
                StartChase();
                break;

            case KuchisakeQuestionUI.Answer.Maybe:
                // Confusion - delayed chase
                RemoveMask();
                StartCoroutine(DelayedChase(2f));
                break;
        }
    }

    public void OnQuestionTimeout()
    {
        // Player didn't answer in time - instant death
        KillPlayer();
    }

    void RemoveMask()
    {
        if (maskObject != null)
        {
            maskObject.SetActive(false);
        }

        if (faceRenderer != null && slitMouthMaterial != null)
        {
            faceRenderer.material = slitMouthMaterial;
        }
    }

    IEnumerator DelayedChase(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartChase();
    }

    void StartChase()
    {
        currentState = EnemyState.Chase;
        chaseTimer = chaseTimeout;
        
        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
        }
    }

    void CatchPlayer()
    {
        KillPlayer();
    }

    void KillPlayer()
    {
        // Play death sounds and effects
        if (deathScreamClip != null)
        {
            audioSource.PlayOneShot(deathScreamClip);
        }

        if (scissorsSnipSound != null)
        {
            audioSource.PlayOneShot(scissorsSnipSound);
        }

        // Trigger player death
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Die();
        }
        else
        {
            Debug.LogWarning("PlayerHealth component not found! Make sure player has PlayerHealth script.");
        }

        // Disable enemy temporarily
        currentState = EnemyState.Disabled;
        if (agent != null)
        {
            agent.isStopped = true;
        }
    }

    void PlayAmbientScissors()
    {
        // Only play if player is somewhat close
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < detectionRange * 2 && scissorsSnipSound != null)
        {
            audioSource.PlayOneShot(scissorsSnipSound, 0.5f);
        }
    }

    public void ResetEnemy()
    {
        currentState = EnemyState.Patrol;
        isActive = true;
        
        if (maskObject != null)
        {
            maskObject.SetActive(true);
        }

        if (faceRenderer != null && normalFaceMaterial != null)
        {
            faceRenderer.material = normalFaceMaterial;
        }

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
        }

        GoToNextPatrolPoint();
    }

    // Visualization in editor
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw escape distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, escapeDistance);

        // Draw patrol points
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                    
                    // Draw line to next patrol point
                    int nextIndex = (i + 1) % patrolPoints.Length;
                    if (patrolPoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                    }
                }
            }
        }
    }
}


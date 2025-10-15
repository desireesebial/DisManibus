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

    [Header("First Encounter")]
    [SerializeField] private bool requireFirstEncounter = true;
    [SerializeField] private Transform firstEncounterPosition; // Where she sits initially
    [SerializeField] private float firstEncounterTriggerDistance = 5f; // Distance to trigger first encounter
    [SerializeField] private Animator animator; // For sitting/standing animations
    [SerializeField] private string sittingAnimationTrigger = "Sitting"; // Animation state name
    [SerializeField] private string standUpAnimationTrigger = "StandUp"; // Stand up animation
    [SerializeField] private float standUpDuration = 2f; // Time for stand up animation
    
    [Header("Movement Animations")]
    [SerializeField] private string walkAnimationParameter = "IsWalking"; // Walk animation parameter
    [SerializeField] private string runAnimationParameter = "IsRunning"; // Run animation parameter
    [SerializeField] private string idleAnimationParameter = "IsIdle"; // Idle animation parameter
    
    [Header("References")]
    [SerializeField] private KuchisakeQuestionUI questionUI;
    [SerializeField] private Transform player;
    
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private float chaseTimer;
    private float ambientScissorTimer;
    private bool isActive = true;
    private bool hasHadFirstEncounter = false;
    private bool isStandingUp = false;

    public enum EnemyState
    {
        WaitingFirstEncounter, // Sitting, waiting for player
        FirstEncounter,        // First question encounter
        StandingUp,           // Animation of standing up
        Patrol,
        Question,
        Chase,
        Retreat,
        Disabled
    }

    public enum AnimationState
    {
        Idle,
        Walking,
        Running
    }

    private EnemyState currentState = EnemyState.WaitingFirstEncounter;
    private AnimationState currentAnimationState = AnimationState.Idle;
    
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

        ambientScissorTimer = ambientScissorInterval;

        // Setup first encounter behavior
        if (requireFirstEncounter)
        {
            SetupFirstEncounter();
        }
        else
        {
            // Skip first encounter, start patrolling immediately
            hasHadFirstEncounter = true;
            currentState = EnemyState.Patrol;
            
            if (patrolPoints.Length > 0)
            {
                GoToNextPatrolPoint();
            }
        }
    }

    void SetupFirstEncounter()
    {
        hasHadFirstEncounter = false;
        currentState = EnemyState.WaitingFirstEncounter;
        
        // Position at first encounter location if set
        if (firstEncounterPosition != null)
        {
            transform.position = firstEncounterPosition.position;
            transform.rotation = firstEncounterPosition.rotation;
        }
        
        // Disable NavMesh agent during first encounter
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        // Play sitting animation if available
        if (animator != null && !string.IsNullOrEmpty(sittingAnimationTrigger))
        {
            animator.SetTrigger(sittingAnimationTrigger);
        }
    }

    void Update()
    {
        if (!isActive || player == null) return;

        // Ambient scissor sounds (only after first encounter)
        if (hasHadFirstEncounter)
        {
            ambientScissorTimer -= Time.deltaTime;
            if (ambientScissorTimer <= 0)
            {
                PlayAmbientScissors();
                ambientScissorTimer = ambientScissorInterval;
            }
        }

        switch (currentState)
        {
            case EnemyState.WaitingFirstEncounter:
                HandleWaitingForFirstEncounter();
                break;
            case EnemyState.FirstEncounter:
                HandleQuestion();
                break;
            case EnemyState.StandingUp:
                // Wait for stand up animation to complete
                break;
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

    void HandleWaitingForFirstEncounter()
    {
        // Check if player is close enough to trigger first encounter
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= firstEncounterTriggerDistance)
        {
            StartFirstEncounter();
        }
    }

    void HandlePatrol()
    {
        // Set walking animation if moving
        if (agent != null && agent.velocity.magnitude > 0.1f)
        {
            ChangeAnimationState(AnimationState.Walking);
        }
        else
        {
            ChangeAnimationState(AnimationState.Idle);
        }

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
        // Set idle animation while asking question
        ChangeAnimationState(AnimationState.Idle);
        
        // Question UI handles the timer and player input
        // This is managed by the UI script
    }

    void HandleChase()
    {
        if (agent != null)
        {
            agent.SetDestination(player.position);
        }

        // Set running animation during chase
        ChangeAnimationState(AnimationState.Running);

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
        // Set idle animation during retreat teleport
        ChangeAnimationState(AnimationState.Idle);
        
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

    void StartFirstEncounter()
    {
        currentState = EnemyState.FirstEncounter;
        
        // Slowly face the player (no sudden movements)
        Vector3 direction = (player.position - transform.position).normalized;
        StartCoroutine(SmoothLookAt(direction, 1.5f)); // Slow, eerie turn

        // Play question voice after a delay (more atmospheric)
        StartCoroutine(DelayedFirstQuestion());
    }

    IEnumerator SmoothLookAt(Vector3 direction, float duration)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = targetRotation;
    }

    IEnumerator DelayedFirstQuestion()
    {
        // Wait a moment for tension
        yield return new WaitForSeconds(1f);

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
        // Check if this is the first encounter
        if (!hasHadFirstEncounter)
        {
            HandleFirstEncounterAnswer(answer);
            return;
        }

        // Normal encounter behavior
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

    void HandleFirstEncounterAnswer(KuchisakeQuestionUI.Answer answer)
    {
        hasHadFirstEncounter = true;
        
        switch (answer)
        {
            case KuchisakeQuestionUI.Answer.Yes:
                // She seems pleased but reveals her true face
                StartCoroutine(FirstEncounterYesSequence());
                break;

            case KuchisakeQuestionUI.Answer.No:
                // She's offended - removes mask and stands
                StartCoroutine(FirstEncounterNoSequence());
                break;

            case KuchisakeQuestionUI.Answer.Maybe:
                // Confused but intrigued - stands up slowly
                StartCoroutine(FirstEncounterMaybeSequence());
                break;
        }
    }

    IEnumerator FirstEncounterYesSequence()
    {
        // She removes mask slowly to show slit mouth
        yield return new WaitForSeconds(1f);
        RemoveMask();
        
        // Brief pause for player to see
        yield return new WaitForSeconds(2f);
        
        // Stand up animation
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration);
        
        // Activate patrol behavior
        ActivatePatrolMode();
    }

    IEnumerator FirstEncounterNoSequence()
    {
        // Quick mask removal
        RemoveMask();
        
        if (angerSound != null)
        {
            audioSource.PlayOneShot(angerSound);
        }
        
        yield return new WaitForSeconds(1f);
        
        // Stand up faster
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration * 0.7f);
        
        // Immediately start chasing
        ActivatePatrolMode();
        StartChase();
    }

    IEnumerator FirstEncounterMaybeSequence()
    {
        // Tilts head (if animation available), then removes mask
        yield return new WaitForSeconds(1.5f);
        RemoveMask();
        
        yield return new WaitForSeconds(1.5f);
        
        // Stand up slowly
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration);
        
        // Activate patrol
        ActivatePatrolMode();
    }

    void BeginStandUp()
    {
        currentState = EnemyState.StandingUp;
        isStandingUp = true;
        
        if (animator != null && !string.IsNullOrEmpty(standUpAnimationTrigger))
        {
            animator.SetTrigger(standUpAnimationTrigger);
        }
    }

    void ActivatePatrolMode()
    {
        isStandingUp = false;
        currentState = EnemyState.Patrol;
        
        // Enable NavMesh agent
        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = patrolSpeed;
        }
        
        // Start patrolling
        if (patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
    }

    public void OnQuestionTimeout()
    {
        // Check if this is first encounter
        if (!hasHadFirstEncounter)
        {
            // First encounter timeout - she stands and becomes active threat
            hasHadFirstEncounter = true;
            RemoveMask();
            
            if (angerSound != null)
            {
                audioSource.PlayOneShot(angerSound);
            }
            
            StartCoroutine(TimeoutStandAndActivate());
        }
        else
        {
            // Normal encounter timeout - instant death
            KillPlayer();
        }
    }

    IEnumerator TimeoutStandAndActivate()
    {
        // Stand up menacingly
        BeginStandUp();
        yield return new WaitForSeconds(standUpDuration);
        
        // Start patrolling aggressively
        ActivatePatrolMode();
        
        // Player escaped this time, but she's now active
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
        currentAnimationState = AnimationState.Idle;
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

        // Reset animation to idle
        ChangeAnimationState(AnimationState.Idle);

        GoToNextPatrolPoint();
    }

    // Animation Methods
    void ChangeAnimationState(AnimationState newAnimationState)
    {
        if (currentAnimationState == newAnimationState)
            return;

        currentAnimationState = newAnimationState;
        UpdateAnimationParameters();
    }

    void UpdateAnimationParameters()
    {
        if (animator == null)
            return;

        // Reset all animation parameters
        SetAnimBool(idleAnimationParameter, false);
        SetAnimBool(walkAnimationParameter, false);
        SetAnimBool(runAnimationParameter, false);

        // Set current animation state
        switch (currentAnimationState)
        {
            case AnimationState.Idle:
                SetAnimBool(idleAnimationParameter, true);
                break;
            case AnimationState.Walking:
                SetAnimBool(walkAnimationParameter, true);
                break;
            case AnimationState.Running:
                SetAnimBool(runAnimationParameter, true);
                break;
        }
    }

    void SetAnimBool(string paramName, bool value)
    {
        if (animator != null && !string.IsNullOrEmpty(paramName) && HasAnimationParameter(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    bool HasAnimationParameter(string paramName)
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

    // Visualization in editor
    void OnDrawGizmosSelected()
    {
        // Draw first encounter trigger range
        if (requireFirstEncounter && firstEncounterPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firstEncounterPosition.position, firstEncounterTriggerDistance);
            
            // Draw line from first encounter to first patrol point
            if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(firstEncounterPosition.position, patrolPoints[0].position);
            }
        }

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


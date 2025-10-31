using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DullahanChaseSystem : MonoBehaviour
{
    [Header("Chase Settings")]
    public float maxChaseSpeed = 8f;
    public float minChaseSpeed = 3f;
    public float maxDetectionRange = 20f;
    public float minDetectionRange = 5f;
    public float intensityUpdateRate = 0.1f;
    public float attackRange = 2f; // Distance to stop from player (should match EnemyDamageController's attackRange)

    [Header("Speed Progression Per Cycle")]
    [Tooltip("If true, speed increases per cycle. If false, uses default chase speed.")]
    public bool enableCycleProgression = true;

    [Tooltip("Base speed for cycle calculations (uses minChaseSpeed by default)")]
    public float baseCycleSpeed = 3f;

    [Tooltip("Speed multipliers for each chase cycle (Cycle 1 uses index 0, Cycle 2 uses index 1, etc.)")]
    public float[] cycleSpeedMultipliers = new float[] { 1.0f, 1.2f, 1.5f, 2.0f, 2.5f, 3.0f };

    [SerializeField] private int currentCycleNumber = 0;
    
    [Header("Patrol Settings")]
    public float patrolSpeed = 2f;
    public float patrolRadius = 15f;
    public float patrolWaitTime = 3f;
    public bool useWaypointPatrol = false;
    public Transform[] patrolWaypoints;
    
    [Header("Dullahan References")]
    public Transform dullahanTransform;
    public NavMeshAgent dullahanAgent;
    public Animator dullahanAnimator;
    
    [Header("Player References")]
    public Transform playerTransform;
    public FirstPersonController playerController;
    
    [Header("Chase Intensity")]
    public float currentChaseIntensity = 0f;
    public float maxChaseIntensity = 1f;
    public float intensityDecayRate = 0.5f;
    public float intensityIncreaseRate = 1f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip chaseSound;
    public AudioClip patrolSound;
    public AudioClip detectionSound;
    
    [Header("Visual Effects")]
    public Light chaseLight;
    public ParticleSystem chaseParticles;
    public Material normalMaterial;
    public Material chaseMaterial;
    
    [Header("State Management")]
    public ChaseState currentState = ChaseState.Patrol;
    public bool isChasing = false;
    public bool isPatrolling = false;
    public bool playerDetected = false;
    
    // Private variables
    private Vector3 lastKnownPlayerPosition;
    private float lastDetectionTime;
    private float patrolTimer = 0f;
    private int currentWaypointIndex = 0;
    private Vector3 patrolCenter;
    private DullahanAudioManager audioManager;
    private DullahanChaseEventManager eventManager;
    private bool isEventControlled = false; // True when chase is controlled by event manager
    
    public enum ChaseState
    {
        Patrol,
        Chase,
        Search,
        Return
    }
    
    void Start()
    {
        InitializeComponents();
        SetupPatrol();
        StartPatrol();
    }
    
    void Update()
    {
        UpdateChaseIntensity();
        HandleStateMachine();
        UpdateVisualEffects();
    }
    
    void InitializeComponents()
    {
        // Get or create components
        if (dullahanTransform == null)
            dullahanTransform = transform;
            
        if (dullahanAgent == null)
            dullahanAgent = GetComponent<NavMeshAgent>();
            
        if (dullahanAnimator == null)
            dullahanAnimator = GetComponent<Animator>();
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerController = playerObj.GetComponent<FirstPersonController>();
        }
        
        // Find managers
        audioManager = FindObjectOfType<DullahanAudioManager>();
        eventManager = FindObjectOfType<DullahanChaseEventManager>();
        
        // Setup audio
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    void SetupPatrol()
    {
        patrolCenter = transform.position;
        
        if (useWaypointPatrol && patrolWaypoints.Length > 0)
        {
            // Use waypoints for patrol
            currentWaypointIndex = 0;
        }
        else
        {
            // Use radius-based patrol
            GenerateRandomPatrolPoint();
        }
    }
    
    void HandleStateMachine()
    {
        switch (currentState)
        {
            case ChaseState.Patrol:
                HandlePatrol();
                break;
            case ChaseState.Chase:
                HandleChase();
                break;
            case ChaseState.Search:
                HandleSearch();
                break;
            case ChaseState.Return:
                HandleReturn();
                break;
        }
    }
    
    void HandlePatrol()
    {
        if (!isPatrolling) return;
        
        if (useWaypointPatrol && patrolWaypoints.Length > 0)
        {
            // Waypoint patrol
            if (dullahanAgent.remainingDistance < 0.5f)
            {
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolWaitTime)
                {
                    MoveToNextWaypoint();
                    patrolTimer = 0f;
                }
            }
        }
        else
        {
            // Radius patrol
            if (dullahanAgent.remainingDistance < 0.5f)
            {
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolWaitTime)
                {
                    GenerateRandomPatrolPoint();
                    patrolTimer = 0f;
                }
            }
        }

        // Check for player detection (only if not event-controlled)
        if (!isEventControlled && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= maxDetectionRange)
            {
                StartChase();
            }
        }
    }
    
    void HandleChase()
    {
        if (!isChasing || playerTransform == null) return;

        // Update chase intensity based on distance
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float normalizedDistance = Mathf.Clamp01(1f - (distanceToPlayer / maxDetectionRange));

        currentChaseIntensity = Mathf.Lerp(currentChaseIntensity, normalizedDistance, intensityIncreaseRate * Time.deltaTime);

        // Set chase speed based on intensity
        float currentSpeed = Mathf.Lerp(minChaseSpeed, maxChaseSpeed, currentChaseIntensity);
        dullahanAgent.speed = currentSpeed;

        // Move towards player only if beyond attack range
        if (distanceToPlayer > attackRange)
        {
            dullahanAgent.SetDestination(playerTransform.position);
            dullahanAgent.isStopped = false;
        }
        else
        {
            // Stop moving when within attack range to prevent pushing
            dullahanAgent.isStopped = true;
        }

        lastKnownPlayerPosition = playerTransform.position;
        lastDetectionTime = Time.time;

        // Check if player is out of range
        if (distanceToPlayer > maxDetectionRange)
        {
            StartSearch();
        }
    }
    
    void HandleSearch()
    {
        // Move to last known player position
        if (Vector3.Distance(transform.position, lastKnownPlayerPosition) > 1f)
        {
            dullahanAgent.SetDestination(lastKnownPlayerPosition);
        }
        else
        {
            // Search around last known position
            Vector3 searchPoint = lastKnownPlayerPosition + Random.insideUnitSphere * 5f;
            searchPoint.y = lastKnownPlayerPosition.y;
            dullahanAgent.SetDestination(searchPoint);
        }
        
        // Check if player is detected again
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= maxDetectionRange)
            {
                StartChase();
                return;
            }
        }
        
        // Return to patrol after search time
        if (Time.time - lastDetectionTime > 10f)
        {
            StartPatrol();
        }
    }
    
    void HandleReturn()
    {
        // Return to patrol center
        if (Vector3.Distance(transform.position, patrolCenter) > 1f)
        {
            dullahanAgent.SetDestination(patrolCenter);
        }
        else
        {
            StartPatrol();
        }
    }
    
    void UpdateChaseIntensity()
    {
        if (currentState == ChaseState.Chase)
        {
            // Intensity increases during chase
            currentChaseIntensity = Mathf.Lerp(currentChaseIntensity, maxChaseIntensity, intensityIncreaseRate * Time.deltaTime);
        }
        else
        {
            // Intensity decays when not chasing
            currentChaseIntensity = Mathf.Lerp(currentChaseIntensity, 0f, intensityDecayRate * Time.deltaTime);
        }
        
        currentChaseIntensity = Mathf.Clamp01(currentChaseIntensity);
    }
    
    void UpdateVisualEffects()
    {
        // Update light intensity
        if (chaseLight != null)
        {
            chaseLight.intensity = Mathf.Lerp(0.5f, 2f, currentChaseIntensity);
            chaseLight.color = Color.Lerp(Color.white, Color.red, currentChaseIntensity);
        }
        
        // Update particle effects
        if (chaseParticles != null)
        {
            var emission = chaseParticles.emission;
            emission.rateOverTime = currentChaseIntensity * 50f;
        }
        
        // Update material
        if (chaseMaterial != null && normalMaterial != null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                // Switch between materials based on chase intensity
                if (currentChaseIntensity > 0.5f)
                {
                    renderer.material = chaseMaterial;
                }
                else
                {
                    renderer.material = normalMaterial;
                }
            }
        }
    }
    
    public void StartChase(int cycleNumber = 0)
    {
        // Allow re-initialization to update cycle number and speed settings

        currentCycleNumber = cycleNumber;

        // If cycle number provided or already chasing, this is event-controlled
        isEventControlled = (cycleNumber > 0 || currentState == ChaseState.Chase);

        Debug.Log($"[DullahanChaseSystem] Starting chase - Cycle #{cycleNumber + 1}, Event Controlled: {isEventControlled}");

        currentState = ChaseState.Chase;
        isChasing = true;
        isPatrolling = false;

        // Calculate speed based on cycle progression
        float targetSpeed = CalculateCycleSpeed(cycleNumber);
        dullahanAgent.speed = targetSpeed;

        // Set stopping distance to attack range to prevent pushing
        dullahanAgent.stoppingDistance = attackRange;
        dullahanAgent.isStopped = false; // Ensure agent starts moving

        Debug.Log($"[DullahanChaseSystem] Cycle {cycleNumber + 1} speed: {targetSpeed:F1} (Base: {baseCycleSpeed}, Max: {maxChaseSpeed})");

        // Play chase sound
        if (audioSource != null && chaseSound != null)
        {
            audioSource.clip = chaseSound;
            audioSource.Play();
        }

        // Notify event manager
        if (eventManager != null)
        {
            eventManager.OnChaseStarted();
        }
    }
    
    public void EndChase()
    {
        if (currentState != ChaseState.Chase) return;

        Debug.Log("[DullahanChaseSystem] Ending chase");

        isChasing = false;
        isEventControlled = false; // Reset event control flag

        // Stop audio
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        // Notify event manager
        if (eventManager != null)
        {
            eventManager.OnChaseEnded();
        }
    }
    
    public void StartPatrol()
    {
        Debug.Log("[DullahanChaseSystem] Starting patrol");

        currentState = ChaseState.Patrol;
        isPatrolling = true;
        isChasing = false;

        // Set patrol speed
        dullahanAgent.speed = patrolSpeed;

        // Reset stopping distance for patrol
        dullahanAgent.stoppingDistance = 0.5f;
        dullahanAgent.isStopped = false;

        // Play patrol sound
        if (audioSource != null && patrolSound != null)
        {
            audioSource.clip = patrolSound;
            audioSource.Play();
        }

        // Generate first patrol point
        if (useWaypointPatrol && patrolWaypoints.Length > 0)
        {
            MoveToNextWaypoint();
        }
        else
        {
            GenerateRandomPatrolPoint();
        }
    }
    
    public void StartSearch()
    {
        Debug.Log("[DullahanChaseSystem] Starting search");

        currentState = ChaseState.Search;
        isChasing = false;
        isPatrolling = false;

        // Set search speed
        dullahanAgent.speed = minChaseSpeed;

        // Reset stopping distance for search
        dullahanAgent.stoppingDistance = 1f;
        dullahanAgent.isStopped = false;
    }
    
    void MoveToNextWaypoint()
    {
        if (patrolWaypoints.Length == 0) return;
        
        Transform waypoint = patrolWaypoints[currentWaypointIndex];
        dullahanAgent.SetDestination(waypoint.position);
        
        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
    }
    
    void GenerateRandomPatrolPoint()
    {
        Vector3 randomPoint = patrolCenter + Random.insideUnitSphere * patrolRadius;
        randomPoint.y = patrolCenter.y;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, 1))
        {
            dullahanAgent.SetDestination(hit.position);
        }
    }
    
    public void SetChaseIntensity(float intensity)
    {
        currentChaseIntensity = Mathf.Clamp01(intensity);
    }
    
    public float GetChaseIntensity()
    {
        return currentChaseIntensity;
    }
    
    public bool IsChasing()
    {
        return isChasing;
    }
    
    public bool IsPatrolling()
    {
        return isPatrolling;
    }

    public int GetCurrentCycleNumber()
    {
        return currentCycleNumber;
    }

    private float CalculateCycleSpeed(int cycleNumber)
    {
        if (!enableCycleProgression)
        {
            // Disabled: use default max speed
            return maxChaseSpeed;
        }

        // Get multiplier for this cycle
        float multiplier = 1.0f;
        if (cycleNumber < cycleSpeedMultipliers.Length)
        {
            multiplier = cycleSpeedMultipliers[cycleNumber];
        }
        else
        {
            // If beyond array length, use last multiplier
            multiplier = cycleSpeedMultipliers[cycleSpeedMultipliers.Length - 1];
        }

        // Calculate speed: base speed * multiplier, capped at max
        float calculatedSpeed = baseCycleSpeed * multiplier;
        float finalSpeed = Mathf.Min(calculatedSpeed, maxChaseSpeed);

        return finalSpeed;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxDetectionRange);
        
        // Draw patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
        
        // Draw waypoints
        if (useWaypointPatrol && patrolWaypoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform waypoint in patrolWaypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawWireSphere(waypoint.position, 1f);
                }
            }
        }
    }
}
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
        
        // Check for player detection
        if (playerTransform != null)
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
        
        // Move towards player
        dullahanAgent.SetDestination(playerTransform.position);
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
    
    public void StartChase()
    {
        if (currentState == ChaseState.Chase) return;
        
        Debug.Log("[DullahanChaseSystem] Starting chase");
        
        currentState = ChaseState.Chase;
        isChasing = true;
        isPatrolling = false;
        
        // Set chase speed
        dullahanAgent.speed = maxChaseSpeed;
        
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
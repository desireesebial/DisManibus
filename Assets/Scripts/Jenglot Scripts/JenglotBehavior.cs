using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Jenglot Enemy Behavior System
/// - Activates when player enters the room (proximity detection)
/// - Follows the player when not being stared at
/// - Stops moving when player is looking directly at it
/// - Uses NavMesh for pathfinding
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class JenglotBehavior : MonoBehaviour
{
    [Header("Jenglot Settings")]
    [SerializeField] private float activationRange = 10f;
    [SerializeField] private float followSpeed = 2f;
    [SerializeField] private float stareDetectionAngle = 45f;
    [SerializeField] private float stareMinDistance = 1f;
    [SerializeField] private float stareMaxDistance = 15f;
    [SerializeField] private bool persistentFollowing = true;  // Once activated, follow player everywhere
    [SerializeField] private float deactivationRange = 100f;   // Only deactivate if player goes extremely far (when persistentFollowing = false)
    
    [Header("Movement Settings")]
    [SerializeField] private float stopDistance = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool canPassThroughDoors = false;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private AudioClip freezeSound;
    
    [Header("Visual Effects")]
    [SerializeField] private Animator animator;
    [SerializeField] private Renderer jenglotRenderer;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material frozenMaterial;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool enableDebugLogs = false;
    
    // Private variables
    private Transform player;
    private Camera playerCamera;
    private NavMeshAgent navAgent;
    private bool isActive = false;
    private bool isBeingStaredAt = false;
    private bool wasMovingLastFrame = false;
    private bool wasStaredAtLastFrame = false;
    private Vector3 lastKnownPlayerPosition;
    
    // Animation states
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int isFrozenHash = Animator.StringToHash("IsFrozen");
    
    private void Start()
    {
        InitializeComponents();
        SetupNavMeshAgent();
        DeactivateJenglot();
    }
    
    private void InitializeComponents()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCamera = player.GetComponentInChildren<Camera>();
            
            if (playerCamera == null)
            {
                Debug.LogWarning("JenglotBehavior: No camera found on player!");
            }
        }
        else
        {
            Debug.LogError("JenglotBehavior: No player found with 'Player' tag!");
        }
        
        // Get components
        navAgent = GetComponent<NavMeshAgent>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (jenglotRenderer == null)
            jenglotRenderer = GetComponentInChildren<Renderer>();

        // Debug message for animation support
        if (enableDebugLogs)
        {
            if (animator != null)
                Debug.Log("Jenglot: Animator found - using animation states");
            else
                Debug.Log("Jenglot: No animator found - using basic visual feedback");
        }
    }
    
    private void SetupNavMeshAgent()
    {
        if (navAgent != null)
        {
            navAgent.speed = followSpeed;
            navAgent.stoppingDistance = stopDistance;
            navAgent.angularSpeed = rotationSpeed * 60f; // Convert to degrees per second
            navAgent.acceleration = 8f;
            navAgent.radius = 0.5f;
            navAgent.height = 1.5f;
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }
    }
    
    private void Update()
    {
        if (player == null) return;
        
        CheckProximityActivation();
        
        if (isActive)
        {
            UpdateStareDetection();
            UpdateMovement();
            UpdateVisualEffects();
            UpdateAudio();
        }
    }
    
    private void CheckProximityActivation()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isActive && distanceToPlayer <= activationRange)
        {
            ActivateJenglot();
        }
        else if (isActive)
        {
            // Check if we should deactivate based on persistent following setting
            if (!persistentFollowing && distanceToPlayer > deactivationRange)
            {
                DeactivateJenglot();
            }
            // If persistentFollowing is true, never deactivate - keep following forever
        }
    }
    
    private void ActivateJenglot()
    {
        isActive = true;
        
        if (enableDebugLogs)
            Debug.Log(persistentFollowing ? "Jenglot activated - Persistent following mode enabled!" : "Jenglot activated - Player entered detection range!");
        
        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Start with normal material
        if (jenglotRenderer != null && normalMaterial != null)
        {
            jenglotRenderer.material = normalMaterial;
        }
    }
    
    private void DeactivateJenglot()
    {
        isActive = false;
        isBeingStaredAt = false;
        
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
        
        if (enableDebugLogs)
            Debug.Log(persistentFollowing ? "Jenglot deactivated - Persistent following disabled and player moved too far" : "Jenglot deactivated - Player left detection range");
    }
    
    private void UpdateStareDetection()
    {
        if (playerCamera == null) return;
        
        isBeingStaredAt = IsPlayerStaringAtJenglot();
        
        // Handle state changes
        if (isBeingStaredAt && !wasStaredAtLastFrame)
        {
            OnStartBeingStaredAt();
        }
        else if (!isBeingStaredAt && wasStaredAtLastFrame)
        {
            OnStopBeingStaredAt();
        }
        
        wasStaredAtLastFrame = isBeingStaredAt;
    }
    
    private bool IsPlayerStaringAtJenglot()
    {
        Vector3 directionToJenglot = (transform.position - playerCamera.transform.position).normalized;
        Vector3 cameraForward = playerCamera.transform.forward;
        
        // Check angle between camera forward and direction to Jenglot
        float angle = Vector3.Angle(cameraForward, directionToJenglot);
        
        // Check distance
        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        
        // Player is staring if:
        // 1. Jenglot is within the detection angle
        // 2. Jenglot is within the stare distance range
        // 3. There's a clear line of sight (no obstacles)
        bool withinAngle = angle <= stareDetectionAngle;
        bool withinDistance = distance >= stareMinDistance && distance <= stareMaxDistance;
        bool clearLineOfSight = HasClearLineOfSight();
        
        if (enableDebugLogs && withinAngle && withinDistance)
        {
            Debug.Log($"Jenglot stare check - Angle: {angle:F1}°, Distance: {distance:F1}m, Clear LOS: {clearLineOfSight}");
        }
        
        return withinAngle && withinDistance && clearLineOfSight;
    }
    
    private bool HasClearLineOfSight()
    {
        Vector3 rayStart = playerCamera.transform.position;
        Vector3 rayDirection = (transform.position - rayStart).normalized;
        float rayDistance = Vector3.Distance(rayStart, transform.position);
        
        // Raycast to check for obstacles
        RaycastHit hit;
        if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance))
        {
            // Check if we hit the Jenglot or something else
            return hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform);
        }
        
        return true; // No obstacles found
    }
    
    private void OnStartBeingStaredAt()
    {
        if (enableDebugLogs)
            Debug.Log("Jenglot is being stared at - Freezing!");
        
        // Stop movement
        if (navAgent != null)
        {
            navAgent.isStopped = true;
        }
        
        // Play freeze sound
        if (audioSource != null && freezeSound != null)
        {
            audioSource.PlayOneShot(freezeSound);
        }
        
        // Change material to frozen
        if (jenglotRenderer != null && frozenMaterial != null)
        {
            jenglotRenderer.material = frozenMaterial;
        }
    }
    
    private void OnStopBeingStaredAt()
    {
        if (enableDebugLogs)
            Debug.Log("Jenglot is no longer being stared at - Resuming movement!");
        
        // Resume movement
        if (navAgent != null)
        {
            navAgent.isStopped = false;
        }
        
        // Change material back to normal
        if (jenglotRenderer != null && normalMaterial != null)
        {
            jenglotRenderer.material = normalMaterial;
        }
    }
    
    private void UpdateMovement()
    {
        if (navAgent == null || isBeingStaredAt) return;
        
        // Update last known player position
        lastKnownPlayerPosition = player.position;
        
        // Set destination to player position
        if (!navAgent.isStopped)
        {
            navAgent.SetDestination(lastKnownPlayerPosition);
        }
        
        // Check if moving
        bool isMoving = navAgent.velocity.magnitude > 0.1f && !navAgent.isStopped;
        
        // Handle movement state changes
        if (isMoving && !wasMovingLastFrame)
        {
            OnStartMoving();
        }
        else if (!isMoving && wasMovingLastFrame)
        {
            OnStopMoving();
        }
        
        wasMovingLastFrame = isMoving;
    }
    
    private void OnStartMoving()
    {
        if (enableDebugLogs)
            Debug.Log("Jenglot started moving towards player");
    }
    
    private void OnStopMoving()
    {
        if (enableDebugLogs)
            Debug.Log("Jenglot stopped moving");
    }
    
    private void UpdateVisualEffects()
    {
        // Update animator parameters (if animator exists)
        if (animator != null)
        {
            bool isMoving = navAgent != null && navAgent.velocity.magnitude > 0.1f && !navAgent.isStopped;
            animator.SetBool(isMovingHash, isMoving && !isBeingStaredAt);
            animator.SetBool(isFrozenHash, isBeingStaredAt);
        }
        else
        {
            // Fallback: Use basic visual feedback without animations
            if (jenglotRenderer != null)
            {
                // Simple color change based on state
                if (isBeingStaredAt)
                {
                    // Frozen state - use frozen material if available
                    if (frozenMaterial != null)
                        jenglotRenderer.material = frozenMaterial;
                    else
                        jenglotRenderer.material.color = Color.cyan; // Default frozen color
                }
                else
                {
                    // Normal state - use normal material if available
                    if (normalMaterial != null)
                        jenglotRenderer.material = normalMaterial;
                    else
                        jenglotRenderer.material.color = Color.white; // Default normal color
                }
            }
        }
    }
    
    private void UpdateAudio()
    {
        if (audioSource == null || movementSound == null) return;
        
        bool shouldPlayMovementSound = navAgent != null && navAgent.velocity.magnitude > 0.1f && 
                                       !navAgent.isStopped && !isBeingStaredAt;
        
        if (shouldPlayMovementSound && !audioSource.isPlaying)
        {
            audioSource.clip = movementSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (!shouldPlayMovementSound && audioSource.isPlaying && audioSource.clip == movementSound)
        {
            audioSource.Stop();
        }
    }
    
    // Public methods for external control
    public void ForceActivate()
    {
        ActivateJenglot();
    }
    
    public void ForceDeactivate()
    {
        DeactivateJenglot();
    }
    
    public void SetFollowSpeed(float newSpeed)
    {
        followSpeed = newSpeed;
        if (navAgent != null)
        {
            navAgent.speed = followSpeed;
        }
    }
    
    public bool IsCurrentlyActive => isActive;
    public bool IsCurrentlyFrozen => isBeingStaredAt;
    public float DistanceToPlayer => player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;
    
    // Custom function to draw wire circles (since Unity doesn't have Gizmos.DrawWireCircle)
    private void DrawWireCircle(Vector3 center, float radius, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 previousPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 currentPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw activation range
        Gizmos.color = isActive ? Color.green : Color.yellow;
        DrawWireCircle(transform.position, activationRange);

        // Draw stare detection cone
        if (playerCamera != null && isActive)
        {
            Gizmos.color = isBeingStaredAt ? Color.red : Color.blue;
            Vector3 cameraPos = playerCamera.transform.position;
            Vector3 directionToJenglot = (transform.position - cameraPos).normalized;

            // Draw line from camera to Jenglot
            Gizmos.DrawLine(cameraPos, transform.position);

            // Draw stare detection range
            Gizmos.color = Color.cyan;
            DrawWireCircle(transform.position, stareMaxDistance);
            DrawWireCircle(transform.position, stareMinDistance);
        }

        // Draw current destination
        if (navAgent != null && navAgent.hasPath)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, navAgent.destination);
        }
    }
}

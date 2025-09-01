using UnityEngine;

/// <summary>
/// Room Trigger System for Jenglot
/// Provides more precise room-based detection using trigger colliders
/// Can be used in conjunction with or instead of the distance-based detection in JenglotBehavior
/// </summary>
[RequireComponent(typeof(Collider))]
public class JenglotRoomTrigger : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private JenglotBehavior jenglotBehavior;
    [SerializeField] private bool autoFindJenglot = true;
    [SerializeField] private string jenglotTag = "Jenglot";
    
    [Header("Activation Settings")]
    [SerializeField] private bool activateOnEnter = true;
    [SerializeField] private bool deactivateOnExit = true;
    [SerializeField] private float deactivationDelay = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip roomEnterSound;
    [SerializeField] private AudioClip roomExitSound;
    
    [Header("Visual Effects")]
    [SerializeField] private Light[] roomLights;
    [SerializeField] private Color normalLightColor = Color.white;
    [SerializeField] private Color dangerLightColor = Color.red;
    [SerializeField] private float lightTransitionSpeed = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool showGizmos = true;
    
    private bool playerInRoom = false;
    private Coroutine deactivationCoroutine;
    private Color targetLightColor;
    
    private void Start()
    {
        InitializeComponents();
        SetupTrigger();
        SetInitialLightColor();
    }
    
    private void InitializeComponents()
    {
        // Auto-find Jenglot if not assigned
        if (jenglotBehavior == null && autoFindJenglot)
        {
            GameObject jenglotObj = GameObject.FindGameObjectWithTag(jenglotTag);
            if (jenglotObj != null)
            {
                jenglotBehavior = jenglotObj.GetComponent<JenglotBehavior>();
                if (enableDebugLogs)
                    Debug.Log($"JenglotRoomTrigger: Auto-found Jenglot at {jenglotObj.name}");
            }
            else
            {
                Debug.LogWarning($"JenglotRoomTrigger: No GameObject found with tag '{jenglotTag}'");
            }
        }
        
        // Get audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Set initial target color
        targetLightColor = normalLightColor;
    }
    
    private void SetupTrigger()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
            if (enableDebugLogs)
                Debug.Log("JenglotRoomTrigger: Trigger collider configured");
        }
        else
        {
            Debug.LogError("JenglotRoomTrigger: No collider found! Add a collider component.");
        }
    }
    
    private void SetInitialLightColor()
    {
        if (roomLights != null)
        {
            foreach (Light light in roomLights)
            {
                if (light != null)
                    light.color = normalLightColor;
            }
        }
    }
    
    private void Update()
    {
        UpdateLightColors();
    }
    
    private void UpdateLightColors()
    {
        if (roomLights == null) return;
        
        foreach (Light light in roomLights)
        {
            if (light != null)
            {
                light.color = Color.Lerp(light.color, targetLightColor, Time.deltaTime * lightTransitionSpeed);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnterRoom();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExitRoom();
        }
    }
    
    private void OnPlayerEnterRoom()
    {
        playerInRoom = true;
        
        if (enableDebugLogs)
            Debug.Log("Player entered Jenglot room!");
        
        // Cancel any pending deactivation
        if (deactivationCoroutine != null)
        {
            StopCoroutine(deactivationCoroutine);
            deactivationCoroutine = null;
        }
        
        // Activate Jenglot
        if (activateOnEnter && jenglotBehavior != null)
        {
            jenglotBehavior.ForceActivate();
        }
        
        // Play sound
        if (audioSource != null && roomEnterSound != null)
        {
            audioSource.PlayOneShot(roomEnterSound);
        }
        
        // Change lights to danger color
        targetLightColor = dangerLightColor;
        
        // Trigger any additional events
        OnRoomEntered();
    }
    
    private void OnPlayerExitRoom()
    {
        playerInRoom = false;
        
        if (enableDebugLogs)
            Debug.Log("Player left Jenglot room!");
        
        // Play sound
        if (audioSource != null && roomExitSound != null)
        {
            audioSource.PlayOneShot(roomExitSound);
        }
        
        // Change lights back to normal
        targetLightColor = normalLightColor;
        
        // Deactivate Jenglot with delay (only if persistent following is disabled)
        if (deactivateOnExit && jenglotBehavior != null)
        {
            // Check if Jenglot has persistent following enabled
            // If persistent following is enabled, don't deactivate when leaving room
            if (!HasPersistentFollowing() || !deactivateOnExit)
            {
                if (deactivationDelay > 0f)
                {
                    deactivationCoroutine = StartCoroutine(DeactivateJenglotWithDelay());
                }
                else
                {
                    jenglotBehavior.ForceDeactivate();
                }
            }
            else if (enableDebugLogs)
            {
                Debug.Log("Jenglot persistent following enabled - staying active even after leaving room");
            }
        }
        
        // Trigger any additional events
        OnRoomExited();
    }
    
    private System.Collections.IEnumerator DeactivateJenglotWithDelay()
    {
        yield return new WaitForSeconds(deactivationDelay);
        
        // Only deactivate if player is still not in room
        if (!playerInRoom && jenglotBehavior != null)
        {
            jenglotBehavior.ForceDeactivate();
            if (enableDebugLogs)
                Debug.Log("Jenglot deactivated after delay");
        }
        
        deactivationCoroutine = null;
    }
    
    // Check if Jenglot has persistent following enabled
    private bool HasPersistentFollowing()
    {
        if (jenglotBehavior == null) return false;

        // Use reflection to access the persistentFollowing field
        var field = jenglotBehavior.GetType().GetField("persistentFollowing",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (bool)field.GetValue(jenglotBehavior);
        }

        return false; // Default to false if we can't access the field
    }

    // Virtual methods for inheritance
    protected virtual void OnRoomEntered()
    {
        // Override in derived classes for custom behavior
    }

    protected virtual void OnRoomExited()
    {
        // Override in derived classes for custom behavior
    }
    
    // Public properties
    public bool IsPlayerInRoom => playerInRoom;
    public JenglotBehavior AssociatedJenglot => jenglotBehavior;
    
    // Public methods
    public void SetJenglotBehavior(JenglotBehavior jenglot)
    {
        jenglotBehavior = jenglot;
    }
    
    public void ForcePlayerEnter()
    {
        OnPlayerEnterRoom();
    }
    
    public void ForcePlayerExit()
    {
        OnPlayerExitRoom();
    }
    
    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            Gizmos.color = playerInRoom ? Color.red : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (trigger is BoxCollider box)
            {
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (trigger is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else if (trigger is CapsuleCollider capsule)
            {
                // Approximate capsule with sphere for simplicity
                Gizmos.DrawWireSphere(capsule.center, capsule.radius);
            }
        }
        
        // Draw connection to Jenglot
        if (jenglotBehavior != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, jenglotBehavior.transform.position);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw detailed room boundary when selected
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (trigger is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (trigger is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
    }
}

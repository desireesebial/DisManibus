using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("The FlashlightController to unlock when picked up. If null, will search for one in the scene.")]
    public FlashlightController flashlightController;
    
    [Tooltip("Trigger-based pickup (player walks into it) or require interaction key press?")]
    public bool autoPickupOnTrigger = true;
    
    [Tooltip("Key to press to pick up if autoPickupOnTrigger is false.")]
    public KeyCode pickupKey = KeyCode.E;
    
    [Tooltip("Tag of the player GameObject. Used to detect when player is near.")]
    public string playerTag = "Player";
    
    [Header("Visual Feedback")]
    [Tooltip("Optional prompt text to display when player is near (requires UI setup).")]
    public string pickupPrompt = "Press E to pick up Flashlight";
    
    [Tooltip("Optional visual object to disable/destroy after pickup (e.g., the flashlight model).")]
    public GameObject visualObject;
    
    [Tooltip("If true, destroy the entire pickup GameObject after collection. Otherwise, just disable it.")]
    public bool destroyOnPickup = true;
    
    [Header("Audio")]
    [Tooltip("Sound to play when the flashlight is picked up.")]
    public AudioClip pickupSound;
    
    [Header("Effects")]
    [Tooltip("Optional particle effect to spawn when picked up.")]
    public GameObject pickupEffect;
    
    [Header("Debug")]
    [Tooltip("Enable detailed debug logging for troubleshooting")]
    public bool enableDebugLogs = false;
    
    // Private variables
    private bool isPickedUp = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private AudioSource audioSource;

    private void Start()
    {
        InitializePickup();
    }

    private void Update()
    {
        if (isPickedUp) return;

        // Manual pickup mode: check for pickup key press when player is in range
        if (!autoPickupOnTrigger && playerInRange && Input.GetKeyDown(pickupKey))
        {
            ExecutePickup();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Always log trigger events for debugging
        Debug.Log($"[FlashlightPickup] OnTriggerEnter called! Object: '{other.gameObject.name}', Tag: '{other.tag}'");
        
        if (isPickedUp)
        {
            Debug.Log($"[FlashlightPickup] Already picked up, ignoring.");
            return;
        }
        
        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[FlashlightPickup] Not the player. Expected tag: '{playerTag}', Got: '{other.tag}'");
            return;
        }

        Debug.Log($"[FlashlightPickup] ✓ PLAYER DETECTED! Executing pickup...");
        playerTransform = other.transform;
        playerInRange = true;

        if (autoPickupOnTrigger)
        {
            ExecutePickup();
        }
        else
        {
            Debug.Log($"[FlashlightPickup] {pickupPrompt}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[FlashlightPickup] OnTriggerExit called! Object: '{other.gameObject.name}'");
        
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"[FlashlightPickup] Player exited trigger area");
            playerInRange = false;
            playerTransform = null;
        }
    }

    /// <summary>
    /// Initialize the pickup system and validate all required components
    /// </summary>
    private void InitializePickup()
    {
        Debug.Log($"[FlashlightPickup - {name}] ===== INITIALIZING =====");
        Debug.Log($"[FlashlightPickup - {name}] Position: {transform.position}");
        Debug.Log($"[FlashlightPickup - {name}] Layer: {LayerMask.LayerToName(gameObject.layer)}");

        // Find FlashlightController if not assigned
        if (flashlightController == null)
        {
            flashlightController = FindAnyObjectByType<FlashlightController>();
            if (flashlightController == null)
            {
                Debug.LogError($"[{name}] CRITICAL ERROR: No FlashlightController found in scene! Pickup will NOT work. " +
                    $"Make sure you have a GameObject with FlashlightController component in your scene.");
                return;
            }
            Debug.Log($"[FlashlightPickup - {name}] ✓ Auto-found FlashlightController: {flashlightController.name}");
        }
        else
        {
            Debug.Log($"[FlashlightPickup - {name}] ✓ FlashlightController assigned: {flashlightController.name}");
        }

        // Validate the FlashlightController is set to require pickup
        if (!flashlightController.requirePickup)
        {
            Debug.LogWarning($"[{name}] FlashlightController.requirePickup is FALSE. " +
                $"The flashlight can be used without picking up this object. " +
                $"Set requirePickup to TRUE on FlashlightController if you want to enforce the pickup.");
        }

        // Setup audio source
        SetupAudioSource();

        // Validate components
        ValidateComponents();

        Debug.Log($"[FlashlightPickup - {name}] Initialization complete - Auto-pickup: {autoPickupOnTrigger}, Pickup key: {pickupKey}");
        Debug.Log($"[FlashlightPickup - {name}] ===== READY - Walk into the trigger area to test =====");
    }

    /// <summary>
    /// Setup audio source for pickup sound
    /// </summary>
    private void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
    }

    /// <summary>
    /// Validate that all required components are present
    /// </summary>
    private void ValidateComponents()
    {
        Debug.Log($"[FlashlightPickup - {name}] --- Validating Components ---");
        
        // Check for collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[{name}] CRITICAL ERROR: No Collider found! Add a Collider and set 'Is Trigger' to TRUE. Pickup will NOT work!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"[{name}] CRITICAL ERROR: Collider 'Is Trigger' is FALSE! Set it to TRUE in the Inspector. Pickup will NOT work!");
        }
        else
        {
            Debug.Log($"[FlashlightPickup - {name}] ✓ Collider OK: {col.GetType().Name}, IsTrigger: {col.isTrigger}");
            if (col is SphereCollider sphere)
                Debug.Log($"  - Sphere radius: {sphere.radius}");
            else if (col is BoxCollider box)
                Debug.Log($"  - Box size: {box.size}");
        }

        // Check for Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"[{name}] CRITICAL ERROR: No Rigidbody found! Add a Rigidbody (set to Kinematic). Pickup will NOT work!");
        }
        else
        {
            Debug.Log($"[FlashlightPickup - {name}] ✓ Rigidbody OK: Kinematic={rb.isKinematic}, UseGravity={rb.useGravity}");
        }

        // Check for player
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError($"[{name}] CRITICAL ERROR: No GameObject with tag '{playerTag}' found! " +
                $"Make sure your player has the '{playerTag}' tag. Pickup will NOT work!");
        }
        else
        {
            Debug.Log($"[FlashlightPickup - {name}] ✓ Player found: {player.name}, Layer: {LayerMask.LayerToName(player.layer)}");
            
            // Check player collider
            Collider playerCol = player.GetComponent<Collider>();
            CharacterController charController = player.GetComponent<CharacterController>();
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            
            if (playerCol != null)
                Debug.Log($"  - Player has Collider: {playerCol.GetType().Name}");
            if (charController != null)
                Debug.Log($"  - Player has CharacterController");
            if (playerRb != null)
                Debug.Log($"  - Player has Rigidbody: Kinematic={playerRb.isKinematic}");
            
            if (playerCol == null && charController == null && playerRb == null)
                Debug.LogWarning($"  - WARNING: Player has no Collider, CharacterController, or Rigidbody!");
                
            // Check layer collision
            int pickupLayer = gameObject.layer;
            int playerLayer = player.layer;
            if (!Physics.GetIgnoreLayerCollision(pickupLayer, playerLayer))
            {
                Debug.Log($"  - ✓ Layers can collide: Pickup({LayerMask.LayerToName(pickupLayer)}) <-> Player({LayerMask.LayerToName(playerLayer)})");
            }
            else
            {
                Debug.LogError($"  - ✗ LAYER COLLISION BLOCKED! Pickup layer '{LayerMask.LayerToName(pickupLayer)}' cannot collide with Player layer '{LayerMask.LayerToName(playerLayer)}'!");
                Debug.LogError($"    Fix: Edit → Project Settings → Physics → Layer Collision Matrix");
            }
        }
    }

    /// <summary>
    /// Execute the pickup sequence - unlocks the flashlight controller
    /// </summary>
    private void ExecutePickup()
    {
        if (isPickedUp) return;

        Debug.Log($"[FlashlightPickup] ========== EXECUTING PICKUP ==========");

        // Validate flashlight controller
        if (flashlightController == null)
        {
            Debug.LogError($"[{name}] Cannot pickup flashlight - no FlashlightController assigned!");
            return;
        }

        isPickedUp = true;

        // Unlock the flashlight controller
        flashlightController.PickupFlashlight();
        Debug.Log($"[FlashlightPickup] ✓ FlashlightController unlocked - Player can now use flashlight with key '{flashlightController.flashlightKey}'!");

        // Play effects
        PlayPickupEffects();

        // Cleanup
        CleanupPickup();
        
        Debug.Log($"[FlashlightPickup] ========== PICKUP COMPLETE ==========");
    }

    /// <summary>
    /// Play all pickup effects (sound, particles, etc.)
    /// </summary>
    private void PlayPickupEffects()
    {
        // Play pickup sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Spawn particle effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Cleanup the pickup object after collection
    /// </summary>
    private void CleanupPickup()
    {
        // Hide visual
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }

        // Disable this script
        this.enabled = false;

        // Destroy or disable GameObject
        if (destroyOnPickup)
        {
            float delay = (audioSource != null && pickupSound != null) ? pickupSound.length : 0f;
            Destroy(gameObject, delay);
        }
        else
        {
            gameObject.SetActive(false);
        }

        DebugLog($"Flashlight picked up successfully!");
    }

    /// <summary>
    /// Public method to manually trigger pickup (e.g., from another script or interaction system)
    /// </summary>
    public void TriggerPickup()
    {
        ExecutePickup();
    }

    /// <summary>
    /// Log debug messages only if debug logging is enabled
    /// </summary>
    private void DebugLog(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[FlashlightPickup - {name}] {message}");
        }
    }

    /// <summary>
    /// Draw gizmo for pickup radius in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw trigger radius for visualization
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            if (col is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position, sphere.radius);
            }
            else if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
    }
}


using UnityEngine;

public class FlashlightPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("The FlashlightController to unlock when picked up. If null, will search for one in the scene.")]
    public FlashlightController flashlightController;
    
    [Tooltip("Optional: The GameObject containing the flashlight functionality to enable on pickup. Leave null if flashlight is already active.")]
    public GameObject flashlightGameObject;
    
    [Tooltip("Trigger-based pickup (player walks into it) or require interaction key press?")]
    public bool autoPickupOnTrigger = true;
    
    [Tooltip("Key to press to pick up if autoPickupOnTrigger is false.")]
    public KeyCode pickupKey = KeyCode.E;
    
    [Tooltip("Radius within which the player can pick up the flashlight (used for both trigger and manual pickup).")]
    public float pickupRadius = 2f;
    
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
    
    // Private variables
    private bool isPickedUp = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private AudioSource audioSource;

    private void Start()
    {
        // Find FlashlightController if not assigned
        if (flashlightController == null)
        {
            flashlightController = FindAnyObjectByType<FlashlightController>();
            if (flashlightController == null)
            {
                Debug.LogWarning($"[{name}] FlashlightPickup could not find a FlashlightController in the scene. Assign one manually.");
            }
        }

        // Setup audio source for pickup sound
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }

        // Ensure we have a collider for trigger detection
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = pickupRadius;
            Debug.Log($"[{name}] Added SphereCollider for pickup detection.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"[{name}] Collider exists but isTrigger is false. Set it to true for pickup detection.");
        }
    }

    private void Update()
    {
        if (isPickedUp) return;

        // Manual pickup mode: check for pickup key press when player is in range
        if (!autoPickupOnTrigger && playerInRange)
        {
            if (Input.GetKeyDown(pickupKey))
            {
                Pickup();
            }
        }

        // Optional: Check distance manually if not using triggers
        if (!autoPickupOnTrigger && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = distance <= pickupRadius;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickedUp) return;

        // Check if the player entered the trigger
        if (other.CompareTag(playerTag))
        {
            playerTransform = other.transform;
            playerInRange = true;

            // Auto pickup if enabled
            if (autoPickupOnTrigger)
            {
                Pickup();
            }
            else
            {
                // Show pickup prompt (you can implement UI display here)
                Debug.Log(pickupPrompt);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            playerTransform = null;
        }
    }

    private void Pickup()
    {
        if (isPickedUp) return;

        // Check if we have a flashlight controller to notify
        if (flashlightController == null)
        {
            Debug.LogError($"[{name}] Cannot pickup flashlight - no FlashlightController assigned!");
            return;
        }

        isPickedUp = true;

        // Enable the flashlight GameObject if assigned (this activates the flashlight functionality)
        if (flashlightGameObject != null)
        {
            flashlightGameObject.SetActive(true);
            Debug.Log($"Flashlight GameObject '{flashlightGameObject.name}' enabled!");
        }

        // Notify the flashlight controller
        flashlightController.PickupFlashlight();

        // Play pickup sound
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Spawn pickup effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Hide or destroy visual
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }

        // Disable this pickup script component to prevent further pickups
        this.enabled = false;

        // Destroy or disable this pickup object
        if (destroyOnPickup)
        {
            // Delay destruction slightly if we need to play the sound
            if (audioSource != null && pickupSound != null)
            {
                Destroy(gameObject, pickupSound.length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }

        Debug.Log("Flashlight picked up!");
    }

    // Public method to manually trigger pickup (e.g., from another script or interaction system)
    public void TriggerPickup()
    {
        Pickup();
    }

    // For debugging in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}


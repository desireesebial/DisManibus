using UnityEngine;
using UnityEngine.Events;

public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorPivot; // The pivot point for door rotation
    public float openAngle = 90f; // How far the door opens (in degrees)
    public bool isOpen = false; // Current state of the door
    
    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public string interactionPrompt = "Press E to open/close door";
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    
    [Header("Events")]
    public UnityEvent onDoorOpened;
    public UnityEvent onDoorClosed;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private AudioSource audioSource;
    private Transform player;
    private SimplePlayerMovement playerMovement;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool playerFound = false;
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find player
        FindPlayer();
        
        // Setup door rotations
        if (doorPivot == null)
        {
            // If no pivot is assigned, use this transform
            doorPivot = transform;
            if (showDebugInfo)
                Debug.LogWarning("DoorInteraction: No door pivot assigned, using this transform. Door may not rotate correctly!");
        }
        
        // Store the closed rotation (current rotation)
        closedRotation = doorPivot.localRotation;
        
        // Calculate the open rotation
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        
        // Set initial state
        if (isOpen)
        {
            doorPivot.localRotation = openRotation;
        }
        else
        {
            doorPivot.localRotation = closedRotation;
        }
        
        // Check if door has collider
        CheckDoorCollider();
        
        if (showDebugInfo)
            Debug.Log("DoorInteraction initialized on: " + gameObject.name);
    }
    
    void FindPlayer()
    {
        // Try to find player by tag first
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<SimplePlayerMovement>();
            playerFound = true;
            if (showDebugInfo)
                Debug.Log("DoorInteraction: Player found by tag: " + player.name);
        }
        else
        {
            // Try to find by name
            playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerMovement = playerObj.GetComponent<SimplePlayerMovement>();
                playerFound = true;
                if (showDebugInfo)
                    Debug.LogWarning("DoorInteraction: Player found by name but missing 'Player' tag. Please add the 'Player' tag to your player GameObject!");
            }
            else
            {
                if (showDebugInfo)
                    Debug.LogError("DoorInteraction: No player found! Make sure your player GameObject has the 'Player' tag or is named 'Player'.");
            }
        }
    }
    
    void CheckDoorCollider()
    {
        // Check if door has a collider for blocking player movement
        Collider doorCollider = GetComponent<Collider>();
        if (doorCollider == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("DoorInteraction: Door has no Collider! Player can walk through the door. Add a Box Collider component.");
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("DoorInteraction: Door has collider: " + doorCollider.GetType().Name);
        }
    }
    
    void Update()
    {
        // Try to find player if not found yet
        if (!playerFound)
        {
            FindPlayer();
        }
        
        // Check for interaction input
        if (Input.GetKeyDown(interactKey) && IsPlayerInRange())
        {
            ToggleDoor();
        }
        
        // Debug info
        if (showDebugInfo && Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("=== Door Debug Info ===");
            Debug.Log("Door: " + gameObject.name);
            Debug.Log("Player Found: " + playerFound);
            Debug.Log("Player: " + (player != null ? player.name : "NULL"));
            Debug.Log("Player Tag: " + (player != null ? player.tag : "NULL"));
            Debug.Log("Distance to Player: " + (player != null ? Vector3.Distance(transform.position, player.position) : "N/A"));
            Debug.Log("Door Pivot: " + (doorPivot != null ? doorPivot.name : "NULL"));
            Debug.Log("Is Open: " + isOpen);
            Debug.Log("Interaction Distance: " + interactionDistance);
        }
    }
    
    void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }
    
    void OpenDoor()
    {
        isOpen = true;
        doorPivot.localRotation = openRotation;
        
        // Play sound
        if (openSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        // Trigger event
        onDoorOpened?.Invoke();
        
        if (showDebugInfo)
            Debug.Log("Door opened: " + gameObject.name);
    }
    
    void CloseDoor()
    {
        isOpen = false;
        doorPivot.localRotation = closedRotation;
        
        // Play sound
        if (closeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
        
        // Trigger event
        onDoorClosed?.Invoke();
        
        if (showDebugInfo)
            Debug.Log("Door closed: " + gameObject.name);
    }
    
    bool IsPlayerInRange()
    {
        if (player == null) return false;
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactionDistance;
        
        if (showDebugInfo && Input.GetKey(KeyCode.F2))
        {
            Debug.Log("Player distance: " + distance + " (Range: " + interactionDistance + ") - In Range: " + inRange);
        }
        
        return inRange;
    }
    
    // Public methods for other scripts to control the door
    public void ForceOpen()
    {
        if (!isOpen)
        {
            OpenDoor();
        }
    }
    
    public void ForceClose()
    {
        if (isOpen)
        {
            CloseDoor();
        }
    }
    
    public bool IsDoorOpen()
    {
        return isOpen;
    }
    
    // Gizmos for interaction range visualization (only in editor)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // Draw door pivot if assigned
        if (doorPivot != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(doorPivot.position, 0.1f);
        }
        
        // Draw line to player if found
        if (player != null)
        {
            Gizmos.color = IsPlayerInRange() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}

using UnityEngine;

public class SimpleDoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public bool isOpen = false;
    
    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    
    private Transform player;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool playerFound = false;
    private bool scriptInitialized = false;
    
    void Start()
    {
        Debug.Log("SimpleDoorInteraction: Start() called on " + gameObject.name);
        
        // Store rotations first
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
        
        Debug.Log("SimpleDoorInteraction: Closed rotation: " + closedRotation.eulerAngles);
        Debug.Log("SimpleDoorInteraction: Open rotation: " + openRotation.eulerAngles);
        
        // Find player
        FindPlayer();
        
        // Set initial state
        if (isOpen)
        {
            transform.localRotation = openRotation;
            Debug.Log("SimpleDoorInteraction: Door set to open position");
        }
        
        scriptInitialized = true;
        Debug.Log("SimpleDoorInteraction: Script fully initialized on " + gameObject.name);
    }
    
    void FindPlayer()
    {
        Debug.Log("SimpleDoorInteraction: Searching for player...");
        
        // Try to find player by tag first
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerFound = true;
            Debug.Log("SimpleDoorInteraction: Player found by tag: " + player.name + " at position " + player.position);
        }
        else
        {
            // Try to find by name
            playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerFound = true;
                Debug.LogWarning("SimpleDoorInteraction: Player found by name but missing 'Player' tag. Please add the 'Player' tag to your player GameObject!");
            }
            else
            {
                Debug.LogError("SimpleDoorInteraction: No player found! Make sure your player GameObject has the 'Player' tag or is named 'Player'.");
            }
        }
    }
    
    void Update()
    {
        if (!scriptInitialized)
        {
            Debug.LogWarning("SimpleDoorInteraction: Script not initialized yet!");
            return;
        }
        
        // Try to find player if not found
        if (!playerFound)
        {
            Debug.Log("SimpleDoorInteraction: Player not found, searching...");
            FindPlayer();
            return;
        }
        
        // Check for interaction
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("SimpleDoorInteraction: E key pressed!");
            
            if (IsPlayerInRange())
            {
                Debug.Log("SimpleDoorInteraction: Player in range, toggling door!");
                ToggleDoor();
            }
            else
            {
                Debug.Log("SimpleDoorInteraction: Player not in range. Distance: " + Vector3.Distance(transform.position, player.position));
            }
        }
        
        // Debug keys
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("=== Door Debug ===");
            Debug.Log("Door: " + gameObject.name);
            Debug.Log("Script Initialized: " + scriptInitialized);
            Debug.Log("Player Found: " + playerFound);
            Debug.Log("Player: " + (player != null ? player.name : "NULL"));
            Debug.Log("Player Tag: " + (player != null ? player.tag : "NULL"));
            Debug.Log("Player Position: " + (player != null ? player.position.ToString() : "NULL"));
            Debug.Log("Door Position: " + transform.position.ToString());
            Debug.Log("Distance: " + (player != null ? Vector3.Distance(transform.position, player.position) : "N/A"));
            Debug.Log("Interaction Distance: " + interactionDistance);
            Debug.Log("Is Open: " + isOpen);
            Debug.Log("Closed Rotation: " + closedRotation.eulerAngles);
            Debug.Log("Open Rotation: " + openRotation.eulerAngles);
            Debug.Log("Current Rotation: " + transform.localRotation.eulerAngles);
            Debug.Log("E Key Code: " + interactKey);
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                Debug.Log("Distance to player: " + distance + " (Range: " + interactionDistance + ") - In Range: " + (distance <= interactionDistance));
            }
            else
            {
                Debug.Log("No player found!");
            }
        }
        
        // Test door rotation with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("SimpleDoorInteraction: Force closing door");
            ForceClose();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("SimpleDoorInteraction: Force opening door");
            ForceOpen();
        }
    }
    
    void ToggleDoor()
    {
        Debug.Log("SimpleDoorInteraction: ToggleDoor() called. Current state: " + (isOpen ? "Open" : "Closed"));
        
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
        Debug.Log("SimpleDoorInteraction: Opening door...");
        isOpen = true;
        transform.localRotation = openRotation;
        Debug.Log("SimpleDoorInteraction: Door opened! New rotation: " + transform.localRotation.eulerAngles);
    }
    
    void CloseDoor()
    {
        Debug.Log("SimpleDoorInteraction: Closing door...");
        isOpen = false;
        transform.localRotation = closedRotation;
        Debug.Log("SimpleDoorInteraction: Door closed! New rotation: " + transform.localRotation.eulerAngles);
    }
    
    bool IsPlayerInRange()
    {
        if (player == null) 
        {
            Debug.LogWarning("SimpleDoorInteraction: Player is null in IsPlayerInRange!");
            return false;
        }
        
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactionDistance;
        
        if (Input.GetKey(KeyCode.F3))
        {
            Debug.Log("SimpleDoorInteraction: Distance check - Distance: " + distance + ", Range: " + interactionDistance + ", In Range: " + inRange);
        }
        
        return inRange;
    }
    
    // Public methods for testing
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
    
    // Gizmos for visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        if (player != null)
        {
            Gizmos.color = IsPlayerInRange() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}

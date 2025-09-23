using UnityEngine;

/// <summary>
/// Helper script to quickly set up a KeypadDoor system in the scene.
/// This can be run in the editor or at runtime to create a complete keypad door setup.
/// </summary>
public class KeypadDoorSetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    [SerializeField] private string keypadCode = "1234";
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Vector3 doorPosition = Vector3.zero;
    [SerializeField] private Vector3 keypadPosition = new Vector3(2f, 0f, 0f);
    [SerializeField] private bool createDoor = true;
    [SerializeField] private bool autoSetupOnStart = false;

    [Header("Door Settings")]
    [SerializeField] private float doorOpenAngle = 90f;
    [SerializeField] private float doorOpenSpeed = 2f;
    [SerializeField] private bool doorStartsLocked = true;

    [Header("Audio Clips (Optional)")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip correctCodeSound;
    [SerializeField] private AudioClip incorrectCodeSound;
    [SerializeField] private AudioClip unlockSound;

    private GameObject createdDoor;
    private KeypadDoorController createdKeypad;

    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupKeypadDoor();
        }
    }

    [ContextMenu("Setup Keypad Door System")]
    public void SetupKeypadDoor()
    {
        Debug.Log("[KeypadDoorSetup] Starting keypad door setup...");

        // Step 1: Create the door if needed
        if (createDoor)
        {
            CreateDoor();
        }

        // Step 2: Create the keypad controller
        CreateKeypadController();

        // Step 3: Configure the keypad
        ConfigureKeypad();

        // Step 4: Verify player setup
        VerifyPlayerSetup();

        Debug.Log("[KeypadDoorSetup] Keypad door setup complete!");
    }

    private void CreateDoor()
    {
        // Create door GameObject
        createdDoor = new GameObject("KeypadDoor_Door");
        createdDoor.transform.position = doorPosition;

        // Add door visual (simple cube for now - replace with your door model)
        GameObject doorVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorVisual.name = "DoorVisual";
        doorVisual.transform.SetParent(createdDoor.transform);
        doorVisual.transform.localPosition = Vector3.zero;
        doorVisual.transform.localScale = new Vector3(1f, 2f, 0.1f);

        // Add door script
        doorscript doorScript = createdDoor.AddComponent<doorscript>();
        
        // Configure door settings
        doorScript.openAngle = doorOpenAngle;
        doorScript.openSpeed = doorOpenSpeed;
        doorScript.isLocked = doorStartsLocked;
        doorScript.isOpen = false;

        // Add collider for interaction
        BoxCollider doorCollider = createdDoor.GetComponent<BoxCollider>();
        if (doorCollider == null)
        {
            doorCollider = createdDoor.AddComponent<BoxCollider>();
        }
        doorCollider.isTrigger = true;
        doorCollider.size = new Vector3(2f, 2f, 2f); // Interaction area

        Debug.Log("[KeypadDoorSetup] Created door at " + doorPosition);
    }

    private void CreateKeypadController()
    {
        // Create keypad GameObject
        GameObject keypadObj = new GameObject("KeypadController");
        keypadObj.transform.position = keypadPosition;

        // Add keypad controller component
        createdKeypad = keypadObj.AddComponent<KeypadDoorController>();

        // Add audio source for sounds
        AudioSource audioSource = keypadObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;

        Debug.Log("[KeypadDoorSetup] Created keypad controller at " + keypadPosition);
    }

    private void ConfigureKeypad()
    {
        if (createdKeypad == null) return;

        // Basic settings
        createdKeypad.correctCode = keypadCode;
        createdKeypad.codeLength = keypadCode.Length;
        createdKeypad.interactionDistance = interactionDistance;
        createdKeypad.autoCloseOnUnlock = true;

        // Door reference
        if (createdDoor != null)
        {
            doorscript doorScript = createdDoor.GetComponent<doorscript>();
            createdKeypad.targetDoor = doorScript;
            createdKeypad.openDoorOnUnlock = true;
        }

        // Audio clips
        createdKeypad.buttonClickSound = buttonClickSound;
        createdKeypad.correctSound = correctCodeSound;
        createdKeypad.incorrectSound = incorrectCodeSound;
        createdKeypad.unlockSound = unlockSound;

        // UI settings (let it auto-build)
        createdKeypad.buildUIIfMissing = true;

        Debug.Log($"[KeypadDoorSetup] Configured keypad with code: {keypadCode}");
    }

    private void VerifyPlayerSetup()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[KeypadDoorSetup] No GameObject with 'Player' tag found! Please tag your player.");
            return;
        }

        // Check for SimplePlayerMovement
        SimplePlayerMovement playerMovement = player.GetComponent<SimplePlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("[KeypadDoorSetup] Player doesn't have SimplePlayerMovement component! Keypad may not work properly.");
        }
        else
        {
            Debug.Log("[KeypadDoorSetup] Player setup verified.");
        }

        // Check for Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.Log("[KeypadDoorSetup] No Canvas found - will be auto-created when keypad is used.");
        }
    }

    [ContextMenu("Test Keypad Code")]
    public void TestKeypadCode()
    {
        if (createdKeypad != null)
        {
            Debug.Log($"[KeypadDoorSetup] Testing keypad with code: {createdKeypad.correctCode}");
            // You can add test logic here
        }
        else
        {
            Debug.LogWarning("[KeypadDoorSetup] No keypad created yet. Run setup first.");
        }
    }

    [ContextMenu("Cleanup Created Objects")]
    public void CleanupCreatedObjects()
    {
        if (createdDoor != null)
        {
            DestroyImmediate(createdDoor);
            Debug.Log("[KeypadDoorSetup] Cleaned up created door.");
        }

        if (createdKeypad != null)
        {
            DestroyImmediate(createdKeypad.gameObject);
            Debug.Log("[KeypadDoorSetup] Cleaned up created keypad.");
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw door position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(doorPosition, new Vector3(1f, 2f, 0.1f));

        // Draw keypad position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(keypadPosition, 0.3f);

        // Draw interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(keypadPosition, interactionDistance);

        // Draw connection line
        Gizmos.color = Color.white;
        Gizmos.DrawLine(doorPosition, keypadPosition);
    }
}


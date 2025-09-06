using UnityEngine;

public class KeyPuzzleDoor : MonoBehaviour
{
    [Header("Setup")]
    public doorscript door;
    public DullahanHeadInventory headInventory;

    [Header("Puzzle Settings")]
    public string requiredKeySOId = "";
    public bool consumeKeyOnWrong = true;
    public bool consumeKeyOnRight = true; // redundant with door.consumeKeyOnUnlock but explicit here

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 3f;
    public Camera cam;
    public bool enableLocalInteraction = false; // disabled by default; doorscript handles interaction

    private bool _playerInRange = false;

    void Start()
    {
        if (door == null) door = GetComponent<doorscript>();
        if (headInventory == null) headInventory = FindObjectOfType<DullahanHeadInventory>();
        if (cam == null) cam = Camera.main;
        if (door != null)
        {
            door.requiredKeySOId = requiredKeySOId;
            door.consumeKeyOnUnlock = consumeKeyOnRight;
        }
    }

    void Update()
    {
        if (!enableLocalInteraction) return;
        if (!_playerInRange) return;
        if (Input.GetKeyDown(interactKey))
        {
            TryUseSelectedKey();
        }
    }

    private void TryUseSelectedKey()
    {
        if (headInventory == null) return;
        var key = headInventory.GetSelectedKey();
        if (key == null) return;

        bool isRight = !string.IsNullOrEmpty(requiredKeySOId) && key.keyId == requiredKeySOId;

        // Consume key in both cases
        headInventory.RemoveSelectedKeyIfKey();

        if (isRight)
        {
            if (door != null)
            {
                door.requiredKeySOId = requiredKeySOId;
                door.isLocked = false;
                door.ToggleDoor();
            }
            else
            {
                Debug.Log("Correct key used, but no door assigned.");
            }
        }
        else
        {
            Debug.Log("Wrong key used. Key consumed.");
            // Optionally play a wrong sound or feedback here
            if (door != null)
            {
                // door remains locked
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
        }
    }
}



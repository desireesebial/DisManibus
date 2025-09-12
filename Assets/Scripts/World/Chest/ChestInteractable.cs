using UnityEngine;

/// <summary>
/// Lets the player interact with a chest by pressing an interaction key while aiming at it.
/// - If the chest has a ChestPadlock and is locked, will attempt to use the player's selected key.
/// - If unlocked (or no padlock), toggles the ChestLid.
/// Attach to a collider on the chest (or a child). Requires a collider to be raycastable.
/// </summary>
public class ChestInteractable : MonoBehaviour
{
    [Header("References")]
    public ChestLid chestLid;
    public ChestPadlock chestPadlock;
    [Tooltip("Root transform of the chest hierarchy. If null, uses this object's root. Any collider under this root will count as the same chest.")]
    public Transform chestRoot;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public float interactRange = 3.0f;
    public LayerMask interactMask = -1;

    private Camera _playerCamera;
    private DullahanHeadInventory _inventory;

    void Awake()
    {
        if (chestLid == null)
            chestLid = GetComponentInParent<ChestLid>();
        if (chestPadlock == null)
            chestPadlock = GetComponentInParent<ChestPadlock>();

        if (chestRoot == null)
            chestRoot = transform.root;
    }

    void Start()
    {
        // Find player camera and inventory
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var fpc = player.GetComponent<FirstPersonController>();
            if (fpc != null && fpc.playerCamera != null)
                _playerCamera = fpc.playerCamera;
            if (_playerCamera == null)
                _playerCamera = player.GetComponentInChildren<Camera>();
            _inventory = player.GetComponent<DullahanHeadInventory>();
        }

        if (_playerCamera == null)
            _playerCamera = Camera.main;
    }

    void Update()
    {
        if (_playerCamera == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        Ray ray = _playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactRange, interactMask, QueryTriggerInteraction.Collide))
        {
            // Check if the ray hit this chest (any child under chestRoot)
            Transform t = hit.collider.transform;
            Transform root = chestRoot != null ? chestRoot : transform;
            if (t == root || t.IsChildOf(root))
            {
                InteractLogic();
            }
        }
    }

    private void InteractLogic()
    {
        // If there is a padlock and it's locked, attempt to use selected key
        if (chestPadlock != null && chestPadlock.IsLocked)
        {
            if (_inventory != null)
            {
                KeySO selectedKey = _inventory.GetSelectedKey();
                if (selectedKey != null)
                {
                    // Try unlock with selected key; will open the lid on success
                    bool unlocked = chestPadlock.TryUseKey(selectedKey);
                    if (unlocked)
                    {
                        // Optionally consume the key if design requires
                        // _inventory.RemoveSelectedKeyIfKey();
                        if (chestLid != null)
                        {
                            chestLid.Open();
                        }
                        return;
                    }
                }
            }
            // Locked and no key/failed: do nothing further
            return;
        }

        // Unlocked or no padlock: toggle lid
        if (chestLid != null)
        {
            chestLid.Toggle();
        }
    }
}



using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DullahanHeadInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public int maxInventorySize = 3;
    public int playerReach = 3;
    public int selectedItem = 0;
    [Tooltip("Auto-select newly picked up items immediately (makes inventory feel more responsive)")]
    public bool autoSelectNewItems = true;

    [Header("Camera and UI")]
    [SerializeField] Camera cam;
    [SerializeField] GameObject pressToPickup_gameobject;

    [Header("Inventory UI")]
    [SerializeField] Image[] inventorySlotImage = new Image[3];
    [SerializeField] Image[] inventoryBackgroundImage = new Image[3];
    [SerializeField] Sprite emptySlotImage;

    [Header("Selected Item UI (Optional)")]
    [SerializeField] TextMeshProUGUI selectedItemNameText;
    [SerializeField] float selectedItemNameVisibleSeconds = 2f;
    [SerializeField] bool setHandVisualsToIgnoreRaycast = false; // optional safety; off by default

    [Header("Input Keys")]
    [SerializeField] KeyCode pickUpItemKey = KeyCode.E;
    [SerializeField] KeyCode throwItemKey = KeyCode.G;

    [Header("Player Item GameObjects")]
    [SerializeField] GameObject realHead_item;
    [SerializeField] GameObject fakeHead1_item;
    [SerializeField] GameObject fakeHead2_item;
    [SerializeField] GameObject lantern_item;

    [Header("Item Prefabs for Dropping")]
    [SerializeField] GameObject realHead_prefab;
    [SerializeField] GameObject fakeHead1_prefab;
    [SerializeField] GameObject fakeHead2_prefab;
    [SerializeField] GameObject lantern_prefab;

    [Header("Throwing Settings")]
    [SerializeField] GameObject throwObject_gameobject;
    [SerializeField] float throwForce = 5f;
    [SerializeField] bool enableDropping = true;

    [Header("Lantern System")]
    public bool hasLantern = false;
    public bool isLanternOn = false;
    public LanternSO currentLantern;
    public Light lanternLight;

    [Header("Flashlight System")]
    public bool hasFlashlight = false;
    public bool isFlashlightOn = false;
    public Light flashlightLight;
    public float flashlightBattery = 100f;
    public float maxFlashlightBattery = 100f;
    public float flashlightDrainRate = 5f;
    public float flashlightRechargeRate = 2f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip headSelectSound;
    public AudioClip flashlightToggleSound;

    private Dictionary<HeadType, GameObject> itemSetActive = new Dictionary<HeadType, GameObject>();
    private GameObject spawnedHeldVisual; // runtime instance of selected head's held visual
    private Coroutine selectedItemNameRoutine;
    
    [System.Serializable]
    public class KeyVisualMapping
    {
        public KeySO key;
        public GameObject keyObject;
    }
    [Header("Key Visuals")]
    [SerializeField] List<KeyVisualMapping> keyVisuals = new List<KeyVisualMapping>();
    private Dictionary<string, GameObject> keyVisualById = new Dictionary<string, GameObject>();
    private bool batteryLowPlayed = false;
    private bool batteryDeadPlayed = false;

    [System.Serializable]
    public class InventorySlot
    {
        public bool isOccupied = false;
        public DullahanHeadSO headItem = null;
        public LanternSO lanternItem = null;
        public KeySO keyItem = null;
        public ItemType itemType = ItemType.Empty;
        
        public enum ItemType
        {
            Empty,
            Head,
            Lantern,
            Key
        }
        
        public void Clear()
        {
            isOccupied = false;
            headItem = null;
            lanternItem = null;
            keyItem = null;
            itemType = ItemType.Empty;
        }
        
        public void SetHead(DullahanHeadSO head)
        {
            isOccupied = true;
            headItem = head;
            lanternItem = null;
            keyItem = null;
            itemType = ItemType.Head;
        }
        
        public void SetLantern(LanternSO lantern)
        {
            isOccupied = true;
            headItem = null;
            lanternItem = lantern;
            keyItem = null;
            itemType = ItemType.Lantern;
        }

        public void SetKey(KeySO key)
        {
            isOccupied = true;
            headItem = null;
            lanternItem = null;
            keyItem = key;
            itemType = ItemType.Key;
        }
    }

    void Start()
    {
        // Initialize inventory slots
        InitializeInventorySlots();
        
        // Initialize item dictionary
        InitializeItemDictionary();

        // Set initial UI state
        UpdateInventoryUI();

        // If we start with items, select the first one
        if (GetItemCount() > 0)
        {
            selectedItem = 0;
            NewItemSelected();
        }
        else
        {
            DeactivateAllItems();
        }

        // Find audio source if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Fallback: ensure we have a camera for center-screen raycasts (crosshair)
        if (cam == null)
        {
            // Prefer the FirstPersonController's assigned camera
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var fpc = player.GetComponent<FirstPersonController>();
                if (fpc != null && fpc.playerCamera != null)
                {
                    cam = fpc.playerCamera;
                }
                if (cam == null)
                {
                    cam = player.GetComponentInChildren<Camera>();
                }
            }

            // As a final fallback, use Camera.main
            if (cam == null)
            {
                cam = Camera.main;
            }
        }
    }

    void Update()
    {
        HandleItemThrowing();
        HandleItemPickup();
        HandleItemSelection();
        HandleLanternAndFlashlight();
        HandleFlashlightBattery();
        UpdateInventoryUI();
    }

    private void HandleItemThrowing()
    {
        if (!enableDropping) return;
        if (Input.GetKeyDown(throwItemKey))
        {
            if (!HasItems() || selectedItem < 0 || selectedItem >= inventorySlots.Count) return;

            var slot = inventorySlots[selectedItem];
            if (!slot.isOccupied) return;

            // Determine what to drop (heads and lantern supported)
            if (slot.itemType == InventorySlot.ItemType.Head && slot.headItem != null)
            {
                DropItemWithPhysics(slot);
                RemoveItemFromSlot(selectedItem);
            }
            else if (slot.itemType == InventorySlot.ItemType.Lantern && slot.lanternItem != null)
            {
                DropItemWithPhysics(slot);
                RemoveItemFromSlot(selectedItem);
            }
            // Keys: intentionally not handled here to avoid altering key pickable behavior
        }
    }

    private void InitializeInventorySlots()
    {
        // Initialize inventory slots if empty
        if (inventorySlots.Count == 0)
        {
            for (int i = 0; i < maxInventorySize; i++)
            {
                inventorySlots.Add(new InventorySlot());
            }
        }
    }

    private void InitializeItemDictionary()
    {
        itemSetActive.Clear();

        if (realHead_item != null)
        {
            itemSetActive.Add(HeadType.Real, realHead_item);
            SanitizeAnchor(realHead_item);
        }
        if (fakeHead1_item != null)
        {
            itemSetActive.Add(HeadType.Fake1, fakeHead1_item);
            SanitizeAnchor(fakeHead1_item);
        }
        if (fakeHead2_item != null)
        {
            itemSetActive.Add(HeadType.Fake2, fakeHead2_item);
            SanitizeAnchor(fakeHead2_item);
        }

        // Initially deactivate all items
        DeactivateAllItems();

        // Build key visual map and hide all key visuals
        keyVisualById.Clear();
        for (int i = 0; i < keyVisuals.Count; i++)
        {
            var entry = keyVisuals[i];
            if (entry != null && entry.key != null)
            {
                string id = entry.key.keyId;
                if (!string.IsNullOrEmpty(id) && entry.keyObject != null && !keyVisualById.ContainsKey(id))
                {
                    keyVisualById.Add(id, entry.keyObject);
                    entry.keyObject.SetActive(false);
                    TrySetIgnoreRaycastLayer(entry.keyObject);
                }
            }
        }
    }

    private void HandleItemPickup()
    {
        if (cam == null) 
        {
            Debug.LogWarning("DullahanHeadInventory: Camera reference is null! Cannot perform raycast pickup.");
            return;
        }

        // Raycast from the screen center (crosshair), not the mouse position
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, playerReach))
        {
            IPickable pickableItem = hitInfo.collider.GetComponentInParent<IPickable>();
            DullahanHeadPickable pickableComponent = hitInfo.collider.GetComponentInParent<DullahanHeadPickable>();
            LanternPickable lanternComponent = hitInfo.collider.GetComponentInParent<LanternPickable>();
            KeyPickable keyComponent = hitInfo.collider.GetComponentInParent<KeyPickable>();

            if (pickableItem != null)
            {
                // Show pickup prompt
                if (pressToPickup_gameobject != null)
                    pressToPickup_gameobject.SetActive(true);

                // Handle pickup input - only if not toggling lantern
                if (Input.GetKeyDown(pickUpItemKey))
                {
                    // Check if we're trying to toggle lantern instead of pickup
                    if (hasLantern && currentLantern != null && currentLantern.toggleKey == pickUpItemKey)
                    {
                        // Don't pickup if we're toggling lantern
                        return;
                    }
                    
                    if (pickableComponent != null && !pickableComponent.isPickedUp)
                    {
                        TryPickupHead(pickableComponent, pickableItem);
                    }
                    else if (lanternComponent != null && !lanternComponent.isPickedUp)
                    {
                        TryPickupLantern(lanternComponent, pickableItem);
                    }
                    else if (keyComponent != null && !keyComponent.isPickedUp)
                    {
                        TryPickupKey(keyComponent, pickableItem);
                    }
                    else
                    {
                        Debug.LogWarning($"DullahanHeadInventory: Hit object '{hitInfo.collider.name}' with IPickable but no recognized pickup component found!");
                    }
                }
            }
            else
            {
                if (pressToPickup_gameobject != null)
                    pressToPickup_gameobject.SetActive(false);
            }
        }
        else
        {
            if (pressToPickup_gameobject != null)
                pressToPickup_gameobject.SetActive(false);
        }
    }

    private void TryPickupHead(DullahanHeadPickable pickableComponent, IPickable pickableItem)
    {
        // Check if item has valid ScriptableObject
        if (pickableComponent.headData == null)
        {
            Debug.LogError("Head has no ScriptableObject assigned!");
            return;
        }

        // Find first empty slot
        int emptySlot = FindFirstEmptySlot();
        if (emptySlot == -1)
        {
            Debug.Log("Inventory is full! Cannot pick up more items.");
            return;
        }

        // Add item to inventory slot
        inventorySlots[emptySlot].SetHead(pickableComponent.headData);

        // Pick up the item (destroys GameObject)
        pickableItem.PickItem();

        // Auto-select newly picked up item for smooth, responsive feel
        if (autoSelectNewItems || GetItemCount() == 1)
        {
            selectedItem = emptySlot;
            NewItemSelected();
            Debug.Log($"Picked up and selected: {pickableComponent.headData.headName} in slot {emptySlot + 1}");
        }
        else
        {
            Debug.Log($"Picked up: {pickableComponent.headData.headName} in slot {emptySlot + 1}");
        }
    }

    private void TryPickupLantern(LanternPickable lanternComponent, IPickable pickableItem)
    {
        // Check if lantern has valid ScriptableObject
        if (lanternComponent.lanternData == null)
        {
            Debug.LogError("Lantern has no ScriptableObject assigned!");
            return;
        }

        // Find first empty slot
        int emptySlot = FindFirstEmptySlot();
        if (emptySlot == -1)
        {
            Debug.Log("Inventory is full! Cannot pick up lantern.");
            return;
        }

        // Add lantern to inventory slot
        inventorySlots[emptySlot].SetLantern(lanternComponent.lanternData);

        // Update lantern system
        hasLantern = true;
        isLanternOn = false;
        currentLantern = lanternComponent.lanternData;

        // Pick up the item (destroys GameObject)
        pickableItem.PickItem();

        // Auto-select newly picked up item for smooth, responsive feel
        if (autoSelectNewItems || GetItemCount() == 1)
        {
            selectedItem = emptySlot;
            NewItemSelected();
        }

        // Show lantern in hand immediately (but turned off)
        SetLanternVisual(false);

        string toggleMessage = currentLantern.toggleMessage;
        Debug.Log($"Picked up {currentLantern.lanternName} in slot {emptySlot + 1}! {toggleMessage}");
    }

    private void TryPickupKey(KeyPickable keyComponent, IPickable pickableItem)
    {
        Debug.Log($"DullahanHeadInventory: Attempting to pickup key from {keyComponent.gameObject.name}");
        
        // Check if key has valid ScriptableObject
        if (keyComponent.keyData == null)
        {
            Debug.LogError($"DullahanHeadInventory: Key '{keyComponent.gameObject.name}' has no ScriptableObject assigned!");
            return;
        }

        // Find first empty slot
        int emptySlot = FindFirstEmptySlot();
        if (emptySlot == -1)
        {
            Debug.LogWarning("DullahanHeadInventory: Inventory is full! Cannot pick up key.");
            return;
        }

        Debug.Log($"DullahanHeadInventory: Adding key '{keyComponent.keyData.keyName}' (ID: {keyComponent.keyData.keyId}) to slot {emptySlot + 1}");

        // Add key to inventory slot
        inventorySlots[emptySlot].SetKey(keyComponent.keyData);

        // Pick up the item (destroys GameObject)
        pickableItem.PickItem();

        // Auto-select newly picked up item for smooth, responsive feel
        if (autoSelectNewItems || GetItemCount() == 1)
        {
            selectedItem = emptySlot;
            NewItemSelected();
            Debug.Log($"DullahanHeadInventory: Successfully picked up and selected key: {keyComponent.keyData.keyName} in slot {emptySlot + 1}");
        }
        else
        {
            Debug.Log($"DullahanHeadInventory: Successfully picked up key: {keyComponent.keyData.keyName} in slot {emptySlot + 1}");
        }
    }

    private void HandleItemSelection()
    {
        int newSelection = -1;

        // Number keys 1-9 select slot index 0-8 within maxInventorySize (regardless of occupancy)
        if (Input.GetKeyDown(KeyCode.Alpha1) && maxInventorySize >= 1) newSelection = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2) && maxInventorySize >= 2) newSelection = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3) && maxInventorySize >= 3) newSelection = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4) && maxInventorySize >= 4) newSelection = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5) && maxInventorySize >= 5) newSelection = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6) && maxInventorySize >= 6) newSelection = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7) && maxInventorySize >= 7) newSelection = 6;
        else if (Input.GetKeyDown(KeyCode.Alpha8) && maxInventorySize >= 8) newSelection = 7;
        else if (Input.GetKeyDown(KeyCode.Alpha9) && maxInventorySize >= 9) newSelection = 8;

        // Mouse wheel scroll cycles selection across all slots (including empty)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.01f)
        {
            newSelection = (selectedItem + 1) % Mathf.Max(1, maxInventorySize);
        }
        else if (scroll < -0.01f)
        {
            int size = Mathf.Max(1, maxInventorySize);
            newSelection = (selectedItem - 1 + size) % size;
        }

        if (newSelection != -1 && newSelection != selectedItem)
        {
            selectedItem = Mathf.Clamp(newSelection, 0, Mathf.Max(0, maxInventorySize - 1));
            NewItemSelected();
        }
    }

    private void HandleLanternAndFlashlight()
    {
        // Toggle lantern - simple on/off only
        if (hasLantern && currentLantern != null && Input.GetKeyDown(currentLantern.toggleKey))
        {
            ToggleLantern();
        }

        // Toggle flashlight
        if (Input.GetKeyDown(KeyCode.F) && hasFlashlight)
        {
            ToggleFlashlight();
        }
    }

    public void NewItemSelected()
    {
        if (!HasItems())
        {
            DeactivateAllItems();
            DestroySpawnedHeldVisual();
            UpdateSelectedItemNameUI();
            return;
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, Mathf.Max(0, maxInventorySize - 1));

        DeactivateAllItems();

        InventorySlot currentSlot = (selectedItem >= 0 && selectedItem < inventorySlots.Count && inventorySlots[selectedItem].isOccupied)
            ? inventorySlots[selectedItem] : null;

        if (currentSlot != null)
        {
            if (currentSlot.itemType == InventorySlot.ItemType.Head && currentSlot.headItem != null)
            {
                if (itemSetActive.ContainsKey(currentSlot.headItem.headType))
                {
                    GameObject itemObject = itemSetActive[currentSlot.headItem.headType];
                    if (itemObject != null)
                    {
                        itemObject.SetActive(true);
                    }
                }
                else
                {
                    GameObject anchor = null;
                    itemSetActive.TryGetValue(HeadType.Real, out anchor);
                    TrySpawnHeldVisual(currentSlot.headItem, anchor);
                }

                if (audioSource != null && headSelectSound != null)
                {
                    audioSource.PlayOneShot(headSelectSound);
                }
            }
            else if (currentSlot.itemType == InventorySlot.ItemType.Lantern && currentSlot.lanternItem != null)
            {
                currentLantern = currentSlot.lanternItem;
                hasLantern = true;
                SetLanternVisual(isLanternOn);
            }
            else if (currentSlot.itemType == InventorySlot.ItemType.Key && currentSlot.keyItem != null)
            {
                ShowKeyVisual(currentSlot.keyItem);
            }
        }
        else
        {
            HideAllKeyVisuals();
            DestroySpawnedHeldVisual();
        }

        UpdateSelectedItemNameUI();
        ShowSelectedNameTemporarily();
    }

    private void UpdateInventoryUI()
    {
        // Update inventory slot images
        for (int i = 0; i < inventorySlotImage.Length; i++)
        {
            if (inventorySlotImage[i] != null)
            {
                if (i < inventorySlots.Count && inventorySlots[i].isOccupied)
                {
                    if (inventorySlots[i].itemType == InventorySlot.ItemType.Head && inventorySlots[i].headItem != null)
                    {
                        inventorySlotImage[i].sprite = inventorySlots[i].headItem.headIcon;
                    }
                    else if (inventorySlots[i].itemType == InventorySlot.ItemType.Lantern && inventorySlots[i].lanternItem != null)
                    {
                        inventorySlotImage[i].sprite = inventorySlots[i].lanternItem.lanternIcon;
                    }
                    else if (inventorySlots[i].itemType == InventorySlot.ItemType.Key && inventorySlots[i].keyItem != null)
                    {
                        inventorySlotImage[i].sprite = inventorySlots[i].keyItem.keyIcon;
                    }
                    else
                    {
                        inventorySlotImage[i].sprite = emptySlotImage;
                    }
                }
                else
                {
                    inventorySlotImage[i].sprite = emptySlotImage;
                }
            }
        }

        // Update background colors for selection (always highlight the selected slot)
        for (int i = 0; i < inventoryBackgroundImage.Length; i++)
        {
            if (inventoryBackgroundImage[i] != null)
            {
                if (i == selectedItem)
                {
                    inventoryBackgroundImage[i].color = new Color32(145, 255, 126, 255);
                }
                else
                {
                    inventoryBackgroundImage[i].color = new Color32(219, 219, 219, 255);
                }
            }
        }
    }

    public void DeactivateAllItems()
    {
        if (realHead_item != null) realHead_item.SetActive(false);
        if (fakeHead1_item != null) fakeHead1_item.SetActive(false);
        if (fakeHead2_item != null) fakeHead2_item.SetActive(false);
        
        // Don't deactivate lantern here - it should be handled by SetLanternVisual
        // The lantern should stay visible when we have it, only the light should be controlled

        // Hide all key visuals
        foreach (var kv in keyVisualById)
        {
            if (kv.Value != null) kv.Value.SetActive(false);
        }
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (!inventorySlots[i].isOccupied)
            {
                return i;
            }
        }
        return -1; // No empty slots
    }

    private bool HasItems()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied)
            {
                return true;
            }
        }
        return false;
    }

    private int GetItemCount()
    {
        int count = 0;
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied)
            {
                count++;
            }
        }
        return count;
    }

    // Public methods for other scripts to use
    public bool HasItem(int itemID)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Head)
            {
                if (inventorySlots[i].headItem != null && inventorySlots[i].headItem.headID == itemID)
                    return true;
            }
        }
        return false;
    }

    public bool HasItemOfType(HeadType type)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Head)
            {
                if (inventorySlots[i].headItem != null && inventorySlots[i].headItem.headType == type)
                    return true;
            }
        }
        return false;
    }

    public DullahanHeadSO GetCurrentItem()
    {
        if (selectedItem >= 0 && selectedItem < inventorySlots.Count && inventorySlots[selectedItem].isOccupied)
        {
            if (inventorySlots[selectedItem].itemType == InventorySlot.ItemType.Head)
                return inventorySlots[selectedItem].headItem;
        }
        return null;
    }

    public List<DullahanHeadSO> GetItemsOfType(HeadType type)
    {
        List<DullahanHeadSO> items = new List<DullahanHeadSO>();
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Head)
            {
                if (inventorySlots[i].headItem != null && inventorySlots[i].headItem.headType == type)
                    items.Add(inventorySlots[i].headItem);
            }
        }
        return items;
    }

    private void DropItemWithPhysics(InventorySlot slot)
    {
        GameObject prefabToThrow = GetPrefabForItem(slot);
        if (prefabToThrow == null) return;

        Vector3 throwPosition;
        if (throwObject_gameobject != null)
        {
            throwPosition = throwObject_gameobject.transform.position;
        }
        else if (cam != null)
        {
            throwPosition = cam.transform.position + cam.transform.forward * 1f;
        }
        else
        {
            throwPosition = transform.position + transform.forward * 1f;
        }

        GameObject droppedItem = Instantiate(prefabToThrow, throwPosition, Quaternion.identity);

        if (slot.itemType == InventorySlot.ItemType.Head)
        {
            var droppedPickable = droppedItem.GetComponent<DullahanHeadPickable>();
            if (droppedPickable != null)
            {
                droppedPickable.headData = slot.headItem;
            }
        }
        else if (slot.itemType == InventorySlot.ItemType.Lantern)
        {
            var droppedLantern = droppedItem.GetComponent<LanternPickable>();
            if (droppedLantern != null)
            {
                droppedLantern.lanternData = slot.lanternItem;
            }
        }

        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb == null) rb = droppedItem.AddComponent<Rigidbody>();
        rb.mass = GetItemMass(slot);
#if UNITY_6000_0_OR_NEWER
        if (rb.linearDamping == 0) rb.linearDamping = 1f;
        if (rb.angularDamping == 0) rb.angularDamping = 5f;
#endif
        Vector3 throwDirection = (cam != null)
            ? (cam.transform.forward + Vector3.up * 0.3f).normalized
            : (transform.forward + Vector3.up * 0.3f).normalized;

        rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);
        rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.VelocityChange);
    }

    private GameObject GetPrefabForItem(InventorySlot slot)
    {
        if (slot.itemType == InventorySlot.ItemType.Head)
        {
            if (slot.headItem != null && slot.headItem.headPrefab != null)
                return slot.headItem.headPrefab;
            if (slot.headItem != null)
            {
                switch (slot.headItem.headType)
                {
                    case HeadType.Real: return realHead_prefab;
                    case HeadType.Fake1: return fakeHead1_prefab;
                    case HeadType.Fake2: return fakeHead2_prefab;
                }
            }
        }
        else if (slot.itemType == InventorySlot.ItemType.Lantern)
        {
            return lantern_prefab;
        }
        return null;
    }

    private float GetItemMass(InventorySlot slot)
    {
        switch (slot.itemType)
        {
            case InventorySlot.ItemType.Head:
                return 1.0f;
            case InventorySlot.ItemType.Lantern:
                return 0.8f;
            default:
                return 0.2f;
        }
    }

    private void DestroySpawnedHeldVisual()
    {
        if (spawnedHeldVisual != null)
        {
            Destroy(spawnedHeldVisual);
            spawnedHeldVisual = null;
        }
    }

    private void TrySpawnHeldVisual(DullahanHeadSO head, GameObject anchor)
    {
        DestroySpawnedHeldVisual();
        if (head == null || head.headPrefab == null || anchor == null) return;
        DisablePhysicsOnHierarchy(anchor);
        spawnedHeldVisual = Instantiate(head.headPrefab, anchor.transform);
        spawnedHeldVisual.transform.localPosition = Vector3.zero;
        spawnedHeldVisual.transform.localRotation = Quaternion.identity;
        spawnedHeldVisual.transform.localScale = Vector3.one;
        var rb = spawnedHeldVisual.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
        var collider = spawnedHeldVisual.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        DisablePhysicsOnHierarchy(spawnedHeldVisual);
        TrySetIgnoreRaycastLayer(spawnedHeldVisual);
    }

    private void ShowKeyVisual(KeySO key)
    {
        foreach (var kv in keyVisualById)
        {
            if (kv.Value != null) kv.Value.SetActive(false);
        }
        if (key == null) return;
        if (!string.IsNullOrEmpty(key.keyId) && keyVisualById.TryGetValue(key.keyId, out var visual) && visual != null)
        {
            visual.SetActive(true);
            TrySetIgnoreRaycastLayer(visual);
        }
    }

    private void HideAllKeyVisuals()
    {
        foreach (var kv in keyVisualById)
        {
            if (kv.Value != null)
            {
                kv.Value.SetActive(false);
            }
        }
    }

    private void DisablePhysicsOnHierarchy(GameObject root)
    {
        if (root == null) return;
        var colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
        var bodies = root.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            if (Application.isPlaying)
            {
                Destroy(bodies[i]);
            }
            else
            {
                DestroyImmediate(bodies[i]);
            }
        }
    }

    private void SetIgnoreRaycastLayer(GameObject root)
    {
        if (root == null) return;
        int ignoreLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreLayer < 0) return;
        SetLayerRecursive(root.transform, ignoreLayer);
    }

    private void TrySetIgnoreRaycastLayer(GameObject root)
    {
        if (!setHandVisualsToIgnoreRaycast) return;
        SetIgnoreRaycastLayer(root);
    }

    private void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++)
        {
            SetLayerRecursive(t.GetChild(i), layer);
        }
    }

    private void SanitizeAnchor(GameObject anchor)
    {
        TrySetIgnoreRaycastLayer(anchor);
        if (anchor != null)
        {
            var collider = anchor.GetComponent<Collider>();
            if (collider != null)
            {
                Debug.LogWarning($"DullahanHeadInventory: Anchor '{anchor.name}' has a Collider. Consider removing it or placing the anchor on a child without gameplay colliders to avoid interaction issues.");
            }
        }
    }

    private void UpdateSelectedItemNameUI()
    {
        if (selectedItemNameText == null)
            return;

        if (!HasItems() || selectedItem < 0 || selectedItem >= inventorySlots.Count || !inventorySlots[selectedItem].isOccupied)
        {
            selectedItemNameText.text = string.Empty;
            selectedItemNameText.enabled = false;
            return;
        }

        var slot = inventorySlots[selectedItem];
        if (slot.itemType == InventorySlot.ItemType.Head && slot.headItem != null)
        {
            selectedItemNameText.text = slot.headItem.headName;
        }
        else if (slot.itemType == InventorySlot.ItemType.Lantern && slot.lanternItem != null)
        {
            selectedItemNameText.text = slot.lanternItem.lanternName;
        }
        else if (slot.itemType == InventorySlot.ItemType.Key && slot.keyItem != null)
        {
            selectedItemNameText.text = slot.keyItem.keyName;
        }
        if (!selectedItemNameText.enabled) selectedItemNameText.enabled = true;
    }

    private void ShowSelectedNameTemporarily()
    {
        if (selectedItemNameText == null) return;
        if (selectedItemNameRoutine != null)
        {
            StopCoroutine(selectedItemNameRoutine);
            selectedItemNameRoutine = null;
        }
        if (!HasItems() || selectedItem < 0 || selectedItem >= inventorySlots.Count || !inventorySlots[selectedItem].isOccupied)
        {
            selectedItemNameText.enabled = false;
            return;
        }
        selectedItemNameText.enabled = true;
        selectedItemNameRoutine = StartCoroutine(HideSelectedNameAfterDelay(selectedItemNameVisibleSeconds));
    }

    private IEnumerator HideSelectedNameAfterDelay(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (selectedItemNameText != null)
        {
            selectedItemNameText.enabled = false;
        }
        selectedItemNameRoutine = null;
    }

    // Lantern and Flashlight methods
    public void GiveLantern(LanternSO lanternData = null)
    {
        hasLantern = true;
        isLanternOn = false;
        currentLantern = lanternData;
        
        // Show lantern in hand immediately (but turned off)
        SetLanternVisual(false);
        
        string message = currentLantern != null ? 
            $"{currentLantern.lanternName} added to inventory! {currentLantern.toggleMessage}" :
            "Lantern added to inventory!";
        Debug.Log(message);
    }

    public void RemoveLantern()
    {
        hasLantern = false;
        isLanternOn = false;
        currentLantern = null;
        
        // Hide lantern from hand
        if (lantern_item != null)
            lantern_item.SetActive(false);
        if (lanternLight != null)
            lanternLight.enabled = false;
            
        Debug.Log("Lantern removed from inventory!");
    }

    public void RemoveItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Count) return;

        InventorySlot slot = inventorySlots[slotIndex];
        if (!slot.isOccupied) return;

        if (slot.itemType == InventorySlot.ItemType.Lantern)
        {
            // If removing the currently selected lantern, update lantern system
            if (slot.lanternItem == currentLantern)
            {
                hasLantern = false;
                isLanternOn = false;
                currentLantern = null;
                
                // Hide lantern from hand
                if (lantern_item != null)
                    lantern_item.SetActive(false);
                if (lanternLight != null)
                    lanternLight.enabled = false;
            }
        }

        // Clear the slot
        slot.Clear();

        // If we removed the selected item, try to select another item
        if (slotIndex == selectedItem)
        {
            // Find next available item
            int nextItem = FindNextAvailableItem(slotIndex);
            if (nextItem != -1)
            {
                selectedItem = nextItem;
                NewItemSelected();
            }
            else
            {
                // No items left, deactivate all
                DeactivateAllItems();
            }
        }

        Debug.Log($"Item removed from slot {slotIndex + 1}");
    }

    private int FindNextAvailableItem(int currentSlot)
    {
        // Check slots after current
        for (int i = currentSlot + 1; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied)
                return i;
        }
        
        // Check slots before current
        for (int i = 0; i < currentSlot; i++)
        {
            if (inventorySlots[i].isOccupied)
                return i;
        }
        
        return -1; // No items found
    }

    public void GiveFlashlight()
    {
        hasFlashlight = true;
        flashlightBattery = maxFlashlightBattery;
        SetFlashlightVisual(false);
        Debug.Log("Flashlight added to inventory!");
    }

    public void RemoveFlashlight()
    {
        hasFlashlight = false;
        SetFlashlightVisual(false);
        Debug.Log("Flashlight removed from inventory!");
    }

    public void ToggleLantern()
    {
        if (!hasLantern || currentLantern == null) 
        {
            Debug.LogWarning("Cannot toggle lantern: hasLantern=" + hasLantern + ", currentLantern=" + (currentLantern != null));
            return;
        }

        // Simple on/off toggle
        isLanternOn = !isLanternOn;
        SetLanternVisual(isLanternOn);

        // Play sound based on ScriptableObject
        if (audioSource != null)
        {
            AudioClip soundToPlay = isLanternOn ? currentLantern.toggleOnSound : currentLantern.toggleOffSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }

        Debug.Log($"{currentLantern.lanternName}: {(isLanternOn ? "ON" : "OFF")} - Lantern GameObject active: {(lantern_item != null ? lantern_item.activeInHierarchy : false)}");
    }

    public void ToggleFlashlight()
    {
        if (!hasFlashlight || (flashlightBattery <= 0 && isFlashlightOn)) return;

        isFlashlightOn = !isFlashlightOn;
        SetFlashlightVisual(isFlashlightOn);

        // Play sound
        if (audioSource != null && flashlightToggleSound != null)
        {
            audioSource.PlayOneShot(flashlightToggleSound);
        }

        Debug.Log($"Flashlight toggled: {(isFlashlightOn ? "ON" : "OFF")}");
    }

    private void SetLanternVisual(bool active)
    {
        // Always show lantern in hand when we have it, regardless of on/off state
        if (lantern_item != null)
            lantern_item.SetActive(hasLantern);

        // Only control the light component
        if (lanternLight != null)
        {
            lanternLight.enabled = active && hasLantern;
            
            // Apply ScriptableObject settings if available
            if (active && currentLantern != null)
            {
                lanternLight.color = currentLantern.lightColor;
                lanternLight.intensity = currentLantern.lightIntensity;
                lanternLight.range = currentLantern.lightRange;
            }
        }
    }

    private void SetFlashlightVisual(bool active)
    {
        if (flashlightLight != null)
            flashlightLight.enabled = active;
    }

    private void HandleFlashlightBattery()
    {
        if (!hasFlashlight) return;

        if (isFlashlightOn)
        {
            // Drain battery
            flashlightBattery -= flashlightDrainRate * Time.deltaTime;
            flashlightBattery = Mathf.Max(0f, flashlightBattery);

            // Check for low battery
            float batteryPercentage = flashlightBattery / maxFlashlightBattery;

            if (batteryPercentage <= 0.1f && !batteryDeadPlayed)
            {
                batteryDeadPlayed = true;
                Debug.Log("Flashlight battery dead!");
            }
            else if (batteryPercentage <= 0.3f && !batteryLowPlayed)
            {
                batteryLowPlayed = true;
                Debug.Log("Flashlight battery low!");
            }
        }
        else
        {
            // Recharge battery when off
            if (flashlightBattery < maxFlashlightBattery)
            {
                flashlightBattery += flashlightRechargeRate * Time.deltaTime;
                flashlightBattery = Mathf.Min(maxFlashlightBattery, flashlightBattery);

                // Reset battery warnings when recharging
                if (flashlightBattery > maxFlashlightBattery * 0.3f)
                    batteryLowPlayed = false;
                if (flashlightBattery > maxFlashlightBattery * 0.1f)
                    batteryDeadPlayed = false;
            }
        }
    }

    // Compatibility methods for other scripts
    public List<DullahanHeadSO> inventoryList 
    { 
        get 
        {
            List<DullahanHeadSO> heads = new List<DullahanHeadSO>();
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Head)
                {
                    if (inventorySlots[i].headItem != null)
                        heads.Add(inventorySlots[i].headItem);
                }
            }
            return heads;
        }
    }
    
    public List<DullahanHeadSO> headInventoryList 
    { 
        get 
        {
            List<DullahanHeadSO> heads = new List<DullahanHeadSO>();
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Head)
                {
                    if (inventorySlots[i].headItem != null)
                        heads.Add(inventorySlots[i].headItem);
                }
            }
            return heads;
        }
    }
    public int selectedHeadIndex => selectedItem;

    public void NewHeadSelected()
    {
        NewItemSelected();
    }

    public void DeactivateAllHeads()
    {
        DeactivateAllItems();
    }

    // Public getters
    public bool HasHeads() => GetItemCount() > 0;
    public int GetHeadCount() => GetItemCount();
    public DullahanHeadSO GetSelectedHead() => GetCurrentItem();
    public DullahanHeadSO GetCurrentHead() => GetSelectedHead();
    public bool HasLantern() => hasLantern;
    public bool IsLanternOn() => isLanternOn;
    public bool HasFlashlight() => hasFlashlight;
    public bool IsFlashlightOn() => isFlashlightOn;
    public float GetFlashlightBattery() => flashlightBattery;
    public float GetFlashlightBatteryPercentage() => flashlightBattery / maxFlashlightBattery;
    public int maxHeadInventorySize => maxInventorySize;
    
    // Compatibility methods for inventoryList operations
    public void AddToInventoryList(DullahanHeadSO head)
    {
        int emptySlot = FindFirstEmptySlot();
        if (emptySlot != -1)
        {
            inventorySlots[emptySlot].SetHead(head);
        }
    }
    
    public void RemoveFromInventoryList(DullahanHeadSO head)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Head)
            {
                if (inventorySlots[i].headItem == head)
                {
                    RemoveItemFromSlot(i);
                    break;
                }
            }
        }
    }
    
    public void ClearInventoryList()
    {
        for (int i = inventorySlots.Count - 1; i >= 0; i--)
        {
            if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Head)
            {
                RemoveItemFromSlot(i);
            }
        }
    }

    // Selected key helpers for puzzles
    public KeySO GetSelectedKey()
    {
        if (selectedItem >= 0 && selectedItem < inventorySlots.Count && inventorySlots[selectedItem].isOccupied)
        {
            if (inventorySlots[selectedItem].itemType == InventorySlot.ItemType.Key)
            {
                return inventorySlots[selectedItem].keyItem;
            }
        }
        return null;
    }

    public bool RemoveSelectedKeyIfKey()
    {
        if (selectedItem >= 0 && selectedItem < inventorySlots.Count && inventorySlots[selectedItem].isOccupied)
        {
            if (inventorySlots[selectedItem].itemType == InventorySlot.ItemType.Key)
            {
                // Hide any key visual
                var key = inventorySlots[selectedItem].keyItem;
                if (key != null && !string.IsNullOrEmpty(key.keyId) && keyVisualById.ContainsKey(key.keyId) && keyVisualById[key.keyId] != null)
                {
                    keyVisualById[key.keyId].SetActive(false);
                }
                RemoveItemFromSlot(selectedItem);
                return true;
            }
        }
        return false;
    }

    // Door key helpers
    public bool HasKey(string keyId)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Key)
            {
                if (inventorySlots[i].keyItem != null && inventorySlots[i].keyItem.keyId == keyId)
                    return true;
            }
        }
        return false;
    }

    public bool ConsumeKey(string keyId)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Key)
            {
                if (inventorySlots[i].keyItem != null && inventorySlots[i].keyItem.keyId == keyId)
                {
                    // Hide key visual if shown
                    if (!string.IsNullOrEmpty(keyId) && keyVisualById.ContainsKey(keyId) && keyVisualById[keyId] != null)
                    {
                        keyVisualById[keyId].SetActive(false);
                    }
                    RemoveItemFromSlot(i);
                    Debug.Log($"Consumed key: {keyId}");
                    return true;
                }
            }
        }
        return false;
    }

    // Head placement/consumption helpers (use heads like keys for puzzles)
    public bool HasHead(HeadType type)
    {
        return HasItemOfType(type);
    }

    public bool RemoveSelectedHeadIfHead()
    {
        if (selectedItem >= 0 && selectedItem < inventorySlots.Count && inventorySlots[selectedItem].isOccupied)
        {
            if (inventorySlots[selectedItem].itemType == InventorySlot.ItemType.Head)
            {
                RemoveItemFromSlot(selectedItem);
                return true;
            }
        }
        return false;
    }

    public bool ConsumeHead(HeadType type)
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            if (inventorySlots[i].isOccupied && inventorySlots[i].itemType == InventorySlot.ItemType.Head)
            {
                if (inventorySlots[i].headItem != null && inventorySlots[i].headItem.headType == type)
                {
                    RemoveItemFromSlot(i);
                    Debug.Log($"Consumed head of type: {type}");
                    return true;
                }
            }
        }
        return false;
    }

    // Returns true if the currently selected head matches the required type and was removed from inventory
    public bool TryPlaceSelectedHeadOnBody(HeadType requiredType)
    {
        if (selectedItem < 0 || selectedItem >= inventorySlots.Count) return false;

        var slot = inventorySlots[selectedItem];
        if (!slot.isOccupied || slot.itemType != InventorySlot.ItemType.Head || slot.headItem == null) return false;

        if (slot.headItem.headType != requiredType) return false;

        // Remove from inventory (this will also update visuals/selection)
        RemoveItemFromSlot(selectedItem);
        Debug.Log($"Placed head of type {requiredType} on Dullahan body");
        return true;
    }

    // Preferred placement path: delegates to SimpleHeadCollectionPuzzle so it can apply effects for fake heads
    public bool TryPlaceSelectedHeadOnBody(SimpleHeadCollectionPuzzle headCollectionPuzzle)
    {
        if (headCollectionPuzzle == null) return false;
        if (selectedItem < 0 || selectedItem >= inventorySlots.Count) return false;

        var slot = inventorySlots[selectedItem];
        if (!slot.isOccupied || slot.itemType != InventorySlot.ItemType.Head || slot.headItem == null) return false;

        // Use simple placement logic (applies effects for fake heads, completes puzzle for real head)
        // Note: SimpleHeadCollectionPuzzle handles placement directly, so this method is mainly for compatibility
        // The actual placement is handled by the SimpleHeadCollectionPuzzle script when F is pressed
        return true; // Return true to indicate head can be placed
    }
}


using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory Settings")]
    public List<KeyItemsSO> inventoryList = new List<KeyItemsSO>();
    public int maxInventorySize = 5;
    public int playerReach = 3;
    public int selectedItem = 0;

    [Header("Camera and UI")]
    [SerializeField] Camera cam;
    [SerializeField] GameObject pressToPickup_gameobject;

    [Header("Inventory UI")]
    [SerializeField] Image[] inventorySlotImage = new Image[5];
    [SerializeField] Image[] inventoryBackgroundImage = new Image[5];
    [SerializeField] Sprite prazdnySlotImage; // Keep original name for compatibility

    [Header("Selected Item UI (Optional)")]
    [SerializeField] TextMeshProUGUI selectedItemNameText;
    [SerializeField] float selectedItemNameVisibleSeconds = 2f;
    [SerializeField] bool setHandVisualsToIgnoreRaycast = false; // optional safety; off by default

    [Header("Input Keys")]
    [SerializeField] KeyCode throwItemKey = KeyCode.G;
    [SerializeField] KeyCode pickUpItemKey = KeyCode.E;

    [Header("Player Item GameObjects")]
    [SerializeField] GameObject keys_item;
    [SerializeField] GameObject document_item;
    [SerializeField] GameObject flashlight_item;

    [Header("Item Prefabs for Dropping")]
    [SerializeField] GameObject keys_prefab;
    [SerializeField] GameObject document_prefab;
    [SerializeField] GameObject flashlight_prefab;

    [Header("Throwing Settings")]
    [SerializeField] bool enableDropping = true;
    [SerializeField] GameObject throwObject_gameobject; // Keep original name for compatibility
    [SerializeField] float throwForce = 5f;

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>();
    private GameObject spawnedHeldVisual; // runtime instance of selected item's held visual
    private Coroutine selectedItemNameRoutine;

    [System.Serializable]
    public class KeyVisualMapping
    {
        public KeyItemsSO keyItem;
        public GameObject keyObject;
    }

    [Header("Key Visuals (Optional)")]
    [SerializeField] private List<KeyVisualMapping> keyVisuals = new List<KeyVisualMapping>();
    private readonly Dictionary<int, GameObject> keyVisualById = new Dictionary<int, GameObject>();

    void Start()
    {
        // Initialize item dictionary
        InitializeItemDictionary();

        // Set initial UI state
        UpdateInventoryUI();

        // If we start with items, select the first one
        if (inventoryList.Count > 0)
        {
            selectedItem = 0;
            NewItemSelected();
        }
        else
        {
            DeactivateAllItems();
        }
    }

    void Update()
    {
        HandleItemThrowing();
        HandleItemPickup();
        HandleItemSelection();
        UpdateInventoryUI();
    }

    private void InitializeItemDictionary()
    {
        itemSetActive.Clear();

        if (keys_item != null)
        {
            itemSetActive.Add(itemType.Keys, keys_item);
            SanitizeAnchor(keys_item);
        }
        if (document_item != null)
        {
            itemSetActive.Add(itemType.Document, document_item);
            SanitizeAnchor(document_item);
        }
        if (flashlight_item != null)
        {
            itemSetActive.Add(itemType.Flashlight, flashlight_item);
            SanitizeAnchor(flashlight_item);
        }

        // Initially deactivate all items
        DeactivateAllItems();

        // Build key visual lookup and hide visuals initially
        keyVisualById.Clear();
        for (int i = 0; i < keyVisuals.Count; i++)
        {
            var entry = keyVisuals[i];
            if (entry != null && entry.keyItem != null && entry.keyObject != null)
            {
                int id = entry.keyItem.itemID;
                if (!keyVisualById.ContainsKey(id))
                {
                    keyVisualById.Add(id, entry.keyObject);
                    entry.keyObject.SetActive(false);
                    TrySetIgnoreRaycastLayer(entry.keyObject);
                }
            }
        }
    }

    private void HandleItemThrowing()
    {
        if (!enableDropping) return;
        if (Input.GetKeyDown(throwItemKey) && HasItems())
        {
            KeyItemsSO itemToThrow = inventoryList[selectedItem];

            // Drop item with physics
            DropItemWithPhysics(itemToThrow);

            // Remove from inventory
            inventoryList.RemoveAt(selectedItem);

            // Adjust selected item index
            if (selectedItem >= inventoryList.Count && inventoryList.Count > 0)
            {
                selectedItem = inventoryList.Count - 1;
            }

            // Update selected item
            if (HasItems())
            {
                NewItemSelected();
            }
            else
            {
                DeactivateAllItems();
            }
        }
    }

    private void HandleItemPickup()
    {
        // Ensure we have a camera; fall back to Camera.main if not assigned
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        // Raycast from screen center to behave like a crosshair pickup
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, playerReach))
        {
            // Support colliders on child objects with the pickable script on parent
            IPickable pickableItem = hitInfo.collider.GetComponentInParent<IPickable>();
            ItemsPickable pickableComponent = hitInfo.collider.GetComponentInParent<ItemsPickable>();

            if (pickableItem != null && pickableComponent != null)
            {
                if (pressToPickup_gameobject != null)
                    pressToPickup_gameobject.SetActive(true);

                if (Input.GetKeyDown(pickUpItemKey))
                {
                    TryPickupItem(pickableComponent, pickableItem);
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

    private void TryPickupItem(ItemsPickable pickableComponent, IPickable pickableItem)
    {
        // Check if inventory is full
        if (inventoryList.Count >= maxInventorySize)
        {
            Debug.Log("Inventory is full!");
            return;
        }

        // Check if item has valid ScriptableObject
        if (pickableComponent.itemScriptableObject == null)
        {
            Debug.LogError("Item has no ScriptableObject assigned!");
            return;
        }

        // Add item to inventory
        inventoryList.Add(pickableComponent.itemScriptableObject);

        // Pick up the item (destroys GameObject)
        pickableItem.PickItem();

        // If this is the first item, select it
        if (inventoryList.Count == 1)
        {
            selectedItem = 0;
            NewItemSelected();
        }

        Debug.Log($"Picked up: {pickableComponent.itemScriptableObject.itemName}");
    }

    private void HandleItemSelection()
    {
        int newSelection = -1;

        // Number keys 1-9 select slot index 0-8 within maxInventorySize
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

    private void NewItemSelected()
    {
        if (!HasItems())
        {
            DeactivateAllItems();
            DestroySpawnedHeldVisual();
            UpdateSelectedItemNameUI();
            return;
        }

        // Clamp selected item to slot range, not inventory count, so empty slots are selectable
        selectedItem = Mathf.Clamp(selectedItem, 0, Mathf.Max(0, maxInventorySize - 1));

        DeactivateAllItems();

        KeyItemsSO currentItem = (selectedItem >= 0 && selectedItem < inventoryList.Count) ? inventoryList[selectedItem] : null;
        if (currentItem != null && itemSetActive.ContainsKey(currentItem.item_type))
        {
            GameObject itemObject = itemSetActive[currentItem.item_type];
            if (itemObject != null)
            {
                itemObject.SetActive(true);
            }

            if (currentItem.item_type == itemType.Keys)
            {
                ShowKeyVisual(currentItem, itemObject);
            }
            else
            {
                TrySpawnHeldVisual(currentItem, itemObject);
            }
        }
        else
        {
            if (currentItem != null && currentItem.item_type == itemType.Keys)
            {
                HideAllKeyVisuals();
            }
            TrySpawnHeldVisual(currentItem, null);
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
                if (i < inventoryList.Count && inventoryList[i] != null && inventoryList[i].item_sprite != null)
                {
                    inventorySlotImage[i].sprite = inventoryList[i].item_sprite;
                }
                else
                {
                    inventorySlotImage[i].sprite = prazdnySlotImage;
                }
            }
        }

        // Update background colors for selection
        for (int i = 0; i < inventoryBackgroundImage.Length; i++)
        {
            if (inventoryBackgroundImage[i] != null)
            {
                if (i == selectedItem)
                {
                    inventoryBackgroundImage[i].color = new Color32(145, 255, 126, 255); // Green for selected
                }
                else
                {
                    inventoryBackgroundImage[i].color = new Color32(219, 219, 219, 255); // Default gray
                }
            }
        }
    }

    // Helper methods for door integration
    public bool HasKey(int keyID)
    {
        for (int i = 0; i < inventoryList.Count; i++)
        {
            var item = inventoryList[i];
            if (item != null && item.item_type == itemType.Keys && item.itemID == keyID)
                return true;
        }
        return false;
    }

    public bool ConsumeKey(int keyID)
    {
        for (int i = 0; i < inventoryList.Count; i++)
        {
            var item = inventoryList[i];
            if (item != null && item.item_type == itemType.Keys && item.itemID == keyID)
            {
                if (keyVisualById.TryGetValue(item.itemID, out var visual) && visual != null)
                {
                    visual.SetActive(false);
                }
                inventoryList.RemoveAt(i);
                // Adjust selected index if needed
                if (selectedItem >= inventoryList.Count)
                    selectedItem = Mathf.Max(0, inventoryList.Count - 1);
                NewItemSelected();
                return true;
            }
        }
        return false;
    }

    private void DropItemWithPhysics(KeyItemsSO itemToDrop)
    {
        GameObject prefabToThrow = GetPrefabForItem(itemToDrop);
        if (prefabToThrow == null) return;

        // Calculate throw position - use throwObject_gameobject position or camera position
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

        // Instantiate the item
        GameObject droppedItem = Instantiate(prefabToThrow, throwPosition, Quaternion.identity);

        // Set up the dropped item's ScriptableObject reference
        ItemsPickable droppedPickable = droppedItem.GetComponent<ItemsPickable>();
        if (droppedPickable != null)
        {
            droppedPickable.itemScriptableObject = itemToDrop;
        }

        // Configure physics
        Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = droppedItem.AddComponent<Rigidbody>();
        }

        // Set physics properties
        rb.mass = GetItemMass(itemToDrop.item_type);

        // Use linearDamping and angularDamping for newer Unity versions
        if (rb.linearDamping == 0) rb.linearDamping = 1f;
        if (rb.angularDamping == 0) rb.angularDamping = 5f;

        // Apply throw force
        Vector3 throwDirection;
        if (cam != null)
        {
            throwDirection = (cam.transform.forward + Vector3.up * 0.3f).normalized;
        }
        else
        {
            throwDirection = (transform.forward + Vector3.up * 0.3f).normalized;
        }

        rb.AddForce(throwDirection * throwForce, ForceMode.VelocityChange);

        // Add some random spin
        rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.VelocityChange);
    }

    private GameObject GetPrefabForItem(KeyItemsSO item)
    {
        switch (item.item_type)
        {
            case itemType.Keys: return keys_prefab;
            case itemType.Document: return document_prefab;
            case itemType.Flashlight: return flashlight_prefab;
            default: return null;
        }
    }

    private float GetItemMass(itemType type)
    {
        switch (type)
        {
            case itemType.Keys: return 0.2f;
            case itemType.Document: return 0.1f;
            case itemType.Flashlight: return 0.8f;
            default: return 0.2f;
        }
    }

    private void DeactivateAllItems()
    {
        if (keys_item != null) keys_item.SetActive(false);
        if (document_item != null) document_item.SetActive(false);
        if (flashlight_item != null) flashlight_item.SetActive(false);
        DestroySpawnedHeldVisual();
        HideAllKeyVisuals();
    }

    private void UpdateSelectedItemNameUI()
    {
        if (selectedItemNameText == null)
            return;

        if (!HasItems() || selectedItem < 0 || selectedItem >= inventoryList.Count || inventoryList[selectedItem] == null)
        {
            selectedItemNameText.text = string.Empty;
            selectedItemNameText.enabled = false;
            return;
        }

        selectedItemNameText.text = inventoryList[selectedItem].itemName;
        if (!selectedItemNameText.enabled) selectedItemNameText.enabled = true;
    }

    private void TrySpawnHeldVisual(KeyItemsSO item, GameObject anchor)
    {
        DestroySpawnedHeldVisual();
        if (item == null || item.heldVisualPrefab == null || anchor == null) return;
        // Ensure the anchor and its children do not block interaction/physics
        DisablePhysicsOnHierarchy(anchor);
        spawnedHeldVisual = Instantiate(item.heldVisualPrefab, anchor.transform);
        spawnedHeldVisual.transform.localPosition = Vector3.zero;
        spawnedHeldVisual.transform.localRotation = Quaternion.identity;
        spawnedHeldVisual.transform.localScale = Vector3.one;
        // Ensure no physics on held visual
        var rb = spawnedHeldVisual.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
        var collider = spawnedHeldVisual.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        DisablePhysicsOnHierarchy(spawnedHeldVisual);
        TrySetIgnoreRaycastLayer(spawnedHeldVisual);
    }

    private void ShowKeyVisual(KeyItemsSO item, GameObject anchor)
    {
        HideAllKeyVisuals();
        DestroySpawnedHeldVisual();
        if (item == null)
        {
            return;
        }

        if (keyVisualById.TryGetValue(item.itemID, out var visual) && visual != null)
        {
            visual.SetActive(true);
            TrySetIgnoreRaycastLayer(visual);
        }
        else if (anchor != null)
        {
            TrySpawnHeldVisual(item, anchor);
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
                Debug.LogWarning($"PlayerInventory: Anchor '{anchor.name}' has a Collider. Consider removing it or placing the anchor on a child without gameplay colliders to avoid interaction issues.");
            }
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

    private void ShowSelectedNameTemporarily()
    {
        if (selectedItemNameText == null) return;
        if (selectedItemNameRoutine != null)
        {
            StopCoroutine(selectedItemNameRoutine);
            selectedItemNameRoutine = null;
        }
        if (!HasItems() || selectedItem < 0 || selectedItem >= inventoryList.Count || inventoryList[selectedItem] == null)
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

    private bool HasItems()
    {
        return inventoryList != null && inventoryList.Count > 0;
    }

    // Public methods for other scripts to use
    public bool HasItem(int itemID)
    {
        foreach (KeyItemsSO item in inventoryList)
        {
            if (item != null && item.itemID == itemID)
                return true;
        }
        return false;
    }

    public bool HasItemOfType(itemType type)
    {
        foreach (KeyItemsSO item in inventoryList)
        {
            if (item != null && item.item_type == type)
                return true;
        }
        return false;
    }

    public KeyItemsSO GetCurrentItem()
    {
        if (HasItems() && selectedItem >= 0 && selectedItem < inventoryList.Count)
            return inventoryList[selectedItem];
        return null;
    }

    public List<KeyItemsSO> GetItemsOfType(itemType type)
    {
        List<KeyItemsSO> items = new List<KeyItemsSO>();
        foreach (KeyItemsSO item in inventoryList)
        {
            if (item != null && item.item_type == type)
                items.Add(item);
        }
        return items;
    }
}
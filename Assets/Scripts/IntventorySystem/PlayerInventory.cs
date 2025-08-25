using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] GameObject throwObject_gameobject; // Keep original name for compatibility
    [SerializeField] float throwForce = 5f;

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>();

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

        if (keys_item != null) itemSetActive.Add(itemType.Keys, keys_item);
        if (document_item != null) itemSetActive.Add(itemType.Document, document_item);
        if (flashlight_item != null) itemSetActive.Add(itemType.Flashlight, flashlight_item);

        // Initially deactivate all items
        DeactivateAllItems();
    }

    private void HandleItemThrowing()
    {
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
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, playerReach))
        {
            IPickable pickableItem = hitInfo.collider.GetComponent<IPickable>();
            ItemsPickable pickableComponent = hitInfo.collider.GetComponent<ItemsPickable>();

            if (pickableItem != null && pickableComponent != null)
            {
                // Show pickup prompt
                if (pressToPickup_gameobject != null)
                    pressToPickup_gameobject.SetActive(true);

                // Handle pickup input
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
        if (!HasItems()) return;

        int newSelection = -1;

        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryList.Count > 0) newSelection = 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryList.Count > 1) newSelection = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryList.Count > 2) newSelection = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4) && inventoryList.Count > 3) newSelection = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5) && inventoryList.Count > 4) newSelection = 4;

        if (newSelection != -1 && newSelection != selectedItem)
        {
            selectedItem = newSelection;
            NewItemSelected();
        }
    }

    private void NewItemSelected()
    {
        if (!HasItems())
        {
            DeactivateAllItems();
            return;
        }

        // Clamp selected item to valid range
        selectedItem = Mathf.Clamp(selectedItem, 0, inventoryList.Count - 1);

        DeactivateAllItems();

        KeyItemsSO currentItem = inventoryList[selectedItem];
        if (currentItem != null && itemSetActive.ContainsKey(currentItem.item_type))
        {
            GameObject itemObject = itemSetActive[currentItem.item_type];
            if (itemObject != null)
            {
                itemObject.SetActive(true);
            }
        }
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
                if (i == selectedItem && HasItems())
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
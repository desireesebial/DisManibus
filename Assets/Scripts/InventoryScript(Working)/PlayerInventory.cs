using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{
    public List<KeyItemsSO> inventoryList = new List<KeyItemsSO>(); // Store ScriptableObjects instead of itemType
    public int playerReach;
    [SerializeField] Camera cam;
    [SerializeField] GameObject pressToPickup_gameobject;
    [SerializeField] Image[] inventorySlotImage = new Image[5];
    [SerializeField] Image[] inventoryBackgroundImage = new Image[5];
    [SerializeField] Sprite prazdnySlotImage;
    [SerializeField] GameObject throwObject_gameobject;
    [SerializeField] KeyCode throwItemKey;
    [SerializeField] KeyCode pickUpItemKey;

    public int selectedItem = 0;

    [Space(10)]
    [Header("Player Item GameObjects")]
    [SerializeField] GameObject keys_item;
    [SerializeField] GameObject document_item;
    [SerializeField] GameObject flashlight_item;

    [Header("Item Prefabs for Dropping")]
    [SerializeField] GameObject keys_prefab;
    [SerializeField] GameObject document_prefab;
    [SerializeField] GameObject flashlight_prefab;

    private Dictionary<itemType, GameObject> itemSetActive = new Dictionary<itemType, GameObject>();
    private Dictionary<KeyItemsSO, GameObject> itemInstantiate = new Dictionary<KeyItemsSO, GameObject>();

    void Start()
    {
        // Initialize dictionaries for item management
        itemSetActive.Add(itemType.Keys, keys_item);
        itemSetActive.Add(itemType.Document, document_item);
        itemSetActive.Add(itemType.Flashlight, flashlight_item);

        if (inventoryList.Count > 0)
        {
            NewItemSelected();
        }
    }

    void Update()
    {
        // Throw item
        if (Input.GetKeyDown(throwItemKey) && inventoryList.Count > 0)
        {
            KeyItemsSO itemToThrow = inventoryList[selectedItem];

            // Find the appropriate prefab to instantiate
            GameObject prefabToThrow = GetPrefabForItem(itemToThrow);
            if (prefabToThrow != null)
            {
                Instantiate(prefabToThrow, throwObject_gameobject.transform.position, Quaternion.identity);
            }

            inventoryList.RemoveAt(selectedItem);

            if (selectedItem >= inventoryList.Count && inventoryList.Count > 0)
            {
                selectedItem = inventoryList.Count - 1;
            }

            if (inventoryList.Count > 0)
            {
                NewItemSelected();
            }
            else
            {
                DeactivateAllItems();
            }
        }

        // Update inventory UI
        UpdateInventoryUI();

        // Handle item pickup
        HandleItemPickup();

        // Handle item selection
        HandleItemSelection();
    }

    private void UpdateInventoryUI()
    {
        // Update inventory slot images
        for (int i = 0; i < inventorySlotImage.Length; i++)
        {
            if (i < inventoryList.Count && inventoryList[i] != null)
            {
                inventorySlotImage[i].sprite = inventoryList[i].item_sprite;
            }
            else
            {
                inventorySlotImage[i].sprite = prazdnySlotImage;
            }
        }

        // Update background colors
        for (int i = 0; i < inventoryBackgroundImage.Length; i++)
        {
            if (i == selectedItem && inventoryList.Count > 0)
            {
                inventoryBackgroundImage[i].color = new Color32(145, 255, 126, 255);
            }
            else
            {
                inventoryBackgroundImage[i].color = new Color32(219, 219, 219, 255);
            }
        }
    }

    private void HandleItemPickup()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, playerReach))
        {
            IPickable item = hitInfo.collider.GetComponent<IPickable>();
            if (item != null)
            {
                pressToPickup_gameobject.SetActive(true);

                if (Input.GetKeyDown(pickUpItemKey))
                {
                    ItemsPickable pickableItem = hitInfo.collider.GetComponent<ItemsPickable>();
                    if (pickableItem != null && pickableItem.itemScriptableObject != null)
                    {
                        inventoryList.Add(pickableItem.itemScriptableObject);
                        item.PickItem();

                        if (inventoryList.Count == 1)
                        {
                            selectedItem = 0;
                            NewItemSelected();
                        }
                    }
                }
            }
            else
            {
                pressToPickup_gameobject.SetActive(false);
            }
        }
        else
        {
            pressToPickup_gameobject.SetActive(false);
        }
    }

    private void HandleItemSelection()
    {
        if (inventoryList.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && inventoryList.Count > 0)
        {
            selectedItem = 0;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && inventoryList.Count > 1)
        {
            selectedItem = 1;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && inventoryList.Count > 2)
        {
            selectedItem = 2;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && inventoryList.Count > 3)
        {
            selectedItem = 3;
            NewItemSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && inventoryList.Count > 4)
        {
            selectedItem = 4;
            NewItemSelected();
        }
    }

    private void NewItemSelected()
    {
        if (inventoryList.Count == 0) return;

        DeactivateAllItems();

        KeyItemsSO currentItem = inventoryList[selectedItem];
        if (currentItem != null)
        {
            GameObject selectedItemGameobject = itemSetActive[currentItem.item_type];
            selectedItemGameobject.SetActive(true);
        }
    }

    private GameObject GetPrefabForItem(KeyItemsSO item)
    {
        // You'll need to assign these in the inspector or create a dictionary
        switch (item.item_type)
        {
            case itemType.Keys:
                return keys_prefab;
            case itemType.Document:
                return document_prefab;
            case itemType.Flashlight:
                return flashlight_prefab;
            default:
                return null;
        }
    }

    private void DeactivateAllItems()
    {
        keys_item.SetActive(false);
        document_item.SetActive(false);
        flashlight_item.SetActive(false);
    }
}

public interface IPickable
{
    void PickItem();
}
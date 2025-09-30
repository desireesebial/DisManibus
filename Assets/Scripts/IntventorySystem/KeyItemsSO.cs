using UnityEngine;

[CreateAssetMenu(fileName = "KeyItems", menuName = "Scriptable Objects/KeyItems")]
public class KeyItemsSO : ScriptableObject
{
    [Header("Properties")]
    public float cooldown;
    public itemType item_type;
    public Sprite item_sprite;
    public string itemName; // "House Key", "Car Key", etc.
    public int itemID; // Unique identifier for different keys
    public string description; // Optional description

    [Header("Held Visual (Optional)")]
    [Tooltip("Prefab to show in the player's hand when this item is selected.")]
    public GameObject heldVisualPrefab; // Specific hand model for this key
}

public enum itemType { Keys, Document, Flashlight }
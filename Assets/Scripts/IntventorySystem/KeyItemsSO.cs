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
}

public enum itemType { Keys, Document, Flashlight }
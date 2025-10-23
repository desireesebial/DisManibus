using UnityEngine;

public class KeyPickable : MonoBehaviour, IPickable
{
    [Header("Key Settings")]
    public KeySO keyData;
    public bool isPickedUp = false;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    void Start()
    {
        // Validate setup
        if (keyData == null)
        {
            Debug.LogError($"KeyPickable on {gameObject.name}: No KeySO assigned to keyData field!");
        }
        else if (enableDebugLogs)
        {
            Debug.Log($"KeyPickable on {gameObject.name}: Ready to pickup key '{keyData.keyName}' (ID: {keyData.keyId})");
        }

        // Ensure we have a collider
        if (GetComponent<Collider>() == null)
        {
            Debug.LogError($"KeyPickable on {gameObject.name}: No Collider found! Keys need a Collider for raycast detection.");
        }
    }

    // Implementation of IPickable interface
    // Called by DullahanHeadInventory when player presses pickup key while targeting this key
    public void PickItem()
    {
        if (isPickedUp)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"KeyPickable on {gameObject.name}: Already picked up!");
            return;
        }

        if (keyData == null)
        {
            Debug.LogError($"KeyPickable on {gameObject.name}: Cannot pickup - no KeySO assigned!");
            return;
        }

        isPickedUp = true;

        if (enableDebugLogs)
            Debug.Log($"KeyPickable on {gameObject.name}: Picked up key '{keyData.keyName}' (ID: {keyData.keyId})");

        // Destroy the key GameObject
        Destroy(gameObject);
    }

    // Helper method to check if this key can be picked up
    public bool CanBePickedUp()
    {
        return !isPickedUp && keyData != null;
    }
}




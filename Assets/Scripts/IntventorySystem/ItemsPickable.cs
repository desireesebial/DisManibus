using UnityEngine;

public class ItemsPickable : MonoBehaviour, IPickable
{
    [Header("Item Configuration")]
    public KeyItemsSO itemScriptableObject;

    [Header("Visual Feedback")]
    public GameObject highlightEffect; // Optional highlight when player is near

    private void Start()
    {
        // Validate that we have a ScriptableObject assigned
        if (itemScriptableObject == null)
        {
            Debug.LogError($"ItemsPickable on {gameObject.name} is missing itemScriptableObject reference!");
        }
    }

    public void PickItem()
    {
        // Mark as picked up in save system (if PersistentItem component exists)
        var persistentItem = GetComponent<SaveLoad.PersistentItem>();
        if (persistentItem != null)
        {
            persistentItem.MarkAsPickedUp();
        }

        // Add pickup effect here if needed
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }

        Destroy(gameObject);
    }

    // Optional: Show highlight when player is near
    public void SetHighlight(bool active)
    {
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(active);
        }
    }
}
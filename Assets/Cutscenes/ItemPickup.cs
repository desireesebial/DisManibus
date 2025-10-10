using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    public string itemName = "flashlight";
    public KeyCode interactKey = KeyCode.E;
    public bool destroyOnPickup = true;
    public bool once = true;

    bool inRange, taken;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) inRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) inRange = false;
    }

    void Update()
    {
        if (!inRange || taken) return;
        if (Input.GetKeyDown(interactKey))
        {
            if (ItemTracker.Instance) ItemTracker.Instance.RegisterItem(itemName);
            taken = once;
            if (destroyOnPickup) Destroy(gameObject);
            else enabled = false;
        }
    }
}

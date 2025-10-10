using UnityEngine;

public class StartItemQuest : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ItemTracker.Instance != null)
            ItemTracker.Instance.StartItemsQuest();
    }
}

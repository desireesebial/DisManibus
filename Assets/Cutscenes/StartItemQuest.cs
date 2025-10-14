using UnityEngine;

public class StartItemQuest : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ItemTracker.Instance != null)
        {
            ItemTracker.Instance.StartItemsQuest();

            // 🔽 Add this listener
            ItemTracker.Instance.OnCompleted += OnItemsQuestCompleted;
        }
    }

    void OnItemsQuestCompleted()
    {
        Debug.Log("✅ All items collected — quest complete!");
        QuestManager.Instance?.CompleteQuest("find items");
    }
}

using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public enum Action { Add, Complete, Remove }
    public Action action = Action.Complete;

    [Tooltip("Quest id (unique key), e.g., 'find_key'")]
    public string questId;

    [TextArea] public string questTextIfAdding;

    // Call from UnityEvent, OnTriggerEnter, or another script.
    public void Fire()
    {
        var qm = QuestManager.Instance;
        if (!qm) return;

        switch (action)
        {
            case Action.Add:
                qm.AddQuest(questId, questTextIfAdding);
                break;
            case Action.Complete:
                qm.CompleteQuest(questId);
                break;
            case Action.Remove:
                qm.RemoveQuest(questId);
                break;
        }
    }

    // Example trigger (optional): auto-fire when player enters
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Fire();
        // If you only want it once:
        // Destroy(this);
    }
}

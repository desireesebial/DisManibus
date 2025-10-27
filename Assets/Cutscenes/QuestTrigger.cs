using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public enum Action { Add, Complete, Remove }
    public Action action = Action.Complete;

    [Tooltip("Quest id (unique key), e.g., 'find_key'")]
    public string questId;

    [TextArea] public string questTextIfAdding;

    [Tooltip("If true, this trigger will only fire once and then disable itself")]
    public bool fireOnce = true;

    private bool hasFired = false;

    // Call from UnityEvent, OnTriggerEnter, or another script.
    public void Fire()
    {
        // Prevent firing multiple times if fireOnce is enabled
        if (fireOnce && hasFired) return;

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

        // Mark as fired and disable if needed
        if (fireOnce)
        {
            hasFired = true;
            enabled = false; // Disable this component to prevent further triggers
        }
    }

    // Example trigger (optional): auto-fire when player enters
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Skip if already fired and fireOnce is enabled
        if (fireOnce && hasFired) return;

        Fire();
    }
}

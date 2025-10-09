using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SetFlagOnTrigger : MonoBehaviour
{
    public string flagToSet = "quest_find_clues_started";
    public bool once = true;

    bool done;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (done && once) return;
        if (!other.CompareTag("Player")) return;

        DialogueFlags.Set(flagToSet); // uses the tiny DialogueFlags helper you already have
        done = true;
    }
}

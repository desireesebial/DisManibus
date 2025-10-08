using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueInteract : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)] public string[] defaultLines;      // normal lines
    [TextArea(2, 4)] public string[] unlockedLines;     // lines shown after flag is unlocked

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public bool mustFinishBeforeNext = true;
    public bool avoidImmediateRepeat = true;

    [Header("Flag Settings")]
    public string unlockFlag;          // which flag unlocks these new lines
    public string setFlagWhenUsed;     // which flag this object sets when interacted

    bool inRange, typing;
    int lastIndex = -1;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!TryGetComponent<Rigidbody>(out var rb))
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) inRange = true; }
    void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) inRange = false; }

    void Update()
    {
        if (!inRange) return;
        if (mustFinishBeforeNext && typing) return;

        if (Input.GetKeyDown(interactKey))
            ShowRandomLine();
    }

    void ShowRandomLine()
    {
        // Choose which pool to use
        string[] pool = (DialogueFlags.Has(unlockFlag) && unlockedLines.Length > 0)
                        ? unlockedLines
                        : defaultLines;

        if (pool == null || pool.Length == 0) return;

        int index = Random.Range(0, pool.Length);
        if (avoidImmediateRepeat && pool.Length > 1)
            while (index == lastIndex)
                index = Random.Range(0, pool.Length);

        lastIndex = index;
        StartCoroutine(TypeAndUnlock(pool[index]));
    }

    IEnumerator TypeAndUnlock(string line)
    {
        typing = true;
        dialogueManager.ShowLine(line);
        yield return new WaitUntil(() => dialogueManager.lineFinished);
        typing = false;

        if (!string.IsNullOrEmpty(setFlagWhenUsed))
            DialogueFlags.Set(setFlagWhenUsed);
    }
}

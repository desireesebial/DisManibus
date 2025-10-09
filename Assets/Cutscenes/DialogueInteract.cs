using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueInteract : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;   // Assign in Inspector (auto-finds if left empty)

    [Header("Dialogue Lines")]
    [TextArea(2, 4)] public string[] defaultLines;   // before flag
    [TextArea(2, 4)] public string[] unlockedLines;  // after flag

    [Header("Flag Settings")]
    public string unlockFlag;                 // e.g. "clue_blue_found" (leave empty if not gating)
    public string setFlagWhenUsed;            // e.g. "clue_blue_found" (leave empty if not setting)

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public bool mustFinishBeforeNext = true;  // ignore presses while typing
    public bool avoidImmediateRepeat = true;  // prevent same random line twice in a row
    public bool randomize = false;            // if false: always pick index 0 (ordered)

    [Header("Clear Settings")]
    public bool clearAfterEachLine = true;    // auto-clear after showing a line
    public float clearDelay = 1f;             // delay before clearing (seconds, scaled time)
    public bool clearOnExit = true;           // clear text when player leaves trigger

    // internal state
    bool inRange;
    bool typing;
    int lastIndex = -1;
    bool hasSetFlag = false;

    void Reset()
    {
        // Make sure this collider is a trigger and has a kinematic RB so CharacterController triggers fire reliably
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void Awake()
    {
        // Auto-find DialogueManager if not assigned
        if (!dialogueManager)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (!dialogueManager)
                Debug.LogError("[DialogueInteract] No DialogueManager found in scene. Assign one in the Inspector.", this);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            inRange = false;
            if (clearOnExit && dialogueManager)
                dialogueManager.ClearLine();
        }
    }

    void Update()
    {
        if (!inRange) return;
        if (mustFinishBeforeNext && typing) return;

        if (Input.GetKeyDown(interactKey))
            ShowOneLine();
    }

    void ShowOneLine()
    {
        if (!dialogueManager)
        {
            Debug.LogWarning("[DialogueInteract] dialogueManager is null – cannot show dialogue.", this);
            return;
        }

        // Choose the pool (flag-aware)
        string[] pool = (!string.IsNullOrEmpty(unlockFlag) && DialogueFlags.Has(unlockFlag))
                        ? unlockedLines
                        : defaultLines;

        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning("[DialogueInteract] No lines to show (pool empty).", this);
            return;
        }

        // Pick index
        int index = randomize ? Random.Range(0, pool.Length) : 0;
        if (randomize && avoidImmediateRepeat && pool.Length > 1)
        {
            while (index == lastIndex)
                index = Random.Range(0, pool.Length);
        }
        lastIndex = index;

        StopAllCoroutines();
        StartCoroutine(TypeAndUnlock(pool[index]));
    }

    IEnumerator TypeAndUnlock(string line)
    {
        typing = true;

        // Guard again in case DM got destroyed mid-game
        if (!dialogueManager)
        {
            typing = false;
            yield break;
        }

        dialogueManager.ShowLine(line);
        yield return new WaitUntil(() => dialogueManager.lineFinished);
        typing = false;

        // Optionally set a flag once after a successful interaction
        if (!hasSetFlag && !string.IsNullOrEmpty(setFlagWhenUsed))
        {
            DialogueFlags.Set(setFlagWhenUsed);
            hasSetFlag = true;
        }

        // Optionally clear after a short delay
        if (clearAfterEachLine && dialogueManager)
        {
            if (clearDelay > 0f)
                yield return new WaitForSeconds(clearDelay); // uses scaled time (consistent with rest of your project)
            dialogueManager.ClearLine();
        }
    }

    // Optional public helper if other scripts (e.g., a paper system) want to force a line now.
    public void PlayNow(bool preferUnlocked = true)
    {
        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();
        if (!dialogueManager) return;

        string[] pool = (preferUnlocked && !string.IsNullOrEmpty(unlockFlag) && DialogueFlags.Has(unlockFlag))
                        ? unlockedLines
                        : defaultLines;

        if (pool == null || pool.Length == 0) return;

        StopAllCoroutines();
        StartCoroutine(TypeAndUnlock(pool[0]));
    }
}

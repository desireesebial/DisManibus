using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueInteract : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;   // Assign or will auto-find

    [Header("Dialogue Lines")]
    [TextArea(2, 4)] public string[] defaultLines;
    [TextArea(2, 4)] public string[] unlockedLines;

    [Header("Flag Settings")]
    [Tooltip("If set, unlockedLines are used when this flag exists.")]
    public string unlockFlag;
    [Tooltip("If set, this flag will be set when the player interacts.")]
    public string setFlagWhenUsed;
    [Tooltip("If true, the flag is set every time; otherwise, only once.")]
    public bool setFlagEveryTime = true;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public bool mustFinishBeforeNext = true;

    [Header("Pick Mode")]
    public bool playSequentially = true;
    public bool wrapSequence = false;
    public bool randomize = false;
    public bool avoidImmediateRepeat = true;

    [Header("Clear Settings")]
    public bool clearAfterEachLine = true;
    public float clearDelay = 1f;
    public bool clearOnExit = true;

    // internal
    bool inRange;
    bool typing;
    int lastIndex = -1;
    int seqDefault = 0;
    int seqUnlocked = 0;
    bool hasSetFlag = false;

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

    void Awake()
    {
        if (!dialogueManager)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (!dialogueManager)
                Debug.LogError("[DialogueInteract] No DialogueManager found in scene.", this);
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
        if (!dialogueManager) return;

        bool useUnlocked = (!string.IsNullOrEmpty(unlockFlag) && DialogueFlags.Has(unlockFlag));
        var pool = useUnlocked ? unlockedLines : defaultLines;
        if (pool == null || pool.Length == 0) return;

        int index = 0;
        if (randomize)
        {
            index = Random.Range(0, pool.Length);
            if (avoidImmediateRepeat && pool.Length > 1)
            {
                int guard = 0;
                while (index == lastIndex && guard++ < 10)
                    index = Random.Range(0, pool.Length);
            }
            lastIndex = index;
        }
        else if (playSequentially)
        {
            if (useUnlocked)
            {
                index = Mathf.Clamp(seqUnlocked, 0, pool.Length - 1);
                if (seqUnlocked < pool.Length - 1) seqUnlocked++;
                else if (wrapSequence) seqUnlocked = 0;
            }
            else
            {
                index = Mathf.Clamp(seqDefault, 0, pool.Length - 1);
                if (seqDefault < pool.Length - 1) seqDefault++;
                else if (wrapSequence) seqDefault = 0;
            }
        }
        else
        {
            index = 0;
        }

        StopAllCoroutines();
        StartCoroutine(TypeThenMaybeClear(pool[index]));
    }

    IEnumerator TypeThenMaybeClear(string line)
    {
        typing = true;

        dialogueManager.ShowLine(line);
        yield return new WaitUntil(() => dialogueManager.lineFinished);
        typing = false;

        if (!string.IsNullOrEmpty(setFlagWhenUsed))
        {
            if (setFlagEveryTime || !hasSetFlag)
            {
                DialogueFlags.Set(setFlagWhenUsed);
                hasSetFlag = true;
            }
        }

        if (clearAfterEachLine)
        {
            if (clearDelay > 0f) yield return new WaitForSeconds(clearDelay);
            dialogueManager.ClearLine();
        }
    }

    public void PlayNow(bool preferUnlocked = true)
    {
        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();
        if (!dialogueManager) return;

        bool useUnlocked = (preferUnlocked && !string.IsNullOrEmpty(unlockFlag) && DialogueFlags.Has(unlockFlag));
        var pool = useUnlocked ? unlockedLines : defaultLines;
        if (pool == null || pool.Length == 0) return;

        int index = 0;
        if (playSequentially)
        {
            if (useUnlocked)
            {
                index = Mathf.Clamp(seqUnlocked, 0, pool.Length - 1);
                if (seqUnlocked < pool.Length - 1) seqUnlocked++;
                else if (wrapSequence) seqUnlocked = 0;
            }
            else
            {
                index = Mathf.Clamp(seqDefault, 0, pool.Length - 1);
                if (seqDefault < pool.Length - 1) seqDefault++;
                else if (wrapSequence) seqDefault = 0;
            }
        }

        StopAllCoroutines();
        StartCoroutine(TypeThenMaybeClear(pool[index]));
    }
}

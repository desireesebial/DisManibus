using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DialogueInteract : MonoBehaviour
{
    public enum FlagTiming { BeforePlayback, AfterPlayback }

    [Header("References")]
    public DialogueManager dialogueManager;   // Auto-finds if left empty

    [Header("Dialogue Pools")]
    [TextArea(2, 4)] public string[] poolA;    // used depending on switch flag logic
    [TextArea(2, 4)] public string[] poolB;

    [Header("Switching Logic")]
    [Tooltip("Flag name that controls which pool is active, e.g. 'code_clue'.")]
    public string switchFlag = "code_clue";
    [Tooltip("If TRUE: Use Pool B when the flag is PRESENT, otherwise use Pool A.\nIf FALSE: Use Pool B when the flag is ABSENT, otherwise use Pool A.")]
    public bool usePoolBWhenFlagPresent = true;

    [Header("Flags To Set On Interact")]
    [Tooltip("Flags to set when the player interacts with this object.")]
    public string[] flagsToSetOnInteract;
    [Tooltip("When to set those flags (before the line is chosen, or after it finishes typing).")]
    public FlagTiming setFlagsWhen = FlagTiming.AfterPlayback;
    [Tooltip("Set each listed flag only once from this object.")]
    public bool setFlagsOnlyOnce = true;   // <-- correct name

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
    public float clearDelay = 1f;          // UN-SCALED seconds
    public bool clearOnExit = true;

    // ---- internal state ----
    bool inRange;
    bool typing;
    int lastIndex = -1;
    int seqA = 0;
    int seqB = 0;
    bool flagsSetAlready = false;

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
        if (other.CompareTag("Player")) inRange = true;
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
        {
            if (setFlagsWhen == FlagTiming.BeforePlayback)
                TrySetFlags();

            ShowOneLine();

            if (setFlagsWhen == FlagTiming.AfterPlayback && !mustFinishBeforeNext)
                TrySetFlags(); // immediate if overlapping presses allowed
        }
    }

    void ShowOneLine()
    {
        if (!dialogueManager) return;

        bool flagPresent = !string.IsNullOrEmpty(switchFlag) && DialogueFlags.Has(switchFlag);
        string[] active = (usePoolBWhenFlagPresent == flagPresent) ? poolB : poolA;

        if (active == null || active.Length == 0) return;

        int index = 0;
        if (randomize)
        {
            index = Random.Range(0, active.Length);
            if (avoidImmediateRepeat && active.Length > 1)
            {
                int guard = 0;
                while (index == lastIndex && guard++ < 10)
                    index = Random.Range(0, active.Length);
            }
            lastIndex = index;
        }
        else if (playSequentially)
        {
            if (active == poolA)
            {
                index = Mathf.Clamp(seqA, 0, active.Length - 1);
                if (seqA < active.Length - 1) seqA++;
                else if (wrapSequence) seqA = 0;
            }
            else
            {
                index = Mathf.Clamp(seqB, 0, active.Length - 1);
                if (seqB < active.Length - 1) seqB++;
                else if (wrapSequence) seqB = 0;
            }
        }
        else
        {
            index = 0;
        }

        StopAllCoroutines();
        StartCoroutine(TypeThenMaybeClear(active[index]));
    }

    IEnumerator TypeThenMaybeClear(string line)
    {
        typing = true;
        dialogueManager.ShowLine(line);
        yield return new WaitUntil(() => dialogueManager.lineFinished);
        typing = false;

        if (setFlagsWhen == FlagTiming.AfterPlayback)
            TrySetFlags();

        if (clearAfterEachLine)
        {
            if (clearDelay > 0f)
            {
                float t = 0f;
                while (t < clearDelay)
                {
                    t += Time.unscaledDeltaTime; // unscaled so it clears even if timeScale == 0
                    yield return null;
                }
            }
            dialogueManager.ClearLine();
        }
    }

    void TrySetFlags()
    {
        if (setFlagsOnlyOnce && flagsSetAlready) return;     // <-- fixed reference
        if (flagsToSetOnInteract == null || flagsToSetOnInteract.Length == 0) return;

        foreach (var f in flagsToSetOnInteract)
        {
            if (!string.IsNullOrEmpty(f))
                DialogueFlags.Set(f);
        }
        flagsSetAlready = true;
    }

    // Optional external trigger
    public void PlayNow(bool forceUsePoolB = false)
    {
        if (!dialogueManager)
            dialogueManager = FindObjectOfType<DialogueManager>();
        if (!dialogueManager) return;

        bool flagPresent = !string.IsNullOrEmpty(switchFlag) && DialogueFlags.Has(switchFlag);
        string[] active = forceUsePoolB ? poolB : (usePoolBWhenFlagPresent == flagPresent ? poolB : poolA);
        if (active == null || active.Length == 0) return;

        int index = 0;
        if (playSequentially)
        {
            if (active == poolA)
            {
                index = Mathf.Clamp(seqA, 0, active.Length - 1);
                if (seqA < active.Length - 1) seqA++;
                else if (wrapSequence) seqA = 0;
            }
            else
            {
                index = Mathf.Clamp(seqB, 0, active.Length - 1);
                if (seqB < active.Length - 1) seqB++;
                else if (wrapSequence) seqB = 0;
            }
        }

        StopAllCoroutines();
        StartCoroutine(TypeThenMaybeClear(active[index]));
    }
}

using UnityEngine;
using System.Collections;

public class PileDialogueRelay : MonoBehaviour
{
    [Header("Links")]
    public NotePilePickable pickable;      // drag your NotePilePickable here
    public DialogueManager dialogueManager; // drag your DialogueManager (Canvas)

    [Header("When to talk")]
    public bool onPileCleared = true;      // talk after the player finishes sifting
    public bool onCancelled = false;       // talk if the player cancels early

    [Header("Dialogue (ordered)")]
    [TextArea(2, 4)] public string[] linesBeforeFlag;   // shown if flag not set yet
    [TextArea(2, 4)] public string[] linesAfterFlag;    // shown if flag IS set

    [Header("Flags (optional)")]
    public string unlockFlag;              // check this to decide which set to use
    public string setFlagWhenDone;         // set this after finishing the dialogue

    [Header("Timing")]
    public float betweenLinesDelay = 0.75f;

    void Reset()
    {
        if (!pickable) pickable = GetComponent<NotePilePickable>();
        if (!dialogueManager) dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void OnEnable()
    {
        if (!pickable || pickable.pileSwiper == null) return;
        if (onPileCleared) pickable.pileSwiper.OnPileCleared += Handle;
        if (onCancelled) pickable.pileSwiper.OnCancelled += Handle;
    }

    void OnDisable()
    {
        if (!pickable || pickable.pileSwiper == null) return;
        if (onPileCleared) pickable.pileSwiper.OnPileCleared -= Handle;
        if (onCancelled) pickable.pileSwiper.OnCancelled -= Handle;
    }

    void Handle()
    {
        // Note: NotePilePickable already restores controls/canvas for you. :contentReference[oaicite:1]{index=1}
        var pool = (!string.IsNullOrEmpty(unlockFlag) && DialogueFlags.Has(unlockFlag))
                   ? linesAfterFlag : linesBeforeFlag;

        if (dialogueManager && pool != null && pool.Length > 0)
            StartCoroutine(PlaySequence(pool));
    }

    IEnumerator PlaySequence(string[] pool)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            dialogueManager.ShowLine(pool[i]);
            yield return new WaitUntil(() => dialogueManager.lineFinished);
            if (i < pool.Length - 1)
                yield return new WaitForSeconds(betweenLinesDelay);
        }

        // optional: set a flag once the whole sequence finishes
        if (!string.IsNullOrEmpty(setFlagWhenDone))
            DialogueFlags.Set(setFlagWhenDone);
    }
}

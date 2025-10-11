using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class ItemTracker : MonoBehaviour
{
    public static ItemTracker Instance { get; private set; }

    [Header("UI (reuse your Clue UI)")]
    public CanvasGroup clueUI;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI listText;

    [Header("Items to find")]
    [Tooltip("Names shown in UI; must match ItemPickup.itemName for auto check-offs.")]
    public List<string> requiredItems = new List<string> { "flashlight", "notebook" };

    [Header("Behavior")]
    public bool startHidden = true;
    public float fadeSpeed = 2f;
    public float autoHideDelayWhenComplete = 2f;

    [Header("Flags / Persistence")]
    [Tooltip("Prefix used for per-item flags. Final flag is prefix + normalizedName, e.g. got_flashlight")]
    public string perItemFlagPrefix = "got_";
    [Tooltip("Normalize to lowercase & trim before creating flags.")]
    public bool normalizeItemNames = true;

    [Header("Completion Hook")]
    [Tooltip("Optional: set this DialogueFlags flag when items are complete (for other systems/doors).")]
    public string completionFlag = "items_complete";
    public bool setCompletionFlag = true;

    // runtime
    private HashSet<string> collected = new HashSet<string>(); // stores normalized names
    public bool IsComplete => collected.Count >= requiredItems.Count;

    public System.Action OnCompleted;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (clueUI)
        {
            clueUI.alpha = startHidden ? 0f : 1f;
            clueUI.blocksRaycasts = !startHidden;
            clueUI.interactable = !startHidden;
        }
    }

    // --- Public API ---

    /// <summary>
    /// Show UI and rebuild collected items from flags. Works even if items were picked up earlier.
    /// </summary>
    public void StartItemsQuest()
    {
        // Rebuild from flags (do NOT clear progress)
        RebuildFromFlags();
        RefreshUI();
        FadeTo(1f);
    }

    public void Hide()
    {
        FadeTo(0f);
    }

    /// <summary>
    /// Register an item pick-up at runtime (called by ItemPickup).
    /// </summary>
    public void RegisterItem(string itemName)
    {
        var norm = Normalize(itemName);
        if (string.IsNullOrEmpty(norm)) return;

        // set per-item flag so progress survives if UI starts later
        DialogueFlags.Set(perItemFlagPrefix + norm);

        collected.Add(norm);
        RefreshUI();

        if (IsComplete)
        {
            OnCompleted?.Invoke();

            if (setCompletionFlag && !string.IsNullOrEmpty(completionFlag))
                DialogueFlags.Set(completionFlag);

            Invoke(nameof(Hide), autoHideDelayWhenComplete);
        }
    }

    // --- Helpers ---

    void RebuildFromFlags()
    {
        foreach (var name in requiredItems)
        {
            var norm = Normalize(name);
            if (DialogueFlags.Has(perItemFlagPrefix + norm))
                collected.Add(norm);
        }
    }

    string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        return normalizeItemNames ? s.Trim().ToLowerInvariant() : s.Trim();
    }

    void RefreshUI()
    {
        if (progressText)
            progressText.text = $"{collected.Count}/{requiredItems.Count} Items found";

        if (listText)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < requiredItems.Count; i++)
            {
                string display = requiredItems[i];
                string norm = Normalize(display);
                bool has = collected.Contains(norm);

                sb.Append("• ");
                if (has) sb.Append("<s>").Append(display).Append("</s>");
                else sb.Append(display);

                if (i < requiredItems.Count - 1) sb.AppendLine();
            }
            listText.text = sb.ToString();
        }
    }

    void FadeTo(float target)
    {
        if (!clueUI) return;
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(target));
    }

    System.Collections.IEnumerator FadeRoutine(float target)
    {
        while (!Mathf.Approximately(clueUI.alpha, target))
        {
            clueUI.alpha = Mathf.MoveTowards(clueUI.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
            clueUI.blocksRaycasts = clueUI.alpha > 0.001f;
            clueUI.interactable = clueUI.blocksRaycasts;
            yield return null;
        }
    }
}

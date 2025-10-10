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
    public List<string> requiredItems = new List<string> { "flashlight", "notebook" };

    [Header("Behavior")]
    public bool startHidden = true;
    public float fadeSpeed = 2f;
    public float autoHideDelayWhenComplete = 2f;

    [Header("Completion Hook")]
    [Tooltip("Optional: set this DialogueFlags flag when items are complete (for other systems).")]
    public string completionFlag = "items_complete";
    public bool setCompletionFlag = true;

    // runtime
    private HashSet<string> collected = new HashSet<string>();
    public bool IsComplete => collected.Count >= requiredItems.Count;

    public System.Action OnCompleted;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (clueUI) clueUI.alpha = startHidden ? 0f : 1f;
    }

    void Start()
    {
        if (!startHidden)
        {
            RefreshUI();
            FadeTo(1f);
        }
    }

    // --- Public API ---

    public void StartItemsQuest()
    {
        collected.Clear();
        RefreshUI();
        FadeTo(1f);
    }

    public void Hide()
    {
        FadeTo(0f);
    }

    public void RegisterItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return;
        if (!requiredItems.Contains(itemName)) requiredItems.Add(itemName);
        collected.Add(itemName);
        RefreshUI();

        if (IsComplete)
        {
            // notify & flag
            OnCompleted?.Invoke();
            if (setCompletionFlag && !string.IsNullOrEmpty(completionFlag))
                DialogueFlags.Set(completionFlag);

            Invoke(nameof(Hide), autoHideDelayWhenComplete);
        }
    }

    // --- UI helpers ---

    void RefreshUI()
    {
        if (progressText)
            progressText.text = $"{collected.Count}/{requiredItems.Count} Items found";

        if (listText)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < requiredItems.Count; i++)
            {
                string name = requiredItems[i];
                bool has = collected.Contains(name);
                sb.Append("• ");
                if (has) sb.Append("<s>").Append(name).Append("</s>");
                else sb.Append(name);
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
        if (!clueUI) yield break;
        while (!Mathf.Approximately(clueUI.alpha, target))
        {
            clueUI.alpha = Mathf.MoveTowards(clueUI.alpha, target, fadeSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }
}

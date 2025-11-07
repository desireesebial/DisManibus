using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NotePileSwiper : MonoBehaviour
{
    [Header("Pile Setup")]
    public List<RectTransform> pilePages = new List<RectTransform>();
    [Tooltip("The final relevant note to show when pile is cleared")] public NoteLetterSO finalNote;
    [Tooltip("Optional: Reference to Note UI to show final note")] public NoteLetterUI noteUI;

    [Header("Input")] 
    public KeyCode swipeLeftKey = KeyCode.A;
    public KeyCode swipeRightKey = KeyCode.D;
    public KeyCode swipeLeftAlt = KeyCode.LeftArrow;
    public KeyCode swipeRightAlt = KeyCode.RightArrow;

    [Header("Swipe Animation")] 
    public float swipeDistance = 600f;
    public float swipeDuration = 0.25f;
    public AnimationCurve swipeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float nextPagePopOffset = 10f;
    public float nextPagePopScale = 1.02f;

    [Header("Behavior")]
    [Tooltip("If true, the last remaining page cannot be swiped away. Use interaction to show the final note instead.")]
    public bool keepLastPage = true;

    [Header("Cancel / Resume")]
    public KeyCode cancelKey = KeyCode.Tab;
    [Tooltip("Persist remaining pages across sessions using PlayerPrefs")] public bool persistProgress = true;
    [Tooltip("Unique identifier for this pile to persist progress")] public string pileId = "DefaultPile";

    [Header("UI Control Guide")]
    [Tooltip("Optional: TextMeshProUGUI to display control instructions on screen")]
    public TextMeshProUGUI controlGuideText;
    [Tooltip("Show control guide when swiping is active")]
    public bool showControlGuide = true;
    [Tooltip("Custom control guide text. Leave empty for auto-generated text based on key bindings")]
    [TextArea(2, 4)]
    public string customControlText = "";

    [Header("Dialogue Integration")]
    [Tooltip("Reference to DialogueManager to show character thoughts")]
    public DialogueManager dialogueManager;
    [Tooltip("Character thoughts/subtitles for each page swipe (from first to last). Accumulated letters will be appended.")]
    public List<string> pageSubtitles = new List<string>();
    [Tooltip("After this many swipes, show the hint message")]
    public int showHintAfterSwipeCount = 4;
    [Tooltip("Hint message to show after specified swipes (about combining letters)")]
    [TextArea(2, 4)]
    public string hintMessage = "Wait... if I combine these letters from top to bottom...";

    [Header("State")]
    public bool isActive = false;

    private bool isAnimating = false;
    private Vector3 originalScale = Vector3.one;
    private bool clearedFired = false;
    private bool progressApplied = false;
    private List<RectTransform> allOriginalPages = new List<RectTransform>();
    private Dictionary<RectTransform, Vector2> originalPositions = new Dictionary<RectTransform, Vector2>();
    private Dictionary<RectTransform, Vector3> originalRotations = new Dictionary<RectTransform, Vector3>();
    private Dictionary<RectTransform, Vector3> originalScales = new Dictionary<RectTransform, Vector3>();
    private int swipeCount = 0;
    private string accumulatedLetters = "";
    private int initialPageCount = 0;

    // Fired once when the pile is fully cleared (after optional final note is shown)
    public System.Action OnPileCleared;
    // Fired when player cancels swiping (session ends early)
    public System.Action OnCancelled;

    void Awake()
    {
        if (noteUI == null)
        {
            noteUI = FindObjectOfType<NoteLetterUI>();
        }
    }

    void OnEnable()
    {
        // Normalize order: last index is top page
        NormalizeOrder();
        CacheOriginalScale();
        clearedFired = false;

        // Store all original pages and their transforms if not already stored
        if (allOriginalPages.Count == 0 && pilePages != null && pilePages.Count > 0)
        {
            StoreOriginalPages();
        }

        if (!progressApplied)
        {
            LoadAndApplyProgress();
            progressApplied = true;
        }

        // Reset dialogue tracking for fresh session
        swipeCount = 0;
        accumulatedLetters = "";
        initialPageCount = pilePages != null ? pilePages.Count : 0;

        // Initialize control guide (hide it initially, will show when ActivatePile(true) is called)
        HideControlGuide();
    }

    void Update()
    {
        if (!isActive || isAnimating) return;

        // Allow cancel
        if (Input.GetKeyDown(cancelKey))
        {
            Cancel();
            return;
        }

        if (pilePages.Count == 0)
        {
            if (!clearedFired)
            {
                // Finished; show final note if provided
                if (finalNote != null && noteUI != null && !noteUI.IsNoteVisible())
                {
                    noteUI.ShowNote(finalNote);
                }
                SaveProgress(); // persist 0 remaining
                clearedFired = true;
                OnPileCleared?.Invoke();
            }
            return;
        }

        if (Input.GetKeyDown(swipeLeftKey) || Input.GetKeyDown(swipeLeftAlt))
        {
            if (!(keepLastPage && pilePages.Count <= 1))
            {
                StartCoroutine(SwipeTopPage(-1f));
            }
        }
        else if (Input.GetKeyDown(swipeRightKey) || Input.GetKeyDown(swipeRightAlt))
        {
            if (!(keepLastPage && pilePages.Count <= 1))
            {
                StartCoroutine(SwipeTopPage(1f));
            }
        }
    }

    public void ActivatePile(bool active)
    {
        isActive = active;

        // Show or hide control guide based on activation state
        if (active)
        {
            ShowControlGuide();

            // Show first dialogue for the currently visible page
            if (pilePages != null && pilePages.Count > 0)
            {
                int currentSubtitleIndex = initialPageCount - pilePages.Count;
                ShowPageSubtitle(currentSubtitleIndex);
            }
        }
        else
        {
            HideControlGuide();

            // Clear dialogue when pile is deactivated
            if (dialogueManager != null)
            {
                dialogueManager.ClearLine();
            }
        }
    }

    public void ResetPile(List<RectTransform> newPages)
    {
        pilePages = newPages != null ? new List<RectTransform>(newPages) : new List<RectTransform>();
        NormalizeOrder();
    }

    private void NormalizeOrder()
    {
        // Ensure their sibling indexes reflect pile order: bottom at index 0, top is last
        for (int i = 0; i < pilePages.Count; i++)
        {
            if (pilePages[i] != null)
            {
                pilePages[i].SetSiblingIndex(i);
            }
        }
    }

    private void CacheOriginalScale()
    {
        if (pilePages.Count > 0 && pilePages[pilePages.Count - 1] != null)
        {
            originalScale = pilePages[pilePages.Count - 1].localScale;
        }
    }

    private IEnumerator SwipeTopPage(float direction)
    {
        if (pilePages.Count == 0) yield break;
        if (keepLastPage && pilePages.Count <= 1) yield break;

        isAnimating = true;

        RectTransform top = pilePages[pilePages.Count - 1];
        Vector2 startPos = top.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(direction * swipeDistance, -Mathf.Abs(swipeDistance) * 0.25f);

        float elapsed = 0f;
        while (elapsed < swipeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / swipeDuration);
            float e = swipeCurve.Evaluate(t);
            top.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, e);
            top.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, direction * 12f, e));
            yield return null;
        }

        // Remove page
        pilePages.RemoveAt(pilePages.Count - 1);
        if (top != null) top.gameObject.SetActive(false);

        // Save progress after each discard
        SaveProgress();

        // Show dialogue for the NEW visible page (if any remain)
        if (pilePages.Count > 0)
        {
            int currentSubtitleIndex = initialPageCount - pilePages.Count;
            ShowPageSubtitle(currentSubtitleIndex);
        }

        // Pop next page slightly for feedback
        if (pilePages.Count > 0)
        {
            RectTransform next = pilePages[pilePages.Count - 1];
            Vector2 basePos = next.anchoredPosition;
            Vector3 baseScale = originalScale;
            float popTime = 0.1f;
            float t2 = 0f;
            while (t2 < popTime)
            {
                t2 += Time.deltaTime;
                float p = t2 / popTime;
                float s = Mathf.SmoothStep(1f, nextPagePopScale, p);
                next.anchoredPosition = basePos + new Vector2(0f, nextPagePopOffset * Mathf.Sin(p * Mathf.PI));
                next.localScale = baseScale * s;
                yield return null;
            }
            // Restore scale
            next.localScale = baseScale;
            next.anchoredPosition = basePos;
        }

        isAnimating = false;

        // Final handling happens in Update when count reaches zero (fires OnPileCleared once)
    }

    public void Cancel()
    {
        if (!isActive || isAnimating) return;
        SaveProgress();
        isActive = false;

        // Clear dialogue when cancelled
        if (dialogueManager != null)
        {
            dialogueManager.ClearLine();
        }

        OnCancelled?.Invoke();
    }

    private string GetPrefsKey()
    {
        return $"NotePile_{pileId}_remaining";
    }

    public void SaveProgress()
    {
        if (!persistProgress || string.IsNullOrEmpty(pileId)) return;
        PlayerPrefs.SetInt(GetPrefsKey(), pilePages != null ? pilePages.Count : 0);
        PlayerPrefs.Save();
    }

    private void LoadAndApplyProgress()
    {
        if (!persistProgress || string.IsNullOrEmpty(pileId)) return;
        int savedRemaining = PlayerPrefs.GetInt(GetPrefsKey(), pilePages != null ? pilePages.Count : 0);
        ApplyRemaining(savedRemaining);
    }

    private void ApplyRemaining(int remaining)
    {
        if (pilePages == null) return;
        int target = Mathf.Clamp(remaining, 0, pilePages.Count);
        for (int i = pilePages.Count - 1; i >= target; i--)
        {
            RectTransform top = pilePages[i];
            if (top != null) top.gameObject.SetActive(false);
            pilePages.RemoveAt(i);
        }
    }

    public bool IsOnlyLastPageRemaining()
    {
        return pilePages != null && pilePages.Count <= 1;
    }

    public bool IsCleared()
    {
        return pilePages == null || pilePages.Count == 0;
    }

    private void StoreOriginalPages()
    {
        allOriginalPages.Clear();
        originalPositions.Clear();
        originalRotations.Clear();
        originalScales.Clear();

        if (pilePages == null) return;

        foreach (var page in pilePages)
        {
            if (page != null)
            {
                allOriginalPages.Add(page);
                originalPositions[page] = page.anchoredPosition;
                originalRotations[page] = page.localEulerAngles;
                originalScales[page] = page.localScale;
            }
        }
    }

    public void ResetPileToStart()
    {
        if (allOriginalPages.Count == 0)
        {
            Debug.LogWarning("No original pages stored. Cannot reset pile.");
            return;
        }

        // Clear current pile
        pilePages.Clear();

        // Re-enable and restore all pages to their original state
        foreach (var page in allOriginalPages)
        {
            if (page != null)
            {
                page.gameObject.SetActive(true);
                
                // Restore original transform
                if (originalPositions.ContainsKey(page))
                    page.anchoredPosition = originalPositions[page];
                if (originalRotations.ContainsKey(page))
                    page.localEulerAngles = originalRotations[page];
                if (originalScales.ContainsKey(page))
                    page.localScale = originalScales[page];
                
                pilePages.Add(page);
            }
        }

        // Normalize order
        NormalizeOrder();

        // Clear persistence
        if (persistProgress)
        {
            PlayerPrefs.SetInt(GetPrefsKey(), pilePages.Count);
            PlayerPrefs.Save();
        }

        // Reset flags
        clearedFired = false;
        isAnimating = false;

        // Reset dialogue tracking
        swipeCount = 0;
        accumulatedLetters = "";
        initialPageCount = pilePages.Count;

        Debug.Log($"Pile reset! {pilePages.Count} pages restored.");
    }

    private void UpdateControlGuideText()
    {
        if (controlGuideText == null || !showControlGuide)
        {
            return;
        }

        // Use custom text if provided, otherwise auto-generate
        if (!string.IsNullOrEmpty(customControlText))
        {
            controlGuideText.text = customControlText;
        }
        else
        {
            // Auto-generate control guide text based on key bindings
            string swipeKeys = $"{swipeLeftKey}/{swipeRightKey}";
            string altKeys = $"{swipeLeftAlt}/{swipeRightAlt}";
            string cancelKeyText = cancelKey.ToString();

            // Format the keys nicely
            swipeKeys = FormatKeyName(swipeKeys);
            altKeys = FormatKeyName(altKeys);
            cancelKeyText = FormatKeyName(cancelKeyText);

            controlGuideText.text = $"Press {swipeKeys} or {altKeys} to swipe papers | {cancelKeyText} to cancel";
        }
    }

    private string FormatKeyName(string keyName)
    {
        // Replace "LeftArrow" with "←", "RightArrow" with "→", etc.
        keyName = keyName.Replace("LeftArrow", "←");
        keyName = keyName.Replace("RightArrow", "→");
        keyName = keyName.Replace("UpArrow", "↑");
        keyName = keyName.Replace("DownArrow", "↓");
        keyName = keyName.Replace("Escape", "ESC");
        return keyName;
    }

    private void ShowControlGuide()
    {
        if (controlGuideText != null && showControlGuide)
        {
            UpdateControlGuideText();
            controlGuideText.gameObject.SetActive(true);
        }
    }

    private void HideControlGuide()
    {
        if (controlGuideText != null)
        {
            controlGuideText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Shows character thoughts and accumulated letters for the currently visible page.
    /// After specified number of subtitles shown, shows hint about combining letters.
    /// </summary>
    /// <param name="subtitleIndex">Index of the subtitle to show (0-based)</param>
    private void ShowPageSubtitle(int subtitleIndex)
    {
        if (dialogueManager == null) return;

        // Check if we have a subtitle for this index
        if (subtitleIndex >= 0 && subtitleIndex < pageSubtitles.Count)
        {
            string subtitle = pageSubtitles[subtitleIndex];

            // Try to extract letter from subtitle (looks for single uppercase letters)
            string extractedLetter = ExtractLetterFromSubtitle(subtitle);
            if (!string.IsNullOrEmpty(extractedLetter))
            {
                accumulatedLetters += extractedLetter;
            }

            // Build the full dialogue message with accumulated letters
            string fullMessage = subtitle;
            if (!string.IsNullOrEmpty(accumulatedLetters))
            {
                fullMessage += $" - Letters so far: {accumulatedLetters}";
            }

            dialogueManager.ShowLine(fullMessage);
            Debug.Log($"[NotePileSwiper] Showing subtitle {subtitleIndex}: {fullMessage}");
        }

        // After showing the specified subtitle index (converted to count), show the hint
        int subtitlesShown = subtitleIndex + 1;
        if (subtitlesShown == showHintAfterSwipeCount && !string.IsNullOrEmpty(hintMessage))
        {
            // Wait a bit before showing hint (let previous dialogue finish)
            StartCoroutine(ShowHintAfterDelay(1.5f));
        }
    }

    /// <summary>
    /// Extracts single uppercase letter from subtitle text.
    /// Looks for patterns like 'B' or "B" or just B surrounded by spaces/punctuation.
    /// </summary>
    private string ExtractLetterFromSubtitle(string subtitle)
    {
        if (string.IsNullOrEmpty(subtitle)) return "";

        // Look for single uppercase letters between quotes or apostrophes
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"['""']([A-Z])['""']");
        var match = regex.Match(subtitle);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        // Fallback: look for standalone uppercase letter surrounded by spaces/punctuation
        regex = new System.Text.RegularExpressions.Regex(@"\b([A-Z])\b");
        match = regex.Match(subtitle);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return "";
    }

    /// <summary>
    /// Shows the hint message after a delay to let previous dialogue finish.
    /// </summary>
    private IEnumerator ShowHintAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dialogueManager != null && !string.IsNullOrEmpty(hintMessage))
        {
            dialogueManager.ShowLine(hintMessage);
            Debug.Log($"[NotePileSwiper] Showing hint: {hintMessage}");
        }
    }
}



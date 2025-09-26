using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public KeyCode cancelKey = KeyCode.Escape;
    [Tooltip("Persist remaining pages across sessions using PlayerPrefs")] public bool persistProgress = true;
    [Tooltip("Unique identifier for this pile to persist progress")] public string pileId = "DefaultPile";

    [Header("State")] 
    public bool isActive = false;

    private bool isAnimating = false;
    private Vector3 originalScale = Vector3.one;
    private bool clearedFired = false;
    private bool progressApplied = false;

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
        if (!progressApplied)
        {
            LoadAndApplyProgress();
            progressApplied = true;
        }
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
}



using UnityEngine;
using TMPro;
using System.Collections;

public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance;

    [Header("UI References")]
    [Tooltip("CanvasGroup that contains the clue UI (progress + code). Starts hidden and fades in.")]
    public CanvasGroup clueCanvas;
    public TextMeshProUGUI progressText;   // e.g., "1/6 clues found"
    public TextMeshProUGUI codeText;       // e.g., "7__9__"

    [Header("Code Settings")]
    public int totalClues = 6;

    [Header("Fade Settings")]
    public float fadeSpeed = 2f;                 // how fast the canvas fades
    public float fadeOutDelay = 2f;              // delay before fade out after last clue (if you ever call the finale)
    public float completeMessageDuration = 2f;   // how long "CODE COMPLETE" stays (if used)

    // --- internal state ---
    private char[] codeSlots;   // the display string, underscores until discovered
    private bool[] found;       // track which indices are found
    private bool hasStarted = false;      // whether we’re allowed to show the UI
    private bool finaleRunning = false;   // guard for finale coroutine

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // init storage
        codeSlots = new char[totalClues];
        found = new bool[totalClues];
        for (int i = 0; i < totalClues; i++) codeSlots[i] = '_';

        // start hidden (object should be active; we control visibility by alpha)
        if (clueCanvas)
        {
            clueCanvas.alpha = 0f;
            clueCanvas.interactable = false;
            clueCanvas.blocksRaycasts = false;
        }

        UpdateUI();
    }

    /// <summary>
    /// Force the clue UI visible (e.g., when a clue is interacted).
    /// Does not change progress, just shows the panel.
    /// </summary>
    public void ShowClueUI()
    {
        hasStarted = true;

        if (clueCanvas)
        {
            if (!clueCanvas.gameObject.activeSelf)
                clueCanvas.gameObject.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(FadeCanvas(1f));
        }

        UpdateUI();
    }

    /// <summary>
    /// Register a found clue at slot 'index' with digit 'value'.
    /// This updates the display and ensures the UI is shown.
    /// </summary>
    public void RegisterClue(int index, char value)
    {
        if (index < 0 || index >= totalClues) return;

        if (!found[index])
        {
            found[index] = true;
            codeSlots[index] = value;
        }

        // make sure UI is visible on first clue
        hasStarted = true;
        if (clueCanvas)
        {
            if (!clueCanvas.gameObject.activeSelf)
                clueCanvas.gameObject.SetActive(true);

            if (clueCanvas.alpha < 0.99f)
            {
                StopAllCoroutines();
                StartCoroutine(FadeCanvas(1f));
            }
        }

        UpdateUI();

        // if you want a finale when all clues are found, uncomment this block
        /*
        if (!finaleRunning && CountCollected() == totalClues)
            StartCoroutine(ShowCodeCompleteThenFade());
        */
    }

    /// <summary>
    /// Reset clues & hide UI. Useful for restarts / testing.
    /// </summary>
    public void ResetClues()
    {
        for (int i = 0; i < totalClues; i++)
        {
            found[i] = false;
            codeSlots[i] = '_';
        }

        hasStarted = false;
        finaleRunning = false;

        if (clueCanvas)
        {
            StopAllCoroutines();
            clueCanvas.alpha = 0f;
            clueCanvas.interactable = false;
            clueCanvas.blocksRaycasts = false;
        }

        UpdateUI();
    }

    // -------- helpers --------

    IEnumerator FadeCanvas(float targetAlpha)
    {
        if (!clueCanvas) yield break;

        // enable interaction on fade-in
        if (targetAlpha > 0.99f)
        {
            clueCanvas.interactable = true;
            clueCanvas.blocksRaycasts = true;
        }

        float start = clueCanvas.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * fadeSpeed;
            clueCanvas.alpha = Mathf.Lerp(start, targetAlpha, t);
            yield return null;
        }
        clueCanvas.alpha = targetAlpha;

        // disable interaction on fade-out
        if (targetAlpha < 0.01f)
        {
            clueCanvas.interactable = false;
            clueCanvas.blocksRaycasts = false;
        }
    }

    IEnumerator ShowCodeCompleteThenFade()
    {
        finaleRunning = true;

        yield return new WaitForSecondsRealtime(fadeOutDelay);

        if (progressText)
            progressText.text = "<color=#00FF00><b>CODE COMPLETE</b></color>";

        yield return new WaitForSecondsRealtime(completeMessageDuration);

        yield return FadeCanvas(0f);

        finaleRunning = false;
    }

    int CountCollected()
    {
        int c = 0;
        for (int i = 0; i < found.Length; i++)
            if (found[i]) c++;
        return c;
    }

    void UpdateUI()
    {
        int collected = CountCollected();

        if (progressText)
            progressText.text = $"{collected}/{totalClues} clues found";

        if (codeText)
            codeText.text = new string(codeSlots);
    }
}

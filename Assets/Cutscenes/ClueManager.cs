using UnityEngine;
using TMPro;
using System.Collections;

public class ClueManager : MonoBehaviour
{
    public static ClueManager Instance;

    [Header("UI References")]
    public CanvasGroup clueCanvas;        // 👈 Drag your CanvasGroup here
    public TextMeshProUGUI progressText;  // e.g. "2/6 clues found"
    public TextMeshProUGUI codeText;      // e.g. "7__9__"

    [Header("Code Settings")]
    public int totalClues = 6;
    private char[] codeSlots;
    private bool[] found;

    [Header("Fade Settings")]
    public float fadeSpeed = 2f;          // how fast it fades in/out
    public float fadeOutDelay = 2f;       // how long before fading out
    public float completeMessageDuration = 2f; // how long "CODE COMPLETE" stays

    bool hasStarted = false;
    bool fading = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        codeSlots = new char[totalClues];
        found = new bool[totalClues];
        for (int i = 0; i < totalClues; i++) codeSlots[i] = '_';

        if (clueCanvas)
        {
            clueCanvas.alpha = 0;
            clueCanvas.interactable = false;
            clueCanvas.blocksRaycasts = false;
        }

        UpdateUI();
    }

    public void RegisterClue(int index, char value)
    {
        if (index < 0 || index >= totalClues) return;

        if (!found[index])
        {
            found[index] = true;
            codeSlots[index] = value;
        }

        int collected = CountCollected();

        // 🟢 Fade in on first clue
        if (!hasStarted)
        {
            hasStarted = true;
            if (clueCanvas)
                StartCoroutine(FadeCanvas(1f));
        }

        // 🔴 When all clues are found, trigger finale
        if (collected == totalClues && !fading)
        {
            fading = true;
            StartCoroutine(ShowCodeCompleteThenFade());
        }

        UpdateUI();
    }

    IEnumerator FadeCanvas(float targetAlpha)
    {
        if (!clueCanvas) yield break;

        clueCanvas.interactable = true;
        clueCanvas.blocksRaycasts = true;

        float startAlpha = clueCanvas.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * fadeSpeed;
            clueCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        clueCanvas.alpha = targetAlpha;
    }

    IEnumerator ShowCodeCompleteThenFade()
    {
        // wait a bit before showing the final message
        yield return new WaitForSecondsRealtime(fadeOutDelay);

        if (progressText)
            progressText.text = "<color=#00FF00><b>CODE COMPLETE</b></color>";

        yield return new WaitForSecondsRealtime(completeMessageDuration);

        // Fade out the entire UI
        yield return FadeCanvas(0f);

        if (clueCanvas)
        {
            clueCanvas.interactable = false;
            clueCanvas.blocksRaycasts = false;
        }
    }

    int CountCollected()
    {
        int c = 0;
        foreach (bool b in found) if (b) c++;
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

using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Standalone manager for stamina tutorial - no FirstPersonController modification needed!
/// Just attach this to a GameObject in your scene and assign references.
/// </summary>
public class StaminaTutorialManager : MonoBehaviour
{
    [Header("Required References")]
    [Tooltip("Drag the FirstPersonController GameObject here")]
    public FirstPersonController playerController;

    [Tooltip("Drag the sprint bar Image here (from FirstPersonController)")]
    public UnityEngine.UI.Image sprintBar;

    [Header("Tutorial UI")]
    [Tooltip("The TextMeshPro text for the notification")]
    public TextMeshProUGUI tutorialText;

    [Tooltip("Parent panel containing the tutorial text")]
    public GameObject tutorialPanel;

    [Header("Tutorial Settings")]
    [TextArea(2, 4)]
    public string message = "Sprinting drains stamina! You'll slow down when stamina runs low.";

    public float displayDuration = 5f;
    public float fadeDuration = 0.5f;
    public float verticalOffsetAboveBar = 40f;

    [Header("Styling")]
    public Color textColor = Color.white;
    public int fontSize = 24;

    [Header("Debug")]
    public bool resetOnStart = false;

    private const string SHOWN_KEY = "StaminaTutorial_Shown";
    private bool hasShown = false;
    private bool wasSprinting = false;

    void Start()
    {
        if (resetOnStart)
        {
            PlayerPrefs.DeleteKey(SHOWN_KEY);
            PlayerPrefs.Save();
        }

        // Setup UI
        if (tutorialText != null)
        {
            tutorialText.text = message;
            tutorialText.color = textColor;
            tutorialText.fontSize = fontSize;
            tutorialText.alignment = TextAlignmentOptions.Center;

            Color startColor = textColor;
            startColor.a = 0;
            tutorialText.color = startColor;
        }

        // Position above sprint bar
        PositionAboveSprintBar();

        // Hide panel
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // Check if already shown
        if (PlayerPrefs.GetInt(SHOWN_KEY, 0) == 1)
        {
            hasShown = true;
        }
    }

    void Update()
    {
        // Check if player is sprinting
        if (playerController != null && !hasShown)
        {
            bool isSprinting = playerController.IsSprinting;

            // Detect first time sprint starts
            if (isSprinting && !wasSprinting)
            {
                ShowTutorial();
            }

            wasSprinting = isSprinting;
        }
    }

    void PositionAboveSprintBar()
    {
        if (tutorialPanel == null || sprintBar == null) return;

        RectTransform panelRect = tutorialPanel.GetComponent<RectTransform>();
        RectTransform barRect = sprintBar.GetComponent<RectTransform>();

        if (panelRect != null && barRect != null)
        {
            // Match sprint bar anchoring
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);

            // Position above sprint bar
            Vector2 barPos = barRect.anchoredPosition;
            float barHeight = barRect.rect.height;

            panelRect.anchoredPosition = new Vector2(
                barPos.x,
                barPos.y + barHeight + verticalOffsetAboveBar
            );

            Debug.Log($"[StaminaTutorialManager] Positioned above sprint bar at {panelRect.anchoredPosition}");
        }
    }

    void ShowTutorial()
    {
        if (hasShown) return;

        hasShown = true;
        PlayerPrefs.SetInt(SHOWN_KEY, 1);
        PlayerPrefs.Save();

        if (tutorialPanel != null && tutorialText != null)
        {
            StartCoroutine(ShowAndFade());
        }
    }

    IEnumerator ShowAndFade()
    {
        tutorialPanel.SetActive(true);

        // Fade in
        yield return Fade(0f, 1f, fadeDuration);

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return Fade(1f, 0f, fadeDuration);

        tutorialPanel.SetActive(false);
    }

    IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = tutorialText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            tutorialText.color = color;

            yield return null;
        }

        color.a = endAlpha;
        tutorialText.color = color;
    }

    [ContextMenu("Reset Tutorial")]
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(SHOWN_KEY);
        PlayerPrefs.Save();
        hasShown = false;
        Debug.Log("[StaminaTutorialManager] Reset!");
    }

    [ContextMenu("Force Show")]
    public void ForceShow()
    {
        hasShown = false;
        ShowTutorial();
    }
}

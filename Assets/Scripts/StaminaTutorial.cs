using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Displays a one-time tutorial notification about the stamina system
/// when the player first presses the sprint key.
/// </summary>
public class StaminaTutorial : MonoBehaviour
{
    [Header("Tutorial Settings")]
    [Tooltip("The message to display to the player")]
    [TextArea(2, 4)]
    public string tutorialMessage = "Sprinting drains stamina! You'll slow down when stamina runs low.";

    [Tooltip("How long to display the message (in seconds)")]
    public float displayDuration = 5f;

    [Tooltip("Fade in/out animation time (in seconds)")]
    public float fadeDuration = 0.5f;

    [Header("UI References")]
    [Tooltip("TextMeshPro text component for displaying the notification")]
    public TextMeshProUGUI notificationText;

    [Tooltip("Parent GameObject containing the notification UI")]
    public GameObject notificationPanel;

    [Header("Positioning")]
    [Tooltip("Automatically position above the sprint/stamina bar")]
    public bool positionAboveStaminaBar = true;

    [Tooltip("Reference to the sprint bar to position above (usually from FirstPersonController)")]
    public RectTransform staminaBarTransform;

    [Tooltip("Vertical offset above the stamina bar (in pixels)")]
    public float verticalOffset = 40f;

    [Tooltip("Manual screen position (only used if positionAboveStaminaBar is false)")]
    public Vector2 manualScreenPosition = new Vector2(0.5f, 0.7f);

    [Header("Styling")]
    public Color textColor = Color.white;
    public int fontSize = 24;
    public FontStyles fontStyle = FontStyles.Normal;

    [Header("Debug")]
    [Tooltip("Reset tutorial on every play (for testing)")]
    public bool resetOnStart = false;

    private const string TUTORIAL_SHOWN_KEY = "StaminaTutorial_Shown";
    private bool hasShownThisSession = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        // Reset for testing if enabled
        if (resetOnStart)
        {
            PlayerPrefs.DeleteKey(TUTORIAL_SHOWN_KEY);
            PlayerPrefs.Save();
            Debug.Log("[StaminaTutorial] Tutorial reset for testing");
        }

        // Setup the notification UI
        SetupNotificationUI();

        // Position the notification panel
        PositionNotificationPanel();

        // Hide initially
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Sets up the notification text and panel
    /// </summary>
    private void SetupNotificationUI()
    {
        if (notificationText != null)
        {
            notificationText.text = tutorialMessage;
            notificationText.color = textColor;
            notificationText.fontSize = fontSize;
            notificationText.fontStyle = fontStyle;
            notificationText.alignment = TextAlignmentOptions.Center;

            // Make sure it starts invisible
            Color startColor = textColor;
            startColor.a = 0;
            notificationText.color = startColor;
        }
    }

    /// <summary>
    /// Positions the notification panel above the stamina bar
    /// </summary>
    private void PositionNotificationPanel()
    {
        if (notificationPanel == null)
        {
            Debug.LogWarning("[StaminaTutorial] Cannot position panel - notificationPanel is null");
            return;
        }

        RectTransform panelRect = notificationPanel.GetComponent<RectTransform>();
        if (panelRect == null)
        {
            Debug.LogWarning("[StaminaTutorial] Cannot position panel - no RectTransform found");
            return;
        }

        if (positionAboveStaminaBar && staminaBarTransform != null)
        {
            // Position above the stamina bar
            Debug.Log("[StaminaTutorial] Positioning notification above stamina bar");

            // Set anchors to bottom-center (same as stamina bar typically)
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);

            // Get the stamina bar's position
            Vector2 staminaBarPos = staminaBarTransform.anchoredPosition;
            float staminaBarHeight = staminaBarTransform.rect.height;

            // Position above the stamina bar with offset
            panelRect.anchoredPosition = new Vector2(
                staminaBarPos.x, // Same X position as stamina bar
                staminaBarPos.y + staminaBarHeight + verticalOffset // Above with offset
            );

            Debug.Log($"[StaminaTutorial] Positioned at: {panelRect.anchoredPosition}, Stamina bar at: {staminaBarPos}");
        }
        else
        {
            // Use manual positioning
            Debug.Log("[StaminaTutorial] Using manual positioning");

            panelRect.anchorMin = manualScreenPosition;
            panelRect.anchorMax = manualScreenPosition;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Call this method to show the tutorial (usually from FirstPersonController)
    /// </summary>
    public void ShowTutorial()
    {
        // Check if already shown
        if (hasShownThisSession)
        {
            return;
        }

        // Check PlayerPrefs to see if shown before
        if (PlayerPrefs.GetInt(TUTORIAL_SHOWN_KEY, 0) == 1)
        {
            hasShownThisSession = true;
            return;
        }

        // Show the tutorial
        if (notificationPanel != null && notificationText != null)
        {
            Debug.Log("[StaminaTutorial] Showing stamina tutorial notification");

            // Mark as shown
            hasShownThisSession = true;
            PlayerPrefs.SetInt(TUTORIAL_SHOWN_KEY, 1);
            PlayerPrefs.Save();

            // Stop any existing fade
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            // Start fade in/out sequence
            fadeCoroutine = StartCoroutine(ShowAndFadeNotification());
        }
        else
        {
            Debug.LogWarning("[StaminaTutorial] Cannot show tutorial - UI references not set!");
        }
    }

    /// <summary>
    /// Coroutine to fade in, wait, then fade out the notification
    /// </summary>
    private IEnumerator ShowAndFadeNotification()
    {
        // Show panel
        notificationPanel.SetActive(true);

        // Fade in
        yield return FadeText(0f, 1f, fadeDuration);

        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return FadeText(1f, 0f, fadeDuration);

        // Hide panel
        notificationPanel.SetActive(false);

        fadeCoroutine = null;
    }

    /// <summary>
    /// Fades the notification text from one alpha value to another
    /// </summary>
    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = notificationText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            notificationText.color = color;

            yield return null;
        }

        // Ensure final alpha is set
        color.a = endAlpha;
        notificationText.color = color;
    }

    /// <summary>
    /// Sets the stamina bar reference for auto-positioning
    /// </summary>
    /// <param name="staminaBar">The RectTransform of the sprint/stamina bar</param>
    public void SetStaminaBarReference(RectTransform staminaBar)
    {
        staminaBarTransform = staminaBar;
        Debug.Log($"[StaminaTutorial] Stamina bar reference set: {staminaBar?.name}");

        // Re-position if panel exists
        if (notificationPanel != null)
        {
            PositionNotificationPanel();
        }
    }

    /// <summary>
    /// Public method to manually reset the tutorial (for testing or new game)
    /// </summary>
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_SHOWN_KEY);
        PlayerPrefs.Save();
        hasShownThisSession = false;
        Debug.Log("[StaminaTutorial] Tutorial has been reset");
    }

    /// <summary>
    /// Check if tutorial has been shown
    /// </summary>
    public bool HasBeenShown()
    {
        return PlayerPrefs.GetInt(TUTORIAL_SHOWN_KEY, 0) == 1;
    }

    /// <summary>
    /// For editor testing - force show the tutorial
    /// </summary>
    [ContextMenu("Force Show Tutorial")]
    public void ForceShowTutorial()
    {
        hasShownThisSession = false;
        PlayerPrefs.SetInt(TUTORIAL_SHOWN_KEY, 0);
        ShowTutorial();
    }
}

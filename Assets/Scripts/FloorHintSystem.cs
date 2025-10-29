using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the hint system for each floor. Shows a horror-themed hint button
/// after the player has been on the floor for more than 5 minutes.
/// </summary>
public class FloorHintSystem : MonoBehaviour
{
    [Header("Hint Settings")]
    [Tooltip("The hint text to display for this floor")]
    [TextArea(3, 10)]
    public string hintText = "Hint text goes here...";

    [Tooltip("Time in seconds before hint button appears (default: 300 = 5 minutes)")]
    public float timeBeforeHintAppears = 300f;

    [Header("UI References (Auto-assigned by Editor script)")]
    public GameObject hintButton;
    public GameObject hintPanel;
    public Text hintTextDisplay;

    [Header("Debug")]
    [Tooltip("Enable to show hint button immediately for testing")]
    public bool debugShowImmediately = false;

    private float timeOnFloor = 0f;
    private bool hintButtonShown = false;
    private bool isPanelOpen = false;

    void Start()
    {
        // Ensure hint button is hidden initially
        if (hintButton != null)
        {
            hintButton.SetActive(debugShowImmediately);
            hintButtonShown = debugShowImmediately;
        }

        // Ensure hint panel is hidden initially
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
            isPanelOpen = false;
        }

        // Set hint text
        if (hintTextDisplay != null)
        {
            hintTextDisplay.text = hintText;
        }
    }

    void Update()
    {
        // Don't count time during cutscenes or when game is paused
        if (Time.timeScale == 0f)
        {
            return;
        }

        // Track time on floor
        if (!hintButtonShown)
        {
            timeOnFloor += Time.deltaTime;

            // Show hint button after enough time has passed
            if (timeOnFloor >= timeBeforeHintAppears)
            {
                ShowHintButton();
            }
        }
    }

    /// <summary>
    /// Shows the hint button (called automatically after timer expires)
    /// </summary>
    void ShowHintButton()
    {
        if (hintButton != null && !hintButtonShown)
        {
            hintButton.SetActive(true);
            hintButtonShown = true;
            Debug.Log($"[FloorHintSystem] Hint button shown after {timeOnFloor} seconds");
        }
    }

    /// <summary>
    /// Toggles the hint panel visibility (called by button click)
    /// </summary>
    public void ToggleHintPanel()
    {
        if (hintPanel != null)
        {
            isPanelOpen = !isPanelOpen;
            hintPanel.SetActive(isPanelOpen);

            Debug.Log($"[FloorHintSystem] Hint panel {(isPanelOpen ? "opened" : "closed")}");
        }
    }

    /// <summary>
    /// Manually show the hint button (for testing or special events)
    /// </summary>
    public void ForceShowHintButton()
    {
        ShowHintButton();
    }

    /// <summary>
    /// Get current time spent on floor
    /// </summary>
    public float GetTimeOnFloor()
    {
        return timeOnFloor;
    }

    /// <summary>
    /// Check if hint button is currently shown
    /// </summary>
    public bool IsHintButtonShown()
    {
        return hintButtonShown;
    }
}

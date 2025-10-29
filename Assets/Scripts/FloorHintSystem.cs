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
    public Text instructionText;

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

        // Ensure instruction text is hidden initially
        if (instructionText != null && instructionText.gameObject != null)
        {
            instructionText.gameObject.SetActive(debugShowImmediately);
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
        // Handle pause state - hide hint UI when paused
        if (Time.timeScale == 0f)
        {
            // Game is paused - hide hint UI if it's shown
            if (hintButtonShown)
            {
                if (hintButton != null && hintButton.activeSelf)
                {
                    hintButton.SetActive(false);
                }

                if (instructionText != null && instructionText.gameObject.activeSelf)
                {
                    instructionText.gameObject.SetActive(false);
                }

                // Close hint panel if it's open
                if (isPanelOpen && hintPanel != null)
                {
                    hintPanel.SetActive(false);
                }
            }

            return;
        }

        // Game is not paused - restore hint UI if it should be shown
        if (hintButtonShown)
        {
            if (hintButton != null && !hintButton.activeSelf)
            {
                hintButton.SetActive(true);
            }

            if (instructionText != null && !instructionText.gameObject.activeSelf)
            {
                instructionText.gameObject.SetActive(true);
            }

            // Restore hint panel state (it stays closed unless player opens it)
            if (isPanelOpen && hintPanel != null && !hintPanel.activeSelf)
            {
                hintPanel.SetActive(true);
            }
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

        // Check for Z key press to toggle hint panel (only when button is shown and not paused)
        if (hintButtonShown && Input.GetKeyDown(KeyCode.Z))
        {
            ToggleHintPanel();
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

            // Also show instruction text
            if (instructionText != null)
            {
                instructionText.gameObject.SetActive(true);
            }

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

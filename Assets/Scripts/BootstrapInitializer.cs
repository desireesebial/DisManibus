using UnityEngine;

/// <summary>
/// Initializes the Bootstrap scene with a PhotoSensitive trigger warning,
/// then loads the MainMenu scene after user acknowledgment.
/// This script should be attached to a GameObject in the Bootstrap scene.
/// </summary>
public class BootstrapInitializer : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    [SerializeField]
    private string loadingMessage = "Loading Main Menu...";

    [Header("Trigger Warning UI")]
    [SerializeField] private GameObject triggerWarningCanvas;
    [SerializeField] private TMPro.TextMeshProUGUI titleText;
    [SerializeField] private TMPro.TextMeshProUGUI bodyText;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;

    [Header("Warning Content")]
    [SerializeField]
    private string warningTitle = "⚠️ PHOTOSENSITIVE SEIZURE WARNING ⚠️";

    [SerializeField] [TextArea(3, 10)]
    private string warningBody = "This game contains flashing lights, rapid scene transitions, and visual effects that may trigger seizures in individuals with photosensitive epilepsy.\n\nPlayer discretion is strongly advised.";

    [SerializeField]
    private string promptMessage = "Press any key to continue...";

    // State tracking
    private bool warningShown = false;
    private bool waitingForInput = false;

    private void Start()
    {
        // Show trigger warning first
        if (triggerWarningCanvas != null)
        {
            ShowTriggerWarning();
        }
        else
        {
            Debug.LogWarning("BootstrapInitializer: No trigger warning canvas assigned. Proceeding to MainMenu immediately.");
            LoadMainMenu();
        }
    }

    private void Update()
    {
        // Wait for any key press to continue
        if (waitingForInput && Input.anyKeyDown)
        {
            HideTriggerWarning();
            LoadMainMenu();
        }
    }

    private void ShowTriggerWarning()
    {
        Debug.Log("BootstrapInitializer: Showing trigger warning...");

        triggerWarningCanvas.SetActive(true);

        // Set title text if assigned
        if (titleText != null)
            titleText.text = warningTitle;

        // Set body text if assigned
        if (bodyText != null)
            bodyText.text = warningBody;

        // Set prompt text if assigned
        if (promptText != null)
            promptText.text = promptMessage;

        warningShown = true;
        waitingForInput = true;
    }

    private void HideTriggerWarning()
    {
        Debug.Log("BootstrapInitializer: Hiding trigger warning...");

        if (triggerWarningCanvas != null)
            triggerWarningCanvas.SetActive(false);

        waitingForInput = false;
    }

    private void LoadMainMenu()
    {
        // Wait for SceneTransitionManager to be ready, then load MainMenu
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(mainMenuSceneName, loadingMessage);
        }
        else
        {
            Debug.LogError("BootstrapInitializer: SceneTransitionManager instance not found!");
        }
    }
}

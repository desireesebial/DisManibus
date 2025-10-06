using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Wiring")]
    [Tooltip("The root GameObject of your pause menu (the Panel with all buttons).")]
    public GameObject pauseRoot;          // Pause Panel
    [Tooltip("CanvasGroup on that same panel (optional but recommended).")]
    public CanvasGroup pauseGroup;        // CanvasGroup on pauseRoot (optional)
    [Tooltip("Settings panel GameObject (your Settings Menu).")]
    public GameObject settingsPanel;
    [Tooltip("Optional: Controls panel GameObject.")]
    public GameObject controlsPanel;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    public bool IsPaused { get; private set; }

    void Start()
    {
        // Ensure everything is hidden at boot
        IsPaused = false;

        if (pauseGroup)
        {
            pauseGroup.alpha = 0f;
            pauseGroup.blocksRaycasts = false;
            pauseGroup.interactable = false;
        }
        if (pauseRoot) pauseRoot.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        IsPaused = true;

        if (pauseRoot) pauseRoot.SetActive(true);

        if (pauseGroup)
        {
            pauseGroup.alpha = 1f;
            pauseGroup.blocksRaycasts = true;
            pauseGroup.interactable = true;
        }

        // Hide sub-panels while opening pause
        if (settingsPanel) settingsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume()
    {
        IsPaused = false;

        if (pauseGroup)
        {
            pauseGroup.alpha = 0f;
            pauseGroup.blocksRaycasts = false;
            pauseGroup.interactable = false;
        }

        if (pauseRoot) pauseRoot.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // === Buttons ===
    public void OpenSettings()
    {
        // Show only Settings while paused
        if (pauseRoot) pauseRoot.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void OpenControls()
    {
        if (pauseRoot) pauseRoot.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(true);
    }

    public void BackFromSubPanel()
    {
        // Return to main pause panel
        if (settingsPanel) settingsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (pauseRoot) pauseRoot.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogWarning("MainMenu scene name not set in PauseMenu script.");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

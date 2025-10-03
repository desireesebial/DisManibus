using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Wiring")]
    [Tooltip("The root GameObject of your pause menu (the Panel with all buttons).")]
    public GameObject pauseRoot;          // The Pause Panel
    [Tooltip("CanvasGroup on that same panel (optional but recommended).")]
    public CanvasGroup pauseGroup;
    public GameObject settingsPanel;
    public GameObject controlsPanel;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    public bool IsPaused { get; private set; }

    void Start()
    {
        IsPaused = false;
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

        // SHOW the menu
        if (pauseRoot != null)
            pauseRoot.SetActive(true);

        if (pauseGroup != null)
        {
            pauseGroup.alpha = 1f;
            pauseGroup.blocksRaycasts = true;
            pauseGroup.interactable = true;
        }

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("[Pause] Game paused + menu shown");
    }

    public void Resume()
    {
        IsPaused = false;

        // HIDE the menu
        if (pauseGroup != null)
        {
            pauseGroup.alpha = 0f;
            pauseGroup.blocksRaycasts = false;
            pauseGroup.interactable = false;
        }

        if (pauseRoot != null)
            pauseRoot.SetActive(false);

        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (settingsPanel) settingsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);

        Debug.Log("[Pause] Game resumed + menu hidden");
    }

    public void OpenSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(true);
        if (controlsPanel) controlsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        if (controlsPanel) controlsPanel.SetActive(true);
        if (settingsPanel) settingsPanel.SetActive(false);
    }

    public void BackFromSubPanel()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
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

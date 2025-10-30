using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using World.UI;

/// <summary>
/// Main menu controller that handles all main menu button interactions.
/// Attach this to a GameObject in your main menu scene and wire up buttons via Unity Inspector.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    #region Scene Settings
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Fourth Floor";
    [SerializeField] private bool useTransitionManager = true;
    [SerializeField] private string newGameLoadingMessage = "Starting New Game";
    [SerializeField] private string loadGameLoadingMessage = "Loading Game";
    #endregion

    #region UI Button References
    [Header("UI Button References (Optional)")]
    [SerializeField] private Button loadGameButton;
    #endregion

    #region UI Panel References
    [Header("UI Panel References (Optional)")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject confirmExitPanel;
    #endregion

    #region Exit Settings
    [Header("Exit Settings")]
    [SerializeField] private bool requireExitConfirmation = false;
    #endregion

    #region Private Fields
    private bool isTransitioning = false;
    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Initialize button states on start.
    /// </summary>
    private void Start()
    {
        UpdateLoadButtonState();
    }

    #endregion

    #region Main Menu Actions

    /// <summary>
    /// Starts a new game, clearing any existing save data.
    /// Call this method from your New Game button's onClick event.
    /// </summary>
    public void NewGame()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[MainMenu] Already transitioning to a scene.");
            return;
        }

        Debug.Log("[MainMenu] Starting new game...");
        isTransitioning = true;

        // Clear existing save data when starting a new game
        SaveLoad.SaveSystem.DeleteSave();

        // Update load button state since save was deleted
        UpdateLoadButtonState();

        // Load game scene with transition
        LoadGameScene(gameSceneName, newGameLoadingMessage);
    }

    /// <summary>
    /// Loads a saved game.
    /// Call this method from your Load Game button's onClick event.
    /// </summary>
    public void LoadGame()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[MainMenu] Already transitioning to a scene.");
            return;
        }

        // Try to load the save data
        if (!SaveLoad.SaveSystem.TryLoadPlayer(out SaveLoad.SaveData data))
        {
            Debug.LogWarning("[MainMenu] Failed to load save data.");
            // TODO: Show a "No Save File" message to the player
            return;
        }

        Debug.Log($"[MainMenu] Loading saved game from scene: {data.sceneName}");
        isTransitioning = true;

        // Set pending load data so it gets applied when scene loads
        SaveLoad.SaveManager.SetPendingLoadData(data);

        // Load the scene where the player saved (NOT the hardcoded scene)
        LoadGameScene(data.sceneName, loadGameLoadingMessage);
    }

    /// <summary>
    /// Opens the settings panel.
    /// Call this method from your Settings button's onClick event.
    /// </summary>
    public void OpenSettings()
    {
        Debug.Log("[MainMenu] Opening settings...");

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[MainMenu] Settings panel not assigned!");
        }
    }

    /// <summary>
    /// Closes the settings panel.
    /// Call this method from your Settings panel's close/back button.
    /// </summary>
    public void CloseSettings()
    {
        Debug.Log("[MainMenu] Closing settings...");

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Opens the credits panel.
    /// Call this method from your Credits button's onClick event.
    /// </summary>
    public void OpenCredits()
    {
        Debug.Log("[MainMenu] Opening credits...");

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[MainMenu] Credits panel not assigned!");
        }
    }

    /// <summary>
    /// Closes the credits panel.
    /// Call this method from your Credits panel's close/back button.
    /// </summary>
    public void CloseCredits()
    {
        Debug.Log("[MainMenu] Closing credits...");

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Opens the controls panel.
    /// Call this method from your Controls button's onClick event.
    /// </summary>
    public void OpenControls()
    {
        Debug.Log("[MainMenu] Opening controls...");

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[MainMenu] Controls panel not assigned!");
        }
    }

    /// <summary>
    /// Closes the controls panel.
    /// Call this method from your Controls panel's close/back button.
    /// </summary>
    public void CloseControls()
    {
        Debug.Log("[MainMenu] Closing controls...");

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Exits the game application.
    /// Call this method from your Exit/Quit button's onClick event.
    /// </summary>
    public void ExitGame()
    {
        if (requireExitConfirmation && confirmExitPanel != null)
        {
            Debug.Log("[MainMenu] Showing exit confirmation...");
            confirmExitPanel.SetActive(true);
        }
        else
        {
            ConfirmExitGame();
        }
    }

    /// <summary>
    /// Confirms and executes the exit game action.
    /// Call this from your confirmation panel's "Yes" button if using exit confirmation.
    /// </summary>
    public void ConfirmExitGame()
    {
        Debug.Log("[MainMenu] Exit Game triggered.");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Cancels the exit game action.
    /// Call this from your confirmation panel's "No" button if using exit confirmation.
    /// </summary>
    public void CancelExitGame()
    {
        Debug.Log("[MainMenu] Exit cancelled.");

        if (confirmExitPanel != null)
        {
            confirmExitPanel.SetActive(false);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Updates the Load Game button state based on save file existence.
    /// Disables button if no save file exists, enables if save file is present.
    /// </summary>
    private void UpdateLoadButtonState()
    {
        if (loadGameButton != null)
        {
            bool hasSave = SaveLoad.SaveSystem.HasSaveData();
            loadGameButton.interactable = hasSave;

            if (!hasSave)
            {
                Debug.Log("[MainMenu] Load button disabled - no save file found.");
            }
            else
            {
                Debug.Log("[MainMenu] Load button enabled - save file found.");
            }
        }
    }

    /// <summary>
    /// Loads the game scene using the transition manager if available.
    /// </summary>
    private void LoadGameScene(string sceneName, string loadingMessage)
    {
        if (useTransitionManager && SceneTransitionManager.Instance != null)
        {
            Debug.Log($"[MainMenu] Loading scene '{sceneName}' with SceneTransitionManager");
            SceneTransitionManager.Instance.LoadScene(sceneName, loadingMessage);
        }
        else
        {
            Debug.Log($"[MainMenu] Loading scene '{sceneName}' directly");
            StartCoroutine(LoadSceneWithLoadingScreen(sceneName, loadingMessage));
        }
    }

    /// <summary>
    /// Fallback scene loading with loading screen integration.
    /// </summary>
    private System.Collections.IEnumerator LoadSceneWithLoadingScreen(string sceneName, string loadingMessage)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName);

        if (LoadingScreenController.Instance != null)
        {
            yield return LoadingScreenController.Instance.TrackAsyncOperation(operation, loadingMessage);
        }
        else
        {
            yield return operation;
        }
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// Sets the target game scene name.
    /// </summary>
    public void SetGameSceneName(string sceneName)
    {
        gameSceneName = sceneName;
        Debug.Log($"[MainMenu] Game scene set to: {sceneName}");
    }

    /// <summary>
    /// Toggles whether to use the scene transition manager.
    /// </summary>
    public void SetUseTransitionManager(bool use)
    {
        useTransitionManager = use;
        Debug.Log($"[MainMenu] Use transition manager: {use}");
    }

    /// <summary>
    /// Checks if a save file exists.
    /// </summary>
    public bool DoesSaveExist()
    {
        return SaveLoad.SaveSystem.HasSaveData();
    }

    #endregion
}

using UnityEngine;
using UnityEngine.SceneManagement;
using World.UI;

/// <summary>
/// Controls the demo level select panel in the main menu.
/// Allows quick navigation to any floor scene for presentation/demo purposes.
/// Does NOT interact with the save system - loads scenes with a clean slate.
/// </summary>
public class LevelSelectController : MonoBehaviour
{
    #region Scene Name Constants
    private const string FOURTH_FLOOR = "4th Floor (better version)";
    private const string THIRD_FLOOR = "3rd floor (better version)";
    private const string SECOND_FLOOR = "2nd Floor (Better Version)";
    private const string FIRST_FLOOR_GOOD = "1st Floor (GOOD ENDING)";
    private const string FIRST_FLOOR_BAD = "1st Floor (BAD ENDING)";
    #endregion

    #region Settings
    [Header("Demo Loading Settings")]
    [SerializeField] private bool useTransitionManager = true;
    [SerializeField] private string demoLoadingMessage = "Loading Demo...";
    #endregion

    #region Private Fields
    private bool isTransitioning = false;
    #endregion

    #region Level Loading Methods

    /// <summary>
    /// Loads the 4th Floor scene in demo mode.
    /// Call this from the 4th Floor button's onClick event.
    /// </summary>
    public void Load4thFloor()
    {
        LoadDemoLevel(FOURTH_FLOOR);
    }

    /// <summary>
    /// Loads the 3rd Floor scene in demo mode.
    /// Call this from the 3rd Floor button's onClick event.
    /// </summary>
    public void Load3rdFloor()
    {
        LoadDemoLevel(THIRD_FLOOR);
    }

    /// <summary>
    /// Loads the 2nd Floor scene in demo mode.
    /// Call this from the 2nd Floor button's onClick event.
    /// </summary>
    public void Load2ndFloor()
    {
        LoadDemoLevel(SECOND_FLOOR);
    }

    /// <summary>
    /// Loads the 1st Floor (Good Ending) scene in demo mode.
    /// Call this from the 1st Floor Good Ending button's onClick event.
    /// </summary>
    public void Load1stFloorGood()
    {
        LoadDemoLevel(FIRST_FLOOR_GOOD);
    }

    /// <summary>
    /// Loads the 1st Floor (Bad Ending) scene in demo mode.
    /// Call this from the 1st Floor Bad Ending button's onClick event.
    /// </summary>
    public void Load1stFloorBad()
    {
        LoadDemoLevel(FIRST_FLOOR_BAD);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Loads a demo level WITHOUT interacting with the save system.
    /// This ensures a clean slate for presentations.
    /// </summary>
    private void LoadDemoLevel(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[LevelSelect] Already transitioning to a scene.");
            return;
        }

        Debug.Log($"[LevelSelect] Loading demo level: {sceneName}");
        isTransitioning = true;

        // IMPORTANT: Do NOT delete saves or set pending load data
        // We want to bypass the save system entirely for demo mode

        // Load the scene
        LoadScene(sceneName, demoLoadingMessage);
    }

    /// <summary>
    /// Loads a scene using the transition manager if available.
    /// </summary>
    private void LoadScene(string sceneName, string loadingMessage)
    {
        if (useTransitionManager && SceneTransitionManager.Instance != null)
        {
            Debug.Log($"[LevelSelect] Loading scene '{sceneName}' with SceneTransitionManager");
            SceneTransitionManager.Instance.LoadScene(sceneName, loadingMessage);
        }
        else
        {
            Debug.Log($"[LevelSelect] Loading scene '{sceneName}' directly");
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
}

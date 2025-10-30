using UnityEngine;

/// <summary>
/// Initializes the Bootstrap scene and automatically loads the MainMenu scene.
/// This script should be attached to a GameObject in the Bootstrap scene.
/// </summary>
public class BootstrapInitializer : MonoBehaviour
{
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    [SerializeField]
    private string loadingMessage = "Loading Main Menu...";

    private void Start()
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

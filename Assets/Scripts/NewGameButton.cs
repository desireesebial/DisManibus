using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NewGameButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Fourth Floor";
    [SerializeField] private bool useTransitionManager = true;
    
    [Header("UI References")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Text buttonText;
    
    private bool isTransitioning = false;
    
    void Start()
    {
        // Get references if not set in inspector
        if (newGameButton == null)
            newGameButton = GetComponent<Button>();
            
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
            
        if (buttonText == null)
            buttonText = GetComponentInChildren<Text>();
        
        // Add click listener
        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(NewGame);
        }
    }
    
    public void NewGame()
    {
        if (isTransitioning) return;
        
        Debug.Log("Starting new game...");
        isTransitioning = true;
        
        // Disable button to prevent multiple clicks
        if (newGameButton != null)
            newGameButton.interactable = false;
        
        // Use transition manager if available, otherwise direct load
        if (useTransitionManager && SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameSceneName);
        }

        // Clear existing save data when starting a new game
        SaveLoad.SaveSystem.DeleteSave();
    }
    
    private void LoadGameScene()
    {
        try
        {
            // Check if scene exists in build settings using modern API
            if (SceneManager.GetSceneByName(gameSceneName).IsValid())
            {
                SceneManager.LoadScene(gameSceneName);
            }
            else
            {
                // Try to load by scene index (fallback)
                Debug.LogWarning($"Scene '{gameSceneName}' not found in build settings. Trying to load by index...");
                
                // Try to find the scene in build settings
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    
                    if (sceneName.Contains("Fourth") || sceneName.Contains("Floor"))
                    {
                        Debug.Log($"Found game scene: {sceneName} at index {i}");
                        SceneManager.LoadScene(i);
                        return;
                    }
                }
                
                // If no suitable scene found, load the first available scene
                if (SceneManager.sceneCountInBuildSettings > 0)
                {
                    Debug.LogWarning("No suitable game scene found. Loading first available scene.");
                    SceneManager.LoadScene(0);
                }
                else
                {
                    Debug.LogError("No scenes available in build settings!");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading scene: {e.Message}");
            // Re-enable button on error
            if (newGameButton != null)
                newGameButton.interactable = true;
            isTransitioning = false;
        }
    }
    
    // Public method to check if scene exists
    public bool IsGameSceneAvailable()
    {
        return SceneManager.GetSceneByName(gameSceneName).IsValid();
    }
    
    // Public method to get available scenes
    public string[] GetAvailableScenes()
    {
        string[] scenes = new string[SceneManager.sceneCountInBuildSettings];
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            scenes[i] = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
        return scenes;
    }
    
    // Public method to set the target scene name
    public void SetGameSceneName(string sceneName)
    {
        gameSceneName = sceneName;
    }
    
    // Public method to toggle transition manager usage
    public void SetUseTransitionManager(bool use)
    {
        useTransitionManager = use;
    }
}

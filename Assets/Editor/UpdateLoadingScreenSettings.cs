using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using World.UI;

/// <summary>
/// Editor utility to update LoadingScreenController settings in Bootstrap scene
/// </summary>
public class UpdateLoadingScreenSettings : EditorWindow
{
    [MenuItem("Tools/Update Loading Screen Settings")]
    public static void UpdateSettings()
    {
        // Save current scene
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        
        // Open Bootstrap scene
        var bootstrapScene = EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Single);
        
        // Find LoadingScreenController
        LoadingScreenController[] controllers = FindObjectsByType<LoadingScreenController>(FindObjectsSortMode.None);
        
        if (controllers.Length == 0)
        {
            Debug.LogError("No LoadingScreenController found in Bootstrap scene!");
            return;
        }
        
        foreach (var controller in controllers)
        {
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty minDisplayTime = so.FindProperty("minimumDisplayTime");
            
            if (minDisplayTime != null)
            {
                float oldValue = minDisplayTime.floatValue;
                minDisplayTime.floatValue = 2.0f;
                so.ApplyModifiedProperties();
                
                Debug.Log($"Updated LoadingScreenController on '{controller.gameObject.name}': minimumDisplayTime {oldValue}s → 2.0s");
                EditorUtility.SetDirty(controller);
            }
        }
        
        // Save the scene
        EditorSceneManager.SaveScene(bootstrapScene);
        Debug.Log("✅ Bootstrap scene saved with updated LoadingScreenController settings!");
    }
    
    [MenuItem("Tools/Update SceneTransitionManager Settings")]
    public static void UpdateTransitionManagerSettings()
    {
        // Save current scene
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        
        // Open Bootstrap scene
        var bootstrapScene = EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity", OpenSceneMode.Single);
        
        // Find SceneTransitionManager
        SceneTransitionManager[] managers = FindObjectsByType<SceneTransitionManager>(FindObjectsSortMode.None);
        
        if (managers.Length == 0)
        {
            Debug.LogError("No SceneTransitionManager found in Bootstrap scene!");
            return;
        }
        
        foreach (var manager in managers)
        {
            SerializedObject so = new SerializedObject(manager);
            SerializedProperty minLoadingTime = so.FindProperty("minimumLoadingTime");
            
            if (minLoadingTime != null)
            {
                float oldValue = minLoadingTime.floatValue;
                minLoadingTime.floatValue = 2.0f;
                so.ApplyModifiedProperties();
                
                Debug.Log($"Updated SceneTransitionManager on '{manager.gameObject.name}': minimumLoadingTime {oldValue}s → 2.0s");
                EditorUtility.SetDirty(manager);
            }
        }
        
        // Save the scene
        EditorSceneManager.SaveScene(bootstrapScene);
        Debug.Log("✅ Bootstrap scene saved with updated SceneTransitionManager settings!");
    }
}


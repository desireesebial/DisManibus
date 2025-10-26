using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class ApplyNightSkyTo1stFloor : EditorWindow
{
    [MenuItem("Tools/Apply Night Sky to 1st Floor")]
    public static void ApplyNightSky()
    {
        // Load the Night Moon Burst skybox material
        string skyboxPath = "Assets/Assets/SKy/Night MoonBurst/Night Moon Burst.mat";
        Material nightSkyMaterial = AssetDatabase.LoadAssetAtPath<Material>(skyboxPath);

        if (nightSkyMaterial == null)
        {
            Debug.LogError($"Could not find skybox material at: {skyboxPath}");
            EditorUtility.DisplayDialog("Error", "Could not find Night Moon Burst skybox material!", "OK");
            return;
        }

        // Check if 1st Floor scene is currently open
        Scene currentScene = SceneManager.GetActiveScene();
        bool is1stFloorScene = currentScene.name == "1st Floor";

        if (!is1stFloorScene)
        {
            // Try to open the 1st Floor scene
            string scenePath = "Assets/Scenes/1st Floor.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            if (!scene.IsValid())
            {
                Debug.LogError($"Could not open scene at: {scenePath}");
                EditorUtility.DisplayDialog("Error", "Could not open 1st Floor scene!", "OK");
                return;
            }

            currentScene = scene;
        }

        // Apply the skybox to the scene's Lighting Settings
        RenderSettings.skybox = nightSkyMaterial;

        // Optional: Adjust ambient lighting for night atmosphere
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 0.5f; // Darker ambient for night

        // Optional: Set fog color to match night sky
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.15f); // Dark blue fog
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.01f;

        // Mark scene as dirty so changes are saved
        EditorSceneManager.MarkSceneDirty(currentScene);

        Debug.Log($"<color=green>Applied Night Moon Burst skybox to {currentScene.name}!</color>");
        Debug.Log("Settings applied:");
        Debug.Log($"  - Skybox: {nightSkyMaterial.name}");
        Debug.Log($"  - Ambient Mode: Skybox");
        Debug.Log($"  - Ambient Intensity: 0.5");
        Debug.Log($"  - Fog: Enabled (dark blue)");

        EditorUtility.DisplayDialog("Success!",
            $"Applied Night Moon Burst sky to 1st Floor scene!\n\n" +
            "Settings:\n" +
            "• Skybox: Night Moon Burst\n" +
            "• Ambient lighting adjusted for night\n" +
            "• Fog enabled with dark blue color\n\n" +
            "Don't forget to save the scene (Ctrl+S)!",
            "OK");
    }
}

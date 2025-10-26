using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class CopyHealthUITo1stFloor : EditorWindow
{
    [MenuItem("Tools/Copy HealthUI from 3rd Floor to 1st Floor")]
    public static void CopyHealthUI()
    {
        // Save current scene first
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.isDirty)
        {
            bool save = EditorUtility.DisplayDialog("Save Current Scene?",
                "Do you want to save the current scene before proceeding?",
                "Save", "Don't Save");

            if (save)
            {
                EditorSceneManager.SaveScene(currentScene);
            }
        }

        // Open 3rd floor scene to find HealthUI
        string thirdFloorPath = "Assets/Scenes/3rd floor (better version).unity";
        Scene thirdFloorScene = EditorSceneManager.OpenScene(thirdFloorPath, OpenSceneMode.Single);

        if (!thirdFloorScene.IsValid())
        {
            EditorUtility.DisplayDialog("Error", "Could not open 3rd floor scene!", "OK");
            return;
        }

        // Find HealthUI in the scene
        GameObject healthUI = null;
        GameObject[] rootObjects = thirdFloorScene.GetRootGameObjects();

        foreach (GameObject rootObj in rootObjects)
        {
            // Check if this is the HealthUI or contains it
            if (rootObj.name == "HealthUi" || rootObj.name == "HealthUI")
            {
                healthUI = rootObj;
                break;
            }

            // Search in children
            Transform found = rootObj.transform.Find("HealthUi");
            if (found == null)
                found = rootObj.transform.Find("HealthUI");

            if (found != null)
            {
                healthUI = found.gameObject;
                break;
            }

            // Deep search
            foreach (Transform child in rootObj.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "HealthUi" || child.name == "HealthUI")
                {
                    healthUI = child.gameObject;
                    break;
                }
            }

            if (healthUI != null) break;
        }

        if (healthUI == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find HealthUI in 3rd floor scene!", "OK");
            return;
        }

        Debug.Log($"Found HealthUI: {healthUI.name} in 3rd floor scene");

        // Create a prefab from the HealthUI temporarily
        string tempPrefabPath = "Assets/TempHealthUI.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(healthUI, tempPrefabPath);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not create temporary prefab!", "OK");
            return;
        }

        Debug.Log("Created temporary prefab");

        // Now open 1st Floor scene
        string firstFloorPath = "Assets/Scenes/1st Floor.unity";
        Scene firstFloorScene = EditorSceneManager.OpenScene(firstFloorPath, OpenSceneMode.Single);

        if (!firstFloorScene.IsValid())
        {
            // Try the GOOD ENDING version if regular 1st Floor doesn't exist
            firstFloorPath = "Assets/Scenes/1st Floor (GOOD ENDING).unity";
            firstFloorScene = EditorSceneManager.OpenScene(firstFloorPath, OpenSceneMode.Single);
        }

        if (!firstFloorScene.IsValid())
        {
            AssetDatabase.DeleteAsset(tempPrefabPath);
            EditorUtility.DisplayDialog("Error", "Could not open 1st Floor scene!", "OK");
            return;
        }

        // Check if HealthUI already exists in 1st floor
        GameObject existingHealthUI = null;
        GameObject[] firstFloorRootObjects = firstFloorScene.GetRootGameObjects();

        foreach (GameObject rootObj in firstFloorRootObjects)
        {
            if (rootObj.name == "HealthUi" || rootObj.name == "HealthUI")
            {
                existingHealthUI = rootObj;
                break;
            }

            foreach (Transform child in rootObj.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "HealthUi" || child.name == "HealthUI")
                {
                    existingHealthUI = child.gameObject;
                    break;
                }
            }

            if (existingHealthUI != null) break;
        }

        if (existingHealthUI != null)
        {
            bool replace = EditorUtility.DisplayDialog("HealthUI Already Exists",
                "HealthUI already exists in 1st Floor. Do you want to replace it?",
                "Replace", "Cancel");

            if (!replace)
            {
                AssetDatabase.DeleteAsset(tempPrefabPath);
                return;
            }

            DestroyImmediate(existingHealthUI);
            Debug.Log("Removed existing HealthUI from 1st Floor");
        }

        // Instantiate the prefab in 1st Floor scene
        GameObject newHealthUI = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        newHealthUI.name = healthUI.name;

        Debug.Log($"Added HealthUI to 1st Floor scene: {newHealthUI.name}");

        // Try to find Canvas to parent to
        Canvas canvas = null;
        foreach (GameObject rootObj in firstFloorRootObjects)
        {
            canvas = rootObj.GetComponentInChildren<Canvas>(true);
            if (canvas != null) break;
        }

        if (canvas != null)
        {
            newHealthUI.transform.SetParent(canvas.transform, false);
            Debug.Log($"Parented HealthUI to Canvas: {canvas.gameObject.name}");
        }

        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(firstFloorScene);

        // Clean up temporary prefab
        AssetDatabase.DeleteAsset(tempPrefabPath);

        Debug.Log("<color=green>Successfully copied HealthUI from 3rd Floor to 1st Floor!</color>");
        EditorUtility.DisplayDialog("Success!",
            "HealthUI has been copied from 3rd Floor to 1st Floor!\n\n" +
            "Don't forget to save the scene (Ctrl+S).",
            "OK");
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class UpdateLacquerFont : EditorWindow
{
    private TMP_FontAsset lacquerFont;
    private List<string> logMessages = new List<string>();

    [MenuItem("Tools/Update UI Fonts to Lacquer")]
    public static void ShowWindow()
    {
        GetWindow<UpdateLacquerFont>("Update to Lacquer Font");
    }

    private void OnGUI()
    {
        GUILayout.Label("Update All In-Game UI Fonts to Lacquer", EditorStyles.boldLabel);
        GUILayout.Label("(Excludes: Pause Menu, Death Screen, Game Over)", EditorStyles.miniLabel);
        GUILayout.Space(10);

        lacquerFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "Lacquer SDF Font:",
            lacquerFont,
            typeof(TMP_FontAsset),
            false
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Auto-Find Lacquer SDF"))
        {
            FindLacquerFont();
        }

        GUILayout.Space(10);

        GUI.enabled = lacquerFont != null;

        if (GUILayout.Button("Update All Floor Scenes"))
        {
            UpdateAllScenes();
        }

        GUI.enabled = true;

        GUILayout.Space(20);
        GUILayout.Label("Log:", EditorStyles.boldLabel);

        if (logMessages.Count > 0)
        {
            GUILayout.BeginVertical("box");
            foreach (var message in logMessages)
            {
                GUILayout.Label(message, EditorStyles.wordWrappedLabel);
            }
            GUILayout.EndVertical();
        }
    }

    private void FindLacquerFont()
    {
        logMessages.Clear();

        // Search for Lacquer SDF in Assets/Assets/Fonts
        string[] guids = AssetDatabase.FindAssets("Lacquer SDF t:TMP_FontAsset", new[] { "Assets/Assets/Fonts" });

        if (guids.Length == 0)
        {
            // Try broader search
            guids = AssetDatabase.FindAssets("Lacquer SDF t:TMP_FontAsset");
        }

        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            lacquerFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            logMessages.Add($"Found Lacquer SDF at: {path}");
            Debug.Log($"Found Lacquer SDF at: {path}");
        }
        else
        {
            logMessages.Add("ERROR: Could not find 'Lacquer SDF' font asset!");
            logMessages.Add("Make sure you've generated the TextMeshPro SDF asset.");
            Debug.LogError("Lacquer SDF not found!");
        }
    }

    private void UpdateAllScenes()
    {
        if (lacquerFont == null)
        {
            logMessages.Add("ERROR: Please select or find Lacquer SDF font first!");
            return;
        }

        logMessages.Clear();
        logMessages.Add("Starting font update process...");
        logMessages.Add("-----------------------------------");

        string[] scenePaths = new string[]
        {
            "Assets/Scenes/1st Floor (GOOD ENDING).unity",
            "Assets/Scenes/1st Floor (BAD ENDING).unity",
            "Assets/Scenes/2nd Floor (Better Version).unity",
            "Assets/Scenes/3rd floor (better version).unity",
            "Assets/Scenes/4th Floor (better version).unity"
        };

        int totalUpdated = 0;

        foreach (string scenePath in scenePaths)
        {
            int sceneUpdates = UpdateScene(scenePath);
            totalUpdated += sceneUpdates;
        }

        logMessages.Add("-----------------------------------");
        logMessages.Add($"COMPLETE! Updated {totalUpdated} text components across all scenes.");
        Debug.Log($"Font update complete! Updated {totalUpdated} components.");

        AssetDatabase.SaveAssets();
    }

    private int UpdateScene(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        logMessages.Add($"\nProcessing: {scene.name}");

        int updatedCount = 0;

        // Find all TextMeshProUGUI components in the scene
        TextMeshProUGUI[] allTextComponents = GameObject.FindObjectsOfType<TextMeshProUGUI>(true);

        foreach (var textComponent in allTextComponents)
        {
            GameObject obj = textComponent.gameObject;
            string objName = obj.name.ToLower();
            string parentName = obj.transform.parent != null ? obj.transform.parent.name.ToLower() : "";
            string fullPath = GetGameObjectPath(obj).ToLower();

            // Check if this component should be EXCLUDED (pause menu, death screen, game over)
            bool shouldExclude = false;

            // Exclude pause menu elements
            if (objName.Contains("pause") || parentName.Contains("pause") || fullPath.Contains("pause"))
            {
                shouldExclude = true;
            }
            // Exclude death/game over screen elements
            else if (objName.Contains("gameover") || parentName.Contains("gameover") || fullPath.Contains("gameover") ||
                     objName.Contains("game over") || parentName.Contains("game over") || fullPath.Contains("game over") ||
                     objName.Contains("deathscreen") || parentName.Contains("deathscreen") || fullPath.Contains("deathscreen") ||
                     objName.Contains("death screen") || parentName.Contains("death screen") || fullPath.Contains("death screen"))
            {
                shouldExclude = true;
            }
            // Exclude "You Died" or similar death messages
            else if (objName.Contains("youdied") || parentName.Contains("youdied") ||
                     objName.Contains("you died") || parentName.Contains("you died"))
            {
                shouldExclude = true;
            }

            // If not excluded, update ALL text components to Lacquer
            if (!shouldExclude)
            {
                // Determine category for logging
                string category = DetermineCategory(obj, objName, parentName);

                textComponent.font = lacquerFont;
                EditorUtility.SetDirty(textComponent);
                updatedCount++;

                string path = GetGameObjectPath(obj);
                logMessages.Add($"  [{category}] Updated: {path}");
                Debug.Log($"Updated {category} font: {path}");
            }
            else
            {
                string path = GetGameObjectPath(obj);
                logMessages.Add($"  [SKIPPED] Excluded: {path}");
                Debug.Log($"Skipped excluded element: {path}");
            }
        }

        EditorSceneManager.SaveScene(scene);
        logMessages.Add($"  Total updated in {scene.name}: {updatedCount}");

        return updatedCount;
    }

    private string DetermineCategory(GameObject obj, string objName, string parentName)
    {
        // Quest UI
        if (objName.Contains("quest") || parentName.Contains("quest") ||
            obj.GetComponent<QuestManager>() != null || obj.GetComponentInParent<QuestManager>() != null)
        {
            return "Quest";
        }
        // Clue UI
        else if (objName.Contains("clue") || parentName.Contains("clue") ||
                 objName.Contains("code") || parentName.Contains("code") ||
                 objName.Contains("progress") || parentName.Contains("progress") ||
                 obj.GetComponent<ClueManager>() != null || obj.GetComponentInParent<ClueManager>() != null)
        {
            return "Clue";
        }
        // Health UI
        else if (objName.Contains("health") || parentName.Contains("health") ||
                 objName.Contains("status") || parentName.Contains("status") ||
                 obj.GetComponent<PlayerHealthSystem>() != null || obj.GetComponentInParent<PlayerHealthSystem>() != null)
        {
            return "Health";
        }
        // Dialogue UI
        else if (objName.Contains("dialogue") || parentName.Contains("dialogue") ||
                 objName.Contains("dialog") || parentName.Contains("dialog") ||
                 obj.GetComponent<DialogueManager>() != null || obj.GetComponentInParent<DialogueManager>() != null)
        {
            return "Dialogue";
        }
        // Prompt UI (pickup, interact, press, etc.)
        else if (objName.Contains("prompt") || parentName.Contains("prompt") ||
                 objName.Contains("pickup") || parentName.Contains("pickup") ||
                 objName.Contains("interact") || parentName.Contains("interact") ||
                 objName.Contains("press") || parentName.Contains("press") ||
                 objName.Contains("hint") || parentName.Contains("hint") ||
                 objName.Contains("instruction") || parentName.Contains("instruction"))
        {
            return "Prompt";
        }
        // Flashlight UI
        else if (objName.Contains("flashlight") || parentName.Contains("flashlight") ||
                 objName.Contains("battery") || parentName.Contains("battery"))
        {
            return "Flashlight";
        }
        // Note/Letter UI
        else if (objName.Contains("note") || parentName.Contains("note") ||
                 objName.Contains("letter") || parentName.Contains("letter") ||
                 objName.Contains("document") || parentName.Contains("document"))
        {
            return "Note";
        }
        // Inventory UI
        else if (objName.Contains("inventory") || parentName.Contains("inventory") ||
                 objName.Contains("item") || parentName.Contains("item"))
        {
            return "Inventory";
        }
        // Stamina UI
        else if (objName.Contains("stamina") || parentName.Contains("stamina"))
        {
            return "Stamina";
        }
        // Default: In-Game UI
        else
        {
            return "In-Game UI";
        }
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}

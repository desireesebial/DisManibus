using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class AddFlashlightIcon : EditorWindow
{
    private Sprite flashlightIconSprite;
    private Vector2 iconSize = new Vector2(40, 40);
    private Vector2 iconOffset = new Vector2(0, 50);
    private List<string> logMessages = new List<string>();

    [MenuItem("Tools/Add Flashlight Icon to Battery UI")]
    public static void ShowWindow()
    {
        GetWindow<AddFlashlightIcon>("Add Flashlight Icon");
    }

    private void OnGUI()
    {
        GUILayout.Label("Add Flashlight Icon to Battery UI", EditorStyles.boldLabel);
        GUILayout.Space(10);

        flashlightIconSprite = (Sprite)EditorGUILayout.ObjectField(
            "Flashlight Icon Sprite:",
            flashlightIconSprite,
            typeof(Sprite),
            false
        );

        GUILayout.Space(10);

        GUILayout.Label("Icon Settings:", EditorStyles.boldLabel);
        iconSize = EditorGUILayout.Vector2Field("Icon Size (pixels):", iconSize);
        iconOffset = EditorGUILayout.Vector2Field("Offset from Battery Bar:", iconOffset);

        GUILayout.Space(10);

        GUI.enabled = flashlightIconSprite != null;

        if (GUILayout.Button("Add Icon to All Floor Scenes"))
        {
            AddIconToAllScenes();
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

    private void AddIconToAllScenes()
    {
        if (flashlightIconSprite == null)
        {
            logMessages.Add("ERROR: Please assign a flashlight icon sprite first!");
            return;
        }

        logMessages.Clear();
        logMessages.Add("Starting icon addition process...");
        logMessages.Add("-----------------------------------");

        string[] scenePaths = new string[]
        {
            "Assets/Scenes/1st Floor (GOOD ENDING).unity",
            "Assets/Scenes/1st Floor (BAD ENDING).unity",
            "Assets/Scenes/2nd Floor (Better Version).unity",
            "Assets/Scenes/3rd floor (better version).unity",
            "Assets/Scenes/4th Floor (better version).unity"
        };

        int successCount = 0;

        foreach (string scenePath in scenePaths)
        {
            bool success = AddIconToScene(scenePath);
            if (success) successCount++;
        }

        logMessages.Add("-----------------------------------");
        logMessages.Add($"COMPLETE! Added icon to {successCount}/{scenePaths.Length} scenes.");
        Debug.Log($"Icon addition complete! {successCount} scenes updated.");

        AssetDatabase.SaveAssets();
    }

    private bool AddIconToScene(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        logMessages.Add($"\nProcessing: {scene.name}");

        // Find SimplifiedFlashlightBatteryUI component in the scene
        SimplifiedFlashlightBatteryUI batteryUI = GameObject.FindObjectOfType<SimplifiedFlashlightBatteryUI>();

        if (batteryUI == null)
        {
            logMessages.Add($"  ERROR: No SimplifiedFlashlightBatteryUI found in {scene.name}");
            Debug.LogWarning($"No SimplifiedFlashlightBatteryUI in {scene.name}");
            return false;
        }

        // Check if icon already exists
        Transform existingIcon = batteryUI.transform.Find("FlashlightIcon");
        if (existingIcon != null)
        {
            logMessages.Add($"  Icon already exists, updating...");
            GameObject.DestroyImmediate(existingIcon.gameObject);
        }

        // Create the icon GameObject
        GameObject iconGO = new GameObject("FlashlightIcon");
        iconGO.transform.SetParent(batteryUI.transform, false);

        // Add Image component
        Image iconImage = iconGO.AddComponent<Image>();
        iconImage.sprite = flashlightIconSprite;
        iconImage.color = Color.white;
        iconImage.raycastTarget = false;

        // Configure RectTransform for positioning above the battery bar
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0);
        iconRect.anchorMax = new Vector2(0, 0);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = iconSize;
        iconRect.anchoredPosition = iconOffset;

        // Assign icon reference to the battery UI script
        batteryUI.flashlightIcon = iconImage;
        EditorUtility.SetDirty(batteryUI);

        logMessages.Add($"  SUCCESS: Added flashlight icon above battery bar");
        logMessages.Add($"    - Size: {iconSize}");
        logMessages.Add($"    - Offset: {iconOffset}");
        Debug.Log($"Added flashlight icon to {scene.name}");

        EditorSceneManager.SaveScene(scene);
        return true;
    }
}

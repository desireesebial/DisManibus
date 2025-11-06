using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Editor tool to automatically set up flashlight icon with circle background
/// Works with FlashlightIconToggle component
/// Adds circle background, assigns on/off sprites, and configures colors
/// </summary>
public class SetupFlashlightCircleBackground : EditorWindow
{
    [MenuItem("Tools/Setup Flashlight Circle Background")]
    public static void AutoSetup()
    {
        // Load the on/off sprites
        string onSpritePath = "Assets/Textures/UI/flashlight on.png";
        string offSpritePath = "Assets/Textures/UI/flashlight off.png";

        // Configure sprites if needed
        ConfigureSpriteSettings(onSpritePath);
        ConfigureSpriteSettings(offSpritePath);

        // Load the sprites
        Sprite onSprite = AssetDatabase.LoadAssetAtPath<Sprite>(onSpritePath);
        Sprite offSprite = AssetDatabase.LoadAssetAtPath<Sprite>(offSpritePath);

        if (onSprite == null || offSprite == null)
        {
            Debug.LogError("Could not load flashlight sprites!");
            EditorUtility.DisplayDialog("Error",
                "Could not load flashlight on.png or flashlight off.png!\n\nPlease check that these files exist in Assets/Textures/UI/",
                "OK");
            return;
        }

        Debug.Log("Found flashlight sprites: " + onSprite.name + ", " + offSprite.name);

        // Add circle background to all scenes
        SetupAllScenes(onSprite, offSprite);
    }

    private static void ConfigureSpriteSettings(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (importer == null)
        {
            Debug.LogError("Could not get TextureImporter for: " + assetPath);
            return;
        }

        bool needsReimport = false;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            needsReimport = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            needsReimport = true;
        }

        if (needsReimport)
        {
            Debug.Log("Configuring " + System.IO.Path.GetFileName(assetPath) + " as Sprite (2D and UI)...");
            importer.SaveAndReimport();
            AssetDatabase.Refresh();
        }
    }

    private static void SetupAllScenes(Sprite onSprite, Sprite offSprite)
    {
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/1st Floor (GOOD ENDING).unity",
            "Assets/Scenes/1st Floor (BAD ENDING).unity",
            "Assets/Scenes/2nd Floor (Better Version).unity",
            "Assets/Scenes/3rd floor (better version).unity",
            "Assets/Scenes/4th Floor (better version).unity"
        };

        int successCount = 0;
        List<string> results = new List<string>();

        results.Add("=== Flashlight Circle Background Setup ===\n");

        foreach (string scenePath in scenePaths)
        {
            bool success = SetupScene(scenePath, onSprite, offSprite, results);
            if (success) successCount++;
        }

        results.Add($"\n=== COMPLETE ===");
        results.Add($"Successfully updated {successCount}/{scenePaths.Length} scenes.");

        string fullLog = string.Join("\n", results);
        Debug.Log(fullLog);

        EditorUtility.DisplayDialog(
            "Flashlight Circle Background Setup Complete",
            $"Updated {successCount}/{scenePaths.Length} scenes with circle background and on/off sprites.\n\nComponent: FlashlightIconToggle\n\nCheck Console for details.",
            "OK"
        );

        AssetDatabase.SaveAssets();
    }

    private static bool SetupScene(string scenePath, Sprite onSprite, Sprite offSprite, List<string> log)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        log.Add($"\nProcessing: {scene.name}");

        // Find FlashlightIconToggle component (the actual battery UI script being used)
        FlashlightIconToggle batteryUI = GameObject.FindObjectOfType<FlashlightIconToggle>();

        if (batteryUI == null)
        {
            log.Add($"  ERROR: No FlashlightIconToggle found!");
            Debug.LogWarning($"No FlashlightIconToggle in {scene.name}");
            return false;
        }

        log.Add($"  Found FlashlightIconToggle on: {batteryUI.gameObject.name}");

        // Check if FlashlightIcon exists
        Transform existingIcon = batteryUI.transform.Find("FlashlightIcon");
        GameObject iconGO = null;
        Image iconImage = null;

        if (existingIcon == null)
        {
            log.Add($"  WARNING: FlashlightIcon not found, checking if it's assigned in component...");
            // Check if icon is already assigned in the component
            if (batteryUI.flashlightIcon != null)
            {
                iconImage = batteryUI.flashlightIcon;
                iconGO = iconImage.gameObject;
                log.Add($"  Found flashlight icon via component reference");
            }
            else
            {
                log.Add($"  ERROR: No FlashlightIcon found! Please run 'Setup Flashlight Icon (Auto)' first.");
                return false;
            }
        }
        else
        {
            iconGO = existingIcon.gameObject;
            iconImage = iconGO.GetComponent<Image>();
            log.Add($"  Found FlashlightIcon GameObject");
        }

        // Check if circle background already exists
        Transform existingCircle = batteryUI.transform.Find("CircleBackground");
        if (existingCircle != null)
        {
            log.Add($"  Updating existing CircleBackground...");
            GameObject.DestroyImmediate(existingCircle.gameObject);
        }

        // Create circle background GameObject
        GameObject circleGO = new GameObject("CircleBackground");
        circleGO.transform.SetParent(batteryUI.transform, false);

        // Add Image component
        Image circleImage = circleGO.AddComponent<Image>();

        // Use Unity's built-in circle sprite (Knob)
        Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        if (circleSprite == null)
        {
            // Fallback to UISprite
            circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        circleImage.sprite = circleSprite;
        circleImage.color = new Color(0.2f, 0.2f, 0.2f, 0.6f); // Default to off color
        circleImage.raycastTarget = false;
        circleImage.type = Image.Type.Simple;

        // Configure RectTransform - same size and position as icon
        RectTransform circleRect = circleGO.GetComponent<RectTransform>();
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();

        circleRect.anchorMin = iconRect.anchorMin;
        circleRect.anchorMax = iconRect.anchorMax;
        circleRect.pivot = iconRect.pivot;
        circleRect.anchoredPosition = iconRect.anchoredPosition;
        circleRect.sizeDelta = new Vector2(50, 50); // Slightly larger than icon for padding

        // Move circle to be before icon in hierarchy (renders behind)
        int iconIndex = iconGO.transform.GetSiblingIndex();
        circleGO.transform.SetSiblingIndex(iconIndex);

        // Assign all references to FlashlightIconToggle
        batteryUI.flashlightIcon = iconImage;
        batteryUI.flashlightOnSprite = onSprite;
        batteryUI.flashlightOffSprite = offSprite;
        batteryUI.circleBackground = circleImage;
        batteryUI.circleOnColor = new Color(1f, 0.8f, 0f, 0.8f); // Bright yellow-orange
        batteryUI.circleOffColor = new Color(0.2f, 0.2f, 0.2f, 0.6f); // Dark gray

        EditorUtility.SetDirty(batteryUI);

        log.Add($"  ✓ SUCCESS: Added circle background and configured sprites");
        log.Add($"    - Circle size: 50x50 pixels");
        log.Add($"    - On sprite: {onSprite.name}");
        log.Add($"    - Off sprite: {offSprite.name}");
        log.Add($"    - Circle renders behind icon");
        log.Add($"    - Component: FlashlightIconToggle");

        EditorSceneManager.SaveScene(scene);
        return true;
    }
}

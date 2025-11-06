using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Automated setup for flashlight icon - finds the icon sprite and adds it to all scenes
/// </summary>
public class FlashlightIconAutoSetup : EditorWindow
{
    [MenuItem("Tools/Setup Flashlight Icon (Auto)")]
    public static void AutoSetup()
    {
        // Find the flashlight_white.png sprite (white version for visibility)
        string iconPath = "Assets/Textures/UI/flashlight_white.png";

        // Configure it as a sprite if needed
        ConfigureSpriteSettings(iconPath);

        // Load the sprite
        Sprite flashlightSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

        if (flashlightSprite == null)
        {
            Debug.LogError("Could not load flashlight sprite from: " + iconPath);
            EditorUtility.DisplayDialog("Error", "Could not load flashlight_white.png!\n\nPlease run 'Create White Flashlight Icon' first.", "OK");
            return;
        }

        Debug.Log("Found flashlight sprite: " + flashlightSprite.name);

        // Add to all scenes
        AddIconToAllScenes(flashlightSprite);
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
            Debug.Log("Configuring flashlight_white.png as Sprite (2D and UI)...");
            importer.SaveAndReimport();
            AssetDatabase.Refresh();
        }
    }

    private static void AddIconToAllScenes(Sprite iconSprite)
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

        results.Add("=== Flashlight Icon Setup ===\n");

        foreach (string scenePath in scenePaths)
        {
            bool success = AddIconToScene(scenePath, iconSprite, results);
            if (success) successCount++;
        }

        results.Add($"\n=== COMPLETE ===");
        results.Add($"Successfully added icon to {successCount}/{scenePaths.Length} scenes.");

        string fullLog = string.Join("\n", results);
        Debug.Log(fullLog);

        EditorUtility.DisplayDialog(
            "Flashlight Icon Setup Complete",
            $"Added flashlight icon to {successCount}/{scenePaths.Length} scenes.\n\nCheck Console for details.",
            "OK"
        );

        AssetDatabase.SaveAssets();
    }

    private static bool AddIconToScene(string scenePath, Sprite iconSprite, List<string> log)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        log.Add($"\nProcessing: {scene.name}");

        // Find Canvas and Battery GameObject
        GameObject batteryGO = FindBatteryGameObject();

        if (batteryGO == null)
        {
            log.Add($"  ERROR: No Battery GameObject found under Canvas!");
            Debug.LogWarning($"No Battery GameObject in {scene.name}");
            return false;
        }

        log.Add($"  Found Battery GameObject: {GetGameObjectPath(batteryGO)}");

        // Check if icon already exists
        Transform existingIcon = batteryGO.transform.Find("FlashlightIcon");
        if (existingIcon != null)
        {
            log.Add($"  Updating existing icon...");
            GameObject.DestroyImmediate(existingIcon.gameObject);
        }

        // Create icon GameObject
        GameObject iconGO = new GameObject("FlashlightIcon");
        iconGO.transform.SetParent(batteryGO.transform, false);

        // Add Image component
        Image iconImage = iconGO.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.color = Color.white; // White color for visibility in dark areas
        iconImage.raycastTarget = false;
        iconImage.preserveAspect = true;
        iconImage.material = null; // Use default UI material

        // Configure RectTransform - position at center-top of battery slider
        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 1f); // Center horizontal, top vertical
        iconRect.anchorMax = new Vector2(0.5f, 1f); // Center horizontal, top vertical
        iconRect.pivot = new Vector2(0.5f, 0f); // Pivot at bottom-center of icon
        iconRect.sizeDelta = new Vector2(40, 40);
        iconRect.anchoredPosition = new Vector2(0, 5); // 5 pixels above battery slider

        // Add or update FlashlightIconToggle component
        FlashlightIconToggle iconToggle = batteryGO.GetComponent<FlashlightIconToggle>();
        if (iconToggle == null)
        {
            iconToggle = batteryGO.AddComponent<FlashlightIconToggle>();
            log.Add($"  Added FlashlightIconToggle component");
        }

        // Auto-assign references
        FlashlightController flashlight = GameObject.FindObjectOfType<FlashlightController>();
        iconToggle.flashlight = flashlight;
        iconToggle.flashlightIcon = iconImage;

        EditorUtility.SetDirty(batteryGO);

        log.Add($"  ✓ SUCCESS: Added icon with toggle functionality");
        log.Add($"    - Position: Center-top of battery slider");
        log.Add($"    - Size: 40x40 pixels");
        log.Add($"    - Sprite swapping: Ready (assign sprites with Setup Flashlight Circle Background)");

        EditorSceneManager.SaveScene(scene);
        return true;
    }

    private static GameObject FindBatteryGameObject()
    {
        // Find Canvas
        Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in canvases)
        {
            // Look for Battery GameObject under this canvas
            Transform batteryTransform = canvas.transform.Find("Battery");
            if (batteryTransform != null)
            {
                return batteryTransform.gameObject;
            }
        }

        return null;
    }

    private static string GetGameObjectPath(GameObject obj)
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

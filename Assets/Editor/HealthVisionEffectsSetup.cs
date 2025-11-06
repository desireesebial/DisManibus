using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automatically set up health vision effects in the current scene
/// Creates the VignetteOverlay UI and HealthVisionEffects component with one click
/// </summary>
public class HealthVisionEffectsSetup : EditorWindow
{
    [MenuItem("Tools/Setup Health Vision Effects")]
    static void SetupHealthVisionEffects()
    {
        // Check if scene is loaded
        Scene currentScene = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(currentScene.name))
        {
            EditorUtility.DisplayDialog("Error", "No scene is currently loaded!", "OK");
            return;
        }

        // Check if HealthVisionEffects already exists
        HealthVisionEffects existing = Object.FindObjectOfType<HealthVisionEffects>();
        if (existing != null)
        {
            bool overwrite = EditorUtility.DisplayDialog(
                "Health Vision Effects Already Exists",
                $"HealthVisionEffects already exists in this scene on GameObject '{existing.gameObject.name}'.\n\nDo you want to set it up again? This will create new UI elements.",
                "Yes, Set Up Again",
                "Cancel"
            );

            if (!overwrite)
            {
                return;
            }
        }

        // Find Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in the scene!\n\nPlease add a Canvas first.", "OK");
            return;
        }

        // Load the vignette texture
        string texturePath = "Assets/Textures/UI/HealthVignette.png";
        Sprite vignetteSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);

        if (vignetteSprite == null)
        {
            bool generateTexture = EditorUtility.DisplayDialog(
                "Vignette Texture Not Found",
                $"The vignette texture was not found at:\n{texturePath}\n\nWould you like to open the Vignette Generator to create it first?",
                "Yes, Open Generator",
                "Cancel"
            );

            if (generateTexture)
            {
                EditorApplication.ExecuteMenuItem("Tools/Generate Vignette Texture");
            }
            return;
        }

        // Start the setup process
        Debug.Log("[HealthVisionSetup] Starting automatic setup...");

        // 1. Create VignetteOverlay UI Image
        GameObject vignetteOverlayGO = CreateVignetteOverlay(canvas, vignetteSprite);

        // 2. Create HealthVisionEffects GameObject
        GameObject healthVisionGO = CreateHealthVisionEffectsObject(canvas.transform);

        // 3. Add and configure the component
        HealthVisionEffects healthVisionComponent = healthVisionGO.AddComponent<HealthVisionEffects>();
        ConfigureHealthVisionEffects(healthVisionComponent, vignetteOverlayGO);

        // Mark scene as dirty so Unity knows to save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);

        // Select the created object
        Selection.activeGameObject = healthVisionGO;
        EditorGUIUtility.PingObject(healthVisionGO);

        // Success message
        Debug.Log("[HealthVisionSetup] Setup complete!");
        EditorUtility.DisplayDialog(
            "Success!",
            "Health Vision Effects have been set up successfully!\n\n" +
            "Created:\n" +
            $"- {vignetteOverlayGO.name} (UI overlay)\n" +
            $"- {healthVisionGO.name} (effect controller)\n\n" +
            "The HealthVisionEffects component has been configured with default settings.\n\n" +
            "PlayerHealthSystem will be auto-detected at runtime.\n\n" +
            "Don't forget to save your scene!",
            "OK"
        );
    }

    static GameObject CreateVignetteOverlay(Canvas canvas, Sprite vignetteSprite)
    {
        // Check if VignetteOverlay already exists
        Transform existingOverlay = canvas.transform.Find("VignetteOverlay");
        if (existingOverlay != null)
        {
            Debug.LogWarning("[HealthVisionSetup] VignetteOverlay already exists, using existing one");
            return existingOverlay.gameObject;
        }

        // Create VignetteOverlay GameObject
        GameObject vignetteOverlay = new GameObject("VignetteOverlay");
        vignetteOverlay.transform.SetParent(canvas.transform, false);
        vignetteOverlay.layer = LayerMask.NameToLayer("UI");

        // Add RectTransform and configure for fullscreen
        RectTransform rectTransform = vignetteOverlay.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero; // Left and Bottom
        rectTransform.offsetMax = Vector2.zero; // Right and Top
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // Add Image component
        Image image = vignetteOverlay.AddComponent<Image>();
        image.sprite = vignetteSprite;
        image.color = new Color(0f, 0f, 0f, 0f); // Start fully transparent
        image.raycastTarget = false; // Don't block UI clicks

        // Move to bottom of Canvas hierarchy so it doesn't cover other UI
        vignetteOverlay.transform.SetAsFirstSibling();

        Debug.Log("[HealthVisionSetup] Created VignetteOverlay");
        return vignetteOverlay;
    }

    static GameObject CreateHealthVisionEffectsObject(Transform parent)
    {
        // Check if HealthVisionEffects already exists
        Transform existingVisionFX = parent.Find("HealthVisionEffects");
        if (existingVisionFX != null)
        {
            Debug.LogWarning("[HealthVisionSetup] HealthVisionEffects GameObject already exists, recreating component");
            // Remove existing component if any
            HealthVisionEffects existingComponent = existingVisionFX.GetComponent<HealthVisionEffects>();
            if (existingComponent != null)
            {
                Object.DestroyImmediate(existingComponent);
            }
            return existingVisionFX.gameObject;
        }

        // Create HealthVisionEffects GameObject
        GameObject healthVisionGO = new GameObject("HealthVisionEffects");
        healthVisionGO.transform.SetParent(parent, false);

        Debug.Log("[HealthVisionSetup] Created HealthVisionEffects GameObject");
        return healthVisionGO;
    }

    static void ConfigureHealthVisionEffects(HealthVisionEffects component, GameObject vignetteOverlay)
    {
        // Get the Image component from vignetteOverlay
        Image vignetteImage = vignetteOverlay.GetComponent<Image>();

        // Configure references
        component.vignetteOverlay = vignetteImage;

        // Try to find PlayerHealthSystem
        PlayerHealthSystem playerHealth = Object.FindObjectOfType<PlayerHealthSystem>();
        if (playerHealth != null)
        {
            component.playerHealthSystem = playerHealth;
            Debug.Log("[HealthVisionSetup] Found and linked PlayerHealthSystem");
        }
        else
        {
            Debug.LogWarning("[HealthVisionSetup] PlayerHealthSystem not found - will auto-detect at runtime");
        }

        // Configure vignette settings
        component.vignetteColor = new Color(0f, 0f, 0f, 1f); // Black
        component.maxVignetteIntensity = 0.8f;
        component.minVignetteIntensity = 0f;

        // Configure health thresholds (for 3-health system)
        component.fullHealthPercent = 1f;
        component.highHealthPercent = 0.66f;
        component.midHealthPercent = 0.33f;
        component.lowHealthPercent = 0.01f;

        // Configure intensity at each threshold
        component.vignetteAtFull = 0f;     // No vignette at full health
        component.vignetteAtHigh = 0.70f;  // Strong vignette at 2 HP
        component.vignetteAtMid = 0.5f;    // Moderate vignette at 1/3 HP
        component.vignetteAtLow = 1.0f;    // Maximum vignette at critical health (1 HP)

        // Configure blur settings
        component.enableBlur = true;       // Enable blur effects
        component.maxBlurIntensity = 5f;

        // Configure blur intensity at each threshold
        component.blurAtFull = 0f;         // No blur at full health
        component.blurAtHigh = 2.5f;       // Moderate blur at 2 HP
        component.blurAtMid = 3.0f;        // Higher blur at 1/3 HP
        component.blurAtLow = 3.5f;        // Strong blur at critical health (1 HP)

        // Configure transition settings
        component.transitionSpeed = 3f;
        component.useSmoothEasing = true;

        // Configure pulse effect
        component.enablePulseAtCritical = true;
        component.pulseSpeed = 2f;
        component.pulseAmount = 0.15f;

        // Debug
        component.showDebug = true;

        Debug.Log("[HealthVisionSetup] Configured HealthVisionEffects component with default settings");
    }
}

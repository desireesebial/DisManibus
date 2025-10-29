using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Editor script to automatically set up the flashlight battery UI in all floor scenes.
/// Access via Tools > Apply Flashlight Battery UI to All Floors
/// </summary>
public class FlashlightBatterySetup : EditorWindow
{
    // Floor scene paths
    private static readonly string[] floorScenePaths = new string[]
    {
        "Assets/Scenes/4th Floor (better version).unity",
        "Assets/Scenes/3rd floor (better version).unity",
        "Assets/Scenes/2nd Floor (Better Version).unity",
        "Assets/Scenes/1st Floor (BAD ENDING).unity"
    };

    [MenuItem("Tools/Apply Flashlight Battery UI to All Floors")]
    public static void ApplyBatteryUIToAllFloors()
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

        int successCount = 0;
        int failCount = 0;
        string errorMessages = "";

        // Process each floor
        for (int i = 0; i < floorScenePaths.Length; i++)
        {
            string scenePath = floorScenePaths[i];
            string floorName = GetFloorName(i);

            Debug.Log($"[FlashlightBatterySetup] Processing {floorName}...");

            try
            {
                if (SetupBatteryUIInScene(scenePath, floorName))
                {
                    successCount++;
                    Debug.Log($"<color=green>[FlashlightBatterySetup] Successfully set up {floorName}</color>");
                }
                else
                {
                    failCount++;
                    errorMessages += $"\n- {floorName}: Failed to set up";
                }
            }
            catch (System.Exception e)
            {
                failCount++;
                errorMessages += $"\n- {floorName}: {e.Message}";
                Debug.LogError($"[FlashlightBatterySetup] Error setting up {floorName}: {e.Message}");
            }
        }

        // Show results
        string resultMessage = $"Flashlight Battery UI Setup Complete!\n\n" +
                              $"Successful: {successCount}/{floorScenePaths.Length}\n" +
                              $"Failed: {failCount}/{floorScenePaths.Length}";

        if (!string.IsNullOrEmpty(errorMessages))
        {
            resultMessage += $"\n\nErrors:{errorMessages}";
        }

        resultMessage += "\n\nMake sure to save your scenes if you made any manual changes!";

        EditorUtility.DisplayDialog("Flashlight Battery UI Setup", resultMessage, "OK");

        Debug.Log($"<color=cyan>[FlashlightBatterySetup] {resultMessage}</color>");
    }

    private static bool SetupBatteryUIInScene(string scenePath, string floorName)
    {
        // Open the scene
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        if (!scene.IsValid())
        {
            Debug.LogError($"[FlashlightBatterySetup] Could not open scene: {scenePath}");
            return false;
        }

        // Find or create Canvas
        Canvas canvas = FindOrCreateCanvas(scene);
        if (canvas == null)
        {
            Debug.LogError($"[FlashlightBatterySetup] Could not find or create Canvas in {floorName}");
            return false;
        }

        // Find FlashlightController
        FlashlightController flashlightController = Object.FindObjectOfType<FlashlightController>();
        if (flashlightController == null)
        {
            Debug.LogWarning($"[FlashlightBatterySetup] No FlashlightController found in {floorName}. Battery UI will be created but won't function until FlashlightController is added.");
        }

        // Check if battery UI already exists
        SimplifiedFlashlightBatteryUI existingBatteryUI = Object.FindObjectOfType<SimplifiedFlashlightBatteryUI>();
        if (existingBatteryUI != null)
        {
            bool replace = EditorUtility.DisplayDialog("Battery UI Already Exists",
                $"A flashlight battery UI already exists in {floorName}. Do you want to replace it?",
                "Replace", "Skip");

            if (!replace)
            {
                Debug.Log($"[FlashlightBatterySetup] Skipped {floorName} (user choice)");
                return true; // Not a failure, user chose to skip
            }

            // Remove existing battery UI
            Object.DestroyImmediate(existingBatteryUI.gameObject);
            Debug.Log($"[FlashlightBatterySetup] Removed existing battery UI from {floorName}");
        }

        // Create battery UI
        GameObject batteryUIPanel = CreateBatteryUI(canvas);
        batteryUIPanel.transform.SetParent(canvas.transform, false);

        // Add SimplifiedFlashlightBatteryUI component
        SimplifiedFlashlightBatteryUI batteryUI = batteryUIPanel.AddComponent<SimplifiedFlashlightBatteryUI>();

        // Add FlashlightStaminaAnchoring component
        FlashlightStaminaAnchoring anchoring = batteryUIPanel.AddComponent<FlashlightStaminaAnchoring>();
        anchoring.SetOffsets(30f, 30f); // 30px from left and bottom

        // Get references
        Slider slider = batteryUIPanel.GetComponentInChildren<Slider>();
        Image fillImage = null;
        if (slider != null)
        {
            fillImage = slider.fillRect?.GetComponent<Image>();
        }

        // Find PlayerHealthSystem
        PlayerHealthSystem playerHealth = Object.FindObjectOfType<PlayerHealthSystem>();
        if (playerHealth == null)
        {
            Debug.LogWarning($"[FlashlightBatterySetup] No PlayerHealthSystem found in {floorName}. Battery UI won't hide on death.");
        }

        // Wire up references
        batteryUI.flashlight = flashlightController;
        batteryUI.batterySlider = slider;
        batteryUI.fillImage = fillImage;
        batteryUI.playerHealth = playerHealth;

        // Mark scene as dirty and save
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[FlashlightBatterySetup] Successfully set up battery UI in {floorName}");
        return true;
    }

    private static Canvas FindOrCreateCanvas(Scene scene)
    {
        // Find existing Canvas
        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject rootObj in rootObjects)
        {
            Canvas canvas = rootObj.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                Debug.Log($"[FlashlightBatterySetup] Found existing Canvas: {canvas.gameObject.name}");
                return canvas;
            }
        }

        // Create new Canvas if none found
        GameObject canvasObj = new GameObject("Canvas");
        Canvas newCanvas = canvasObj.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("[FlashlightBatterySetup] Created new Canvas");
        return newCanvas;
    }

    private static GameObject CreateBatteryUI(Canvas canvas)
    {
        // Create main panel
        GameObject panelObj = new GameObject("FlashlightBatteryUI");
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();

        // Will be positioned by FlashlightStaminaAnchoring
        panelRect.sizeDelta = new Vector2(200, 30);

        // Add background image
        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark semi-transparent

        // Create slider
        GameObject sliderObj = new GameObject("BatterySlider");
        sliderObj.transform.SetParent(panelObj.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.05f, 0.2f);
        sliderRect.anchorMax = new Vector2(0.95f, 0.8f);
        sliderRect.sizeDelta = Vector2.zero;

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.transition = Selectable.Transition.None; // No interaction

        // Create Background for slider
        GameObject sliderBgObj = new GameObject("Background");
        sliderBgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform sliderBgRect = sliderBgObj.AddComponent<RectTransform>();
        sliderBgRect.anchorMin = Vector2.zero;
        sliderBgRect.anchorMax = Vector2.one;
        sliderBgRect.sizeDelta = Vector2.zero;

        Image sliderBgImage = sliderBgObj.AddComponent<Image>();
        sliderBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray

        // Create Fill Area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, -10);
        fillAreaRect.anchoredPosition = Vector2.zero;

        // Create Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green; // Start with green
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        // Assign Fill Area and Fill to Slider
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        Debug.Log("[FlashlightBatterySetup] Created battery UI");
        return panelObj;
    }

    private static string GetFloorName(int index)
    {
        switch (index)
        {
            case 0: return "4th Floor";
            case 1: return "3rd Floor";
            case 2: return "2nd Floor";
            case 3: return "1st Floor";
            default: return "Unknown Floor";
        }
    }
}

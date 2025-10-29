using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Editor script to automatically set up the hint system in all floor scenes.
/// Access via Tools > Apply Hint System to All Floors
/// </summary>
public class HintSystemSetup : EditorWindow
{
    // Floor scene paths
    private static readonly string[] floorScenePaths = new string[]
    {
        "Assets/Scenes/4th Floor (better version).unity",
        "Assets/Scenes/3rd floor (better version).unity",
        "Assets/Scenes/2nd Floor (Better Version).unity",
        "Assets/Scenes/1st Floor (BAD ENDING).unity"
    };

    // Hint texts for each floor
    private static readonly string[] hintTexts = new string[]
    {
        "The scattered papers are important, make sure to remember where they are. Kamatayan has a pattern, you can avoid running into him.",
        "The papers in the lugage can complete a sentence, read them by combining the letters from top to buttom. Jenglot will stop when you aim the flashlight into him, use it well!",
        "The key on the bed can be used on the crate - The key on the crate can be used on the chest. There are 3 heads of dullahan, if you offer the 2 fake heads, it will lead you to the real one. Offer the real head to get the good ending.",
        "The shrine will protect you from Kuchisake Onna. Follow the clues on the paper to open the lantern on right pattern!"
    };

    [MenuItem("Tools/Apply Hint System to All Floors")]
    public static void ApplyHintSystemToAllFloors()
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
            string hintText = hintTexts[i];
            string floorName = GetFloorName(i);

            Debug.Log($"[HintSystemSetup] Processing {floorName}...");

            try
            {
                if (SetupHintSystemInScene(scenePath, hintText, floorName))
                {
                    successCount++;
                    Debug.Log($"<color=green>[HintSystemSetup] Successfully set up {floorName}</color>");
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
                Debug.LogError($"[HintSystemSetup] Error setting up {floorName}: {e.Message}");
            }
        }

        // Show results
        string resultMessage = $"Hint System Setup Complete!\n\n" +
                              $"Successful: {successCount}/{floorScenePaths.Length}\n" +
                              $"Failed: {failCount}/{floorScenePaths.Length}";

        if (!string.IsNullOrEmpty(errorMessages))
        {
            resultMessage += $"\n\nErrors:{errorMessages}";
        }

        resultMessage += "\n\nMake sure to save your scenes if you made any manual changes!";

        EditorUtility.DisplayDialog("Hint System Setup", resultMessage, "OK");

        Debug.Log($"<color=cyan>[HintSystemSetup] {resultMessage}</color>");
    }

    private static bool SetupHintSystemInScene(string scenePath, string hintText, string floorName)
    {
        // Open the scene
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        if (!scene.IsValid())
        {
            Debug.LogError($"[HintSystemSetup] Could not open scene: {scenePath}");
            return false;
        }

        // Find or create Canvas
        Canvas canvas = FindOrCreateCanvas(scene);
        if (canvas == null)
        {
            Debug.LogError($"[HintSystemSetup] Could not find or create Canvas in {floorName}");
            return false;
        }

        // Check if hint system already exists
        FloorHintSystem existingHintSystem = Object.FindObjectOfType<FloorHintSystem>();
        if (existingHintSystem != null)
        {
            bool replace = EditorUtility.DisplayDialog("Hint System Already Exists",
                $"A hint system already exists in {floorName}. Do you want to replace it?",
                "Replace", "Skip");

            if (!replace)
            {
                Debug.Log($"[HintSystemSetup] Skipped {floorName} (user choice)");
                return true; // Not a failure, user chose to skip
            }

            // Remove existing hint system
            if (existingHintSystem.hintButton != null)
                Object.DestroyImmediate(existingHintSystem.hintButton);
            if (existingHintSystem.hintPanel != null)
                Object.DestroyImmediate(existingHintSystem.hintPanel);
            Object.DestroyImmediate(existingHintSystem.gameObject);

            Debug.Log($"[HintSystemSetup] Removed existing hint system from {floorName}");
        }

        // Create hint system root object
        GameObject hintSystemRoot = new GameObject("HintSystemRoot");
        hintSystemRoot.transform.SetParent(canvas.transform, false);

        // Add FloorHintSystem component
        FloorHintSystem hintSystem = hintSystemRoot.AddComponent<FloorHintSystem>();
        hintSystem.hintText = hintText;
        hintSystem.timeBeforeHintAppears = 300f; // 5 minutes

        // Create hint button
        GameObject hintButton = CreateHintButton(canvas);
        hintButton.transform.SetParent(canvas.transform, false);
        hintSystem.hintButton = hintButton;

        // Create instruction text
        GameObject instructionTextObj = CreateInstructionText(canvas);
        instructionTextObj.transform.SetParent(canvas.transform, false);
        hintSystem.instructionText = instructionTextObj.GetComponent<Text>();

        // Create hint panel
        GameObject hintPanel = CreateHintPanel(canvas);
        hintPanel.transform.SetParent(canvas.transform, false);
        hintSystem.hintPanel = hintPanel;

        // Get hint text display reference
        Text[] texts = hintPanel.GetComponentsInChildren<Text>();
        foreach (Text text in texts)
        {
            if (text.gameObject.name == "HintText")
            {
                hintSystem.hintTextDisplay = text;
                break;
            }
        }

        // No button event listeners needed - hint panel is controlled by Z key only

        // Mark scene as dirty and save
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log($"[HintSystemSetup] Successfully set up hint system in {floorName}");
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
                Debug.Log($"[HintSystemSetup] Found existing Canvas: {canvas.gameObject.name}");
                return canvas;
            }
        }

        // Create new Canvas if none found
        GameObject canvasObj = new GameObject("Canvas");
        Canvas newCanvas = canvasObj.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        Debug.Log("[HintSystemSetup] Created new Canvas");
        return newCanvas;
    }

    private static GameObject CreateHintButton(Canvas canvas)
    {
        // Create hint indicator GameObject (visual only, not clickable)
        GameObject buttonObj = new GameObject("HintButton");
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();

        // Position at top-center
        buttonRect.anchorMin = new Vector2(0.5f, 1f);
        buttonRect.anchorMax = new Vector2(0.5f, 1f);
        buttonRect.pivot = new Vector2(0.5f, 1f);
        buttonRect.anchoredPosition = new Vector2(0, -20); // 20 pixels from top
        buttonRect.sizeDelta = new Vector2(80, 80);

        // Add Image component for background (visual only)
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.15f, 0f, 0f, 0.9f); // Dark red, mostly opaque

        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = "?";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 48;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = new Color(0.8f, 0f, 0f, 1f); // Dark red text

        // Add subtle outline for better visibility
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        Debug.Log("[HintSystemSetup] Created hint indicator (visual only)");
        return buttonObj;
    }

    private static GameObject CreateInstructionText(Canvas canvas)
    {
        // Create instruction text GameObject
        GameObject instructionObj = new GameObject("InstructionText");
        RectTransform instructionRect = instructionObj.AddComponent<RectTransform>();

        // Position below the hint button (button is at -20, button height is 80, so text starts at -100)
        instructionRect.anchorMin = new Vector2(0.5f, 1f);
        instructionRect.anchorMax = new Vector2(0.5f, 1f);
        instructionRect.pivot = new Vector2(0.5f, 1f);
        instructionRect.anchoredPosition = new Vector2(0, -105); // Just below the button
        instructionRect.sizeDelta = new Vector2(100, 25);

        // Add Text component
        Text instructionText = instructionObj.AddComponent<Text>();
        instructionText.text = "[Z] Open/Close";
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.fontSize = 14;
        instructionText.fontStyle = FontStyle.Bold;
        instructionText.alignment = TextAnchor.MiddleCenter;
        instructionText.color = new Color(0.7f, 0f, 0f, 1f); // Dark red text

        // Add outline for better visibility
        Outline outline = instructionObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        Debug.Log("[HintSystemSetup] Created instruction text");
        return instructionObj;
    }

    private static GameObject CreateHintPanel(Canvas canvas)
    {
        // Create panel GameObject
        GameObject panelObj = new GameObject("HintPanel");
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();

        // Position in center of screen
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(600, 400);

        // Add background Image
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.05f, 0.05f, 0.95f); // Very dark red, mostly opaque

        // Add border
        Outline panelOutline = panelObj.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.5f, 0f, 0f, 1f); // Red border
        panelOutline.effectDistance = new Vector2(3, -3);

        // Create title text
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.anchoredPosition = Vector2.zero;

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "HINT";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 32;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = new Color(0.7f, 0f, 0f, 1f); // Dark red

        // Create hint text
        GameObject hintTextObj = new GameObject("HintText");
        hintTextObj.transform.SetParent(panelObj.transform, false);
        RectTransform hintTextRect = hintTextObj.AddComponent<RectTransform>();
        hintTextRect.anchorMin = new Vector2(0.1f, 0.15f);
        hintTextRect.anchorMax = new Vector2(0.9f, 0.75f);
        hintTextRect.sizeDelta = Vector2.zero;
        hintTextRect.anchoredPosition = Vector2.zero;

        Text hintText = hintTextObj.AddComponent<Text>();
        hintText.text = "Hint text will appear here...";
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 20;
        hintText.alignment = TextAnchor.UpperLeft;
        hintText.color = new Color(0.9f, 0.8f, 0.8f, 1f); // Light pinkish-white
        hintText.horizontalOverflow = HorizontalWrapMode.Wrap;
        hintText.verticalOverflow = VerticalWrapMode.Overflow;

        // Create close instruction text
        GameObject closeInstructionObj = new GameObject("CloseInstruction");
        closeInstructionObj.transform.SetParent(panelObj.transform, false);
        RectTransform closeInstructionRect = closeInstructionObj.AddComponent<RectTransform>();
        closeInstructionRect.anchorMin = new Vector2(0.1f, 0.05f);
        closeInstructionRect.anchorMax = new Vector2(0.9f, 0.12f);
        closeInstructionRect.sizeDelta = Vector2.zero;
        closeInstructionRect.anchoredPosition = Vector2.zero;

        Text closeInstructionText = closeInstructionObj.AddComponent<Text>();
        closeInstructionText.text = "Press [Z] to close";
        closeInstructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeInstructionText.fontSize = 16;
        closeInstructionText.fontStyle = FontStyle.Bold;
        closeInstructionText.alignment = TextAnchor.MiddleCenter;
        closeInstructionText.color = new Color(0.7f, 0f, 0f, 1f); // Dark red text

        // Add outline for better visibility
        Outline closeInstructionOutline = closeInstructionObj.AddComponent<Outline>();
        closeInstructionOutline.effectColor = Color.black;
        closeInstructionOutline.effectDistance = new Vector2(1, -1);

        Debug.Log("[HintSystemSetup] Created hint panel");
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

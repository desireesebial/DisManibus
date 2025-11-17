using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using HealthSystem;

/// <summary>
/// Editor utility to automatically set up directional damage indicators in the current scene
/// Creates blood splatter UI elements and DirectionalDamageIndicator component with one click
/// </summary>
public class DirectionalDamageIndicatorSetup : EditorWindow
{
    private Sprite bloodSplatterSprite;
    private Canvas targetCanvas;
    private PlayerHealthSystem playerHealthSystem;
    private bool autoLinkToHealthSystem = true;

    // UI Settings
    private float splatterWidth = 1000f;
    private float splatterHeight = 600f;
    private float edgeOffset = 200f;

    private Vector2 scrollPosition;

    [MenuItem("Tools/Setup Directional Damage Indicator")]
    static void ShowWindow()
    {
        DirectionalDamageIndicatorSetup window = GetWindow<DirectionalDamageIndicatorSetup>("Damage Indicator Setup");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    [MenuItem("Tools/Quick Setup Directional Damage Indicator")]
    static void QuickSetup()
    {
        PerformQuickSetup();
    }

    void OnEnable()
    {
        // Try to find canvas and player health system automatically
        RefreshReferences();
    }

    void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Directional Damage Indicator Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool will automatically create and configure the directional damage indicator system.\n\n" +
            "It will create:\n" +
            "• 4 blood splatter UI images (top, bottom, left, right)\n" +
            "• DirectionalDamageIndicator component\n" +
            "• All necessary references and connections",
            MessageType.Info
        );

        EditorGUILayout.Space();
        GUILayout.Label("References", EditorStyles.boldLabel);

        // Canvas field
        Canvas previousCanvas = targetCanvas;
        targetCanvas = (Canvas)EditorGUILayout.ObjectField("Target Canvas", targetCanvas, typeof(Canvas), true);

        if (targetCanvas != previousCanvas && targetCanvas != null)
        {
            EditorUtility.SetDirty(this);
        }

        // Player Health System field
        PlayerHealthSystem previousHealthSystem = playerHealthSystem;
        playerHealthSystem = (PlayerHealthSystem)EditorGUILayout.ObjectField(
            "Player Health System",
            playerHealthSystem,
            typeof(PlayerHealthSystem),
            true
        );

        if (playerHealthSystem != previousHealthSystem && playerHealthSystem != null)
        {
            EditorUtility.SetDirty(this);
        }

        // Blood splatter sprite field
        bloodSplatterSprite = (Sprite)EditorGUILayout.ObjectField(
            "Blood Splatter Sprite",
            bloodSplatterSprite,
            typeof(Sprite),
            false
        );

        EditorGUILayout.Space();

        // Auto-link toggle
        autoLinkToHealthSystem = EditorGUILayout.Toggle(
            "Auto-Link to Health System",
            autoLinkToHealthSystem
        );

        EditorGUILayout.Space();
        GUILayout.Label("UI Size Settings", EditorStyles.boldLabel);

        splatterWidth = EditorGUILayout.Slider("Splatter Width", splatterWidth, 400f, 2000f);
        splatterHeight = EditorGUILayout.Slider("Splatter Height", splatterHeight, 300f, 1000f);
        edgeOffset = EditorGUILayout.Slider("Edge Offset", edgeOffset, 0f, 500f);

        EditorGUILayout.Space();

        // Refresh button
        if (GUILayout.Button("Refresh References"))
        {
            RefreshReferences();
        }

        EditorGUILayout.Space();

        // Validation
        bool canSetup = true;
        string validationMessage = "";

        if (targetCanvas == null)
        {
            EditorGUILayout.HelpBox("No Canvas found! Please create a Canvas or assign one.", MessageType.Warning);
            canSetup = false;
            validationMessage = "No Canvas assigned";
        }

        if (bloodSplatterSprite == null)
        {
            EditorGUILayout.HelpBox(
                "No blood splatter sprite assigned.\n\n" +
                "The tool will create placeholder images (white squares) that you can replace later.",
                MessageType.Info
            );
        }

        if (playerHealthSystem == null && autoLinkToHealthSystem)
        {
            EditorGUILayout.HelpBox(
                "No PlayerHealthSystem found.\n\n" +
                "The indicator will still be created, but you'll need to manually link it to PlayerHealthSystem later.",
                MessageType.Warning
            );
        }

        // Check if already exists
        DirectionalDamageIndicator existing = Object.FindObjectOfType<DirectionalDamageIndicator>();
        if (existing != null)
        {
            EditorGUILayout.HelpBox(
                $"DirectionalDamageIndicator already exists on '{existing.gameObject.name}'.\n\n" +
                "Setting up again will create a new one (the old one won't be deleted).",
                MessageType.Info
            );
        }

        EditorGUILayout.Space();

        // Setup button
        GUI.enabled = canSetup;
        if (GUILayout.Button("Create Directional Damage Indicator", GUILayout.Height(40)))
        {
            PerformSetup(targetCanvas, bloodSplatterSprite, playerHealthSystem);
        }
        GUI.enabled = true;

        if (!canSetup && !string.IsNullOrEmpty(validationMessage))
        {
            EditorGUILayout.HelpBox($"Cannot setup: {validationMessage}", MessageType.Error);
        }

        EditorGUILayout.Space();

        EditorGUILayout.EndScrollView();
    }

    private void RefreshReferences()
    {
        // Find canvas
        if (targetCanvas == null)
        {
            targetCanvas = Object.FindObjectOfType<Canvas>();
        }

        // Find player health system
        if (playerHealthSystem == null)
        {
            playerHealthSystem = Object.FindObjectOfType<PlayerHealthSystem>();
        }

        // Try to find blood splatter sprite if not assigned
        if (bloodSplatterSprite == null)
        {
            string[] searchPaths = new string[]
            {
                "Assets/Textures/UI/DamageIndicators",
                "Assets/Textures/UI",
                "Assets/Sprites/UI",
                "Assets/UI"
            };

            foreach (string path in searchPaths)
            {
                string[] guids = AssetDatabase.FindAssets("blood t:Sprite", new[] { path });
                if (guids.Length > 0)
                {
                    string spritePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    bloodSplatterSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                    if (bloodSplatterSprite != null)
                    {
                        Debug.Log($"[DamageIndicatorSetup] Found blood splatter sprite at: {spritePath}");
                        break;
                    }
                }
            }
        }

        Repaint();
    }

    private static void PerformQuickSetup()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(currentScene.name))
        {
            EditorUtility.DisplayDialog("Error", "No scene is currently loaded!", "OK");
            return;
        }

        // Find canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            bool createCanvas = EditorUtility.DisplayDialog(
                "No Canvas Found",
                "No Canvas found in the scene. Would you like to create one?",
                "Yes, Create Canvas",
                "Cancel"
            );

            if (createCanvas)
            {
                canvas = CreateCanvas();
            }
            else
            {
                return;
            }
        }

        // Find player health system
        PlayerHealthSystem healthSystem = Object.FindObjectOfType<PlayerHealthSystem>();

        // Find blood splatter sprite (optional)
        Sprite bloodSprite = null;
        string[] guids = AssetDatabase.FindAssets("blood t:Sprite");
        if (guids.Length > 0)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(guids[0]);
            bloodSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }

        PerformSetup(canvas, bloodSprite, healthSystem);
    }

    private static void PerformSetup(Canvas canvas, Sprite bloodSprite, PlayerHealthSystem healthSystem)
    {
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Canvas is required for setup!", "OK");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();

        // Check if already exists
        DirectionalDamageIndicator existing = Object.FindObjectOfType<DirectionalDamageIndicator>();
        if (existing != null)
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Already Exists",
                $"DirectionalDamageIndicator already exists on '{existing.gameObject.name}'.\n\n" +
                "Do you want to create another one?",
                "Yes",
                "Cancel"
            );

            if (!proceed)
                return;
        }

        Debug.Log("[DamageIndicatorSetup] Starting setup...");

        // Create the main container
        GameObject containerGO = CreateContainer(canvas);

        // Create the 4 blood splatter images
        Image topSplatter = CreateBloodSplatter(containerGO.transform, "Blood Splatter - Top", bloodSprite,
            TextAnchor.UpperCenter, new Vector2(0, -200), 180f);

        Image bottomSplatter = CreateBloodSplatter(containerGO.transform, "Blood Splatter - Bottom", bloodSprite,
            TextAnchor.LowerCenter, new Vector2(0, 200), 0f);

        Image leftSplatter = CreateBloodSplatter(containerGO.transform, "Blood Splatter - Left", bloodSprite,
            TextAnchor.MiddleLeft, new Vector2(200, 0), 90f);

        Image rightSplatter = CreateBloodSplatter(containerGO.transform, "Blood Splatter - Right", bloodSprite,
            TextAnchor.MiddleRight, new Vector2(-200, 0), -90f);

        // Add the DirectionalDamageIndicator component
        DirectionalDamageIndicator indicator = containerGO.AddComponent<DirectionalDamageIndicator>();

        // Configure the component
        ConfigureIndicator(indicator, topSplatter, bottomSplatter, leftSplatter, rightSplatter, healthSystem);

        // Link to PlayerHealthSystem if found
        if (healthSystem != null)
        {
            SerializedObject healthSystemSO = new SerializedObject(healthSystem);
            SerializedProperty damageIndicatorProp = healthSystemSO.FindProperty("damageIndicator");

            if (damageIndicatorProp != null)
            {
                damageIndicatorProp.objectReferenceValue = indicator;
                healthSystemSO.ApplyModifiedProperties();
                Debug.Log("[DamageIndicatorSetup] Linked to PlayerHealthSystem");
            }
        }

        // Mark scene as dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);

        // Select the created object
        Selection.activeGameObject = containerGO;
        EditorGUIUtility.PingObject(containerGO);

        // Success message
        string successMessage = "Directional Damage Indicator has been set up successfully!\n\n" +
            "Created:\n" +
            "• Directional Damage Indicator container\n" +
            "• 4 blood splatter UI images (top, bottom, left, right)\n" +
            "• DirectionalDamageIndicator component\n\n";

        if (bloodSprite == null)
        {
            successMessage += "⚠ No blood splatter sprite was assigned - using placeholder images.\n" +
                "Assign a sprite to each Image component in the Inspector.\n\n";
        }

        if (healthSystem != null)
        {
            successMessage += "✓ Automatically linked to PlayerHealthSystem\n\n";
        }
        else
        {
            successMessage += "⚠ No PlayerHealthSystem found.\n" +
                "Please assign the Damage Indicator field manually in PlayerHealthSystem.\n\n";
        }

        successMessage += "Don't forget to save your scene!";

        Debug.Log("[DamageIndicatorSetup] Setup complete!");
        EditorUtility.DisplayDialog("Success!", successMessage, "OK");
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        Debug.Log("[DamageIndicatorSetup] Created new Canvas");
        return canvas;
    }

    private static GameObject CreateContainer(Canvas canvas)
    {
        GameObject containerGO = new GameObject("Directional Damage Indicator");
        containerGO.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = containerGO.AddComponent<RectTransform>();

        // Stretch to fill entire canvas
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;

        Debug.Log("[DamageIndicatorSetup] Created container GameObject");
        return containerGO;
    }

    private static Image CreateBloodSplatter(Transform parent, string name, Sprite sprite,
        TextAnchor anchor, Vector2 position, float rotation)
    {
        GameObject splatterGO = new GameObject(name);
        splatterGO.transform.SetParent(parent, false);

        Image image = splatterGO.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;

        // Set color with alpha 0 (invisible by default)
        Color color = Color.white;
        color.a = 0f;
        image.color = color;

        RectTransform rectTransform = splatterGO.GetComponent<RectTransform>();

        // Set anchor based on position
        switch (anchor)
        {
            case TextAnchor.UpperCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
                break;
            case TextAnchor.LowerCenter:
                rectTransform.anchorMin = new Vector2(0.5f, 0f);
                rectTransform.anchorMax = new Vector2(0.5f, 0f);
                rectTransform.pivot = new Vector2(0.5f, 0f);
                break;
            case TextAnchor.MiddleLeft:
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(0f, 0.5f);
                rectTransform.pivot = new Vector2(0f, 0.5f);
                break;
            case TextAnchor.MiddleRight:
                rectTransform.anchorMin = new Vector2(1f, 0.5f);
                rectTransform.anchorMax = new Vector2(1f, 0.5f);
                rectTransform.pivot = new Vector2(1f, 0.5f);
                break;
        }

        rectTransform.sizeDelta = new Vector2(1000f, 600f);
        rectTransform.anchoredPosition = position;
        rectTransform.localRotation = Quaternion.Euler(0, 0, rotation);

        Debug.Log($"[DamageIndicatorSetup] Created {name}");
        return image;
    }

    private static void ConfigureIndicator(DirectionalDamageIndicator indicator,
        Image top, Image bottom, Image left, Image right, PlayerHealthSystem healthSystem)
    {
        SerializedObject indicatorSO = new SerializedObject(indicator);

        // Assign image references
        indicatorSO.FindProperty("topSplatter").objectReferenceValue = top;
        indicatorSO.FindProperty("bottomSplatter").objectReferenceValue = bottom;
        indicatorSO.FindProperty("leftSplatter").objectReferenceValue = left;
        indicatorSO.FindProperty("rightSplatter").objectReferenceValue = right;

        // Set default values
        indicatorSO.FindProperty("fadeOutDuration").floatValue = 2.5f;
        indicatorSO.FindProperty("minDamageAlpha").floatValue = 0.5f;
        indicatorSO.FindProperty("maxDamageAlpha").floatValue = 0.9f;
        indicatorSO.FindProperty("highDamageScale").floatValue = 1.2f;

        // Find and assign camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = Object.FindObjectOfType<Camera>();
        }
        indicatorSO.FindProperty("playerCamera").objectReferenceValue = mainCamera;

        // Find and assign player transform
        Transform playerTransform = null;
        if (healthSystem != null)
        {
            playerTransform = healthSystem.transform;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        indicatorSO.FindProperty("playerTransform").objectReferenceValue = playerTransform;

        indicatorSO.ApplyModifiedProperties();

        Debug.Log("[DamageIndicatorSetup] Configured DirectionalDamageIndicator component");
    }
}

using UnityEngine;
using UnityEditor;
using HealthSystem;

/// <summary>
/// Custom inspector for DirectionalDamageIndicator component
/// Adds test buttons and better organization to the Inspector
/// </summary>
[CustomEditor(typeof(DirectionalDamageIndicator))]
public class DirectionalDamageIndicatorEditor : Editor
{
    private SerializedProperty topSplatter;
    private SerializedProperty bottomSplatter;
    private SerializedProperty leftSplatter;
    private SerializedProperty rightSplatter;

    private SerializedProperty fadeOutDuration;
    private SerializedProperty minDamageAlpha;
    private SerializedProperty maxDamageAlpha;
    private SerializedProperty highDamageScale;

    private SerializedProperty playerCamera;
    private SerializedProperty playerTransform;

    private bool showImageReferences = true;
    private bool showSettings = true;
    private bool showReferences = true;
    private bool showTestButtons = true;

    private void OnEnable()
    {
        // Find serialized properties
        topSplatter = serializedObject.FindProperty("topSplatter");
        bottomSplatter = serializedObject.FindProperty("bottomSplatter");
        leftSplatter = serializedObject.FindProperty("leftSplatter");
        rightSplatter = serializedObject.FindProperty("rightSplatter");

        fadeOutDuration = serializedObject.FindProperty("fadeOutDuration");
        minDamageAlpha = serializedObject.FindProperty("minDamageAlpha");
        maxDamageAlpha = serializedObject.FindProperty("maxDamageAlpha");
        highDamageScale = serializedObject.FindProperty("highDamageScale");

        playerCamera = serializedObject.FindProperty("playerCamera");
        playerTransform = serializedObject.FindProperty("playerTransform");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DirectionalDamageIndicator indicator = (DirectionalDamageIndicator)target;

        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Directional Damage Indicator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Displays blood splatter effects on screen edges to show damage direction.\n" +
            "Connect 4 UI Images for top, bottom, left, and right directions.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        // Blood Splatter Images Section
        showImageReferences = EditorGUILayout.Foldout(showImageReferences, "Blood Splatter Images", true);
        if (showImageReferences)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(topSplatter, new GUIContent("Top Splatter"));
            EditorGUILayout.PropertyField(bottomSplatter, new GUIContent("Bottom Splatter"));
            EditorGUILayout.PropertyField(leftSplatter, new GUIContent("Left Splatter"));
            EditorGUILayout.PropertyField(rightSplatter, new GUIContent("Right Splatter"));

            // Validation
            int missingCount = 0;
            if (topSplatter.objectReferenceValue == null) missingCount++;
            if (bottomSplatter.objectReferenceValue == null) missingCount++;
            if (leftSplatter.objectReferenceValue == null) missingCount++;
            if (rightSplatter.objectReferenceValue == null) missingCount++;

            if (missingCount > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{missingCount} splatter image(s) not assigned. All 4 images must be assigned for the indicator to work.",
                    MessageType.Warning
                );
            }
            else
            {
                EditorGUILayout.HelpBox("All splatter images assigned!", MessageType.None);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Settings Section
        showSettings = EditorGUILayout.Foldout(showSettings, "Indicator Settings", true);
        if (showSettings)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(fadeOutDuration, new GUIContent("Fade Out Duration",
                "How long the indicator stays visible before fading (in seconds)"));

            EditorGUILayout.PropertyField(minDamageAlpha, new GUIContent("Min Damage Alpha",
                "Maximum alpha value for minimum damage (1 HP)"));

            EditorGUILayout.PropertyField(maxDamageAlpha, new GUIContent("Max Damage Alpha",
                "Maximum alpha value for high damage (2-3 HP)"));

            EditorGUILayout.PropertyField(highDamageScale, new GUIContent("High Damage Scale",
                "Scale multiplier for high damage intensity"));

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // References Section
        showReferences = EditorGUILayout.Foldout(showReferences, "References", true);
        if (showReferences)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(playerCamera, new GUIContent("Player Camera",
                "Reference to the player's camera for direction calculation"));

            EditorGUILayout.PropertyField(playerTransform, new GUIContent("Player Transform",
                "Reference to the player transform"));

            // Auto-find buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Find Camera", GUILayout.Width(100)))
            {
                Camera mainCamera = Camera.main;
                if (mainCamera == null)
                    mainCamera = Object.FindObjectOfType<Camera>();

                if (mainCamera != null)
                {
                    playerCamera.objectReferenceValue = mainCamera;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log($"[DamageIndicator] Found camera: {mainCamera.name}");
                }
                else
                {
                    EditorUtility.DisplayDialog("Not Found", "No camera found in scene!", "OK");
                }
            }

            if (GUILayout.Button("Find Player", GUILayout.Width(100)))
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform.objectReferenceValue = player.transform;
                    serializedObject.ApplyModifiedProperties();
                    Debug.Log($"[DamageIndicator] Found player: {player.name}");
                }
                else
                {
                    PlayerHealthSystem playerHealth = Object.FindObjectOfType<PlayerHealthSystem>();
                    if (playerHealth != null)
                    {
                        playerTransform.objectReferenceValue = playerHealth.transform;
                        serializedObject.ApplyModifiedProperties();
                        Debug.Log($"[DamageIndicator] Found player via PlayerHealthSystem: {playerHealth.name}");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Not Found", "No player found in scene!", "OK");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            // Warnings
            if (playerCamera.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Player Camera not assigned. It will be auto-detected at runtime.", MessageType.Warning);
            }

            if (playerTransform.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Player Transform not assigned. It will be auto-detected at runtime.", MessageType.Warning);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Test Buttons Section (only in Play Mode or with valid references)
        showTestButtons = EditorGUILayout.Foldout(showTestButtons, "Testing", true);
        if (showTestButtons)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox(
                "Use these buttons to test each direction indicator.\n" +
                "Testing works in both Edit Mode and Play Mode.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            // First row: Top and Bottom
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = Application.isPlaying || playerTransform.objectReferenceValue != null;

            if (GUILayout.Button("Test Top (Front)", GUILayout.Height(30)))
            {
                TestDirection(indicator, Vector3.forward, 1);
            }

            if (GUILayout.Button("Test Bottom (Back)", GUILayout.Height(30)))
            {
                TestDirection(indicator, Vector3.back, 1);
            }

            EditorGUILayout.EndHorizontal();

            // Second row: Left and Right
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Test Left", GUILayout.Height(30)))
            {
                TestDirection(indicator, Vector3.left, 1);
            }

            if (GUILayout.Button("Test Right", GUILayout.Height(30)))
            {
                TestDirection(indicator, Vector3.right, 1);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Damage amount buttons
            EditorGUILayout.LabelField("Test Different Damage Amounts:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("1 HP Damage (Front)"))
            {
                TestDirection(indicator, Vector3.forward, 1);
            }

            if (GUILayout.Button("2 HP Damage (Front)"))
            {
                TestDirection(indicator, Vector3.forward, 2);
            }

            if (GUILayout.Button("3 HP Damage (Front)"))
            {
                TestDirection(indicator, Vector3.forward, 3);
            }

            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            if (!Application.isPlaying && playerTransform.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "Player Transform must be assigned to test in Edit Mode.",
                    MessageType.Warning
                );
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Link to PlayerHealthSystem
        EditorGUILayout.LabelField("Integration", EditorStyles.boldLabel);

        PlayerHealthSystem healthSystem = Object.FindObjectOfType<PlayerHealthSystem>();
        if (healthSystem != null)
        {
            SerializedObject healthSystemSO = new SerializedObject(healthSystem);
            SerializedProperty damageIndicatorProp = healthSystemSO.FindProperty("damageIndicator");

            if (damageIndicatorProp != null && damageIndicatorProp.objectReferenceValue == indicator)
            {
                EditorGUILayout.HelpBox(
                    $"✓ Linked to PlayerHealthSystem on '{healthSystem.gameObject.name}'",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Not linked to PlayerHealthSystem. Click below to link automatically.",
                    MessageType.Warning
                );

                if (GUILayout.Button("Link to PlayerHealthSystem"))
                {
                    damageIndicatorProp.objectReferenceValue = indicator;
                    healthSystemSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(healthSystem);
                    Debug.Log("[DamageIndicator] Linked to PlayerHealthSystem");
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox(
                "No PlayerHealthSystem found in scene.\n" +
                "The indicator will not work until PlayerHealthSystem calls it.",
                MessageType.Warning
            );
        }

        EditorGUILayout.Space();

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }

    private void TestDirection(DirectionalDamageIndicator indicator, Vector3 direction, int damage)
    {
        Transform playerTrans = playerTransform.objectReferenceValue as Transform;

        if (playerTrans == null)
        {
            if (Application.isPlaying)
            {
                // Try to get it from the component itself
                playerTrans = indicator.GetType()
                    .GetField("playerTransform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(indicator) as Transform;
            }

            if (playerTrans == null)
            {
                EditorUtility.DisplayDialog("Error", "Player Transform not found!", "OK");
                return;
            }
        }

        // Calculate enemy position based on direction
        Vector3 enemyPosition = playerTrans.position + playerTrans.TransformDirection(direction) * 5f;

        // Show the indicator
        indicator.ShowDamageIndicator(enemyPosition, damage);

        Debug.Log($"[DamageIndicator] Testing {direction} direction with {damage} damage");
    }
}

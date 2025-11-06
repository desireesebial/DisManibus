using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Unity Editor tool to automatically set enemy attack cooldowns based on floor number
/// Opens via: Tools > Set Enemy Cooldowns by Floor
///
/// Cooldown timings:
/// - 4th Floor: 5.0 seconds
/// - 3rd Floor: 4.5 seconds
/// - 2nd Floor: 4.0 seconds
/// - 1st Floor: 3.0 seconds
/// </summary>
public class SetEnemyCooldownsByFloor : EditorWindow
{
    private Vector2 scrollPosition;
    private string log = "";
    private bool isProcessing = false;

    // Cooldown settings per floor
    private const float FLOOR_4_COOLDOWN = 5.0f;
    private const float FLOOR_3_COOLDOWN = 4.5f;
    private const float FLOOR_2_COOLDOWN = 4.0f;
    private const float FLOOR_1_COOLDOWN = 3.0f;

    [MenuItem("Tools/Set Enemy Cooldowns by Floor")]
    public static void ShowWindow()
    {
        var window = GetWindow<SetEnemyCooldownsByFloor>("Enemy Cooldowns Setup");
        window.minSize = new Vector2(500, 400);
    }

    private void OnGUI()
    {
        GUILayout.Label("Enemy Attack Cooldown Setup by Floor", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool will automatically set enemy attack cooldowns based on floor:\n\n" +
            "• 4th Floor scenes: 5.0 seconds\n" +
            "• 3rd Floor scenes: 4.5 seconds\n" +
            "• 2nd Floor scenes: 4.0 seconds\n" +
            "• 1st Floor scenes: 3.0 seconds\n\n" +
            "Affects these components:\n" +
            "- EnemyDamageController\n" +
            "- KuchisakeOnnaController\n" +
            "- DullahanMeleeAttack\n" +
            "- JenglotAI",
            MessageType.Info);

        GUILayout.Space(10);

        GUI.enabled = !isProcessing;
        if (GUILayout.Button("Apply Cooldowns to All Floor Scenes", GUILayout.Height(40)))
        {
            ApplyCooldownsToAllScenes();
        }
        GUI.enabled = true;

        GUILayout.Space(10);

        if (isProcessing)
        {
            EditorGUILayout.LabelField("Processing... Please wait", EditorStyles.centeredGreyMiniLabel);
        }

        GUILayout.Space(10);

        GUILayout.Label("Log:", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        EditorGUILayout.TextArea(log, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void ApplyCooldownsToAllScenes()
    {
        isProcessing = true;
        log = "=== Starting Enemy Cooldown Setup ===\n";
        Repaint();

        try
        {
            // Find all scene files
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            List<string> scenePaths = new List<string>();

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // Only process floor scenes (case-insensitive)
                if (path.ToLower().Contains("floor"))
                {
                    scenePaths.Add(path);
                }
            }

            Log($"Found {scenePaths.Count} floor scenes to process");

            // Save current scene if it's dirty
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                if (EditorUtility.DisplayDialog("Save Current Scene?",
                    "Current scene has unsaved changes. Save before continuing?",
                    "Save", "Don't Save"))
                {
                    EditorSceneManager.SaveOpenScenes();
                }
            }

            int totalEnemiesUpdated = 0;

            // Process each scene
            foreach (string scenePath in scenePaths)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                float cooldown = GetCooldownForScene(sceneName);

                if (cooldown <= 0)
                {
                    Log($"\n--- Skipping: {sceneName} (not a floor scene) ---");
                    continue;
                }

                Log($"\n--- Processing: {sceneName} (Cooldown: {cooldown}s) ---");

                // Open scene
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                int sceneEnemiesUpdated = 0;

                // Find and update EnemyDamageController components
                EnemyDamageController[] damageControllers = FindObjectsOfType<EnemyDamageController>(true);
                Log($"Found {damageControllers.Length} EnemyDamageController components");
                foreach (EnemyDamageController controller in damageControllers)
                {
                    bool updated = false;
                    if (SetCooldown(controller, "attackCooldown", cooldown))
                    {
                        updated = true;
                    }
                    // Also enable movement pause
                    if (SetBoolean(controller, "pauseMovementAfterAttack", true))
                    {
                        updated = true;
                    }
                    if (updated)
                    {
                        Log($"  ✓ Updated EnemyDamageController on: {controller.gameObject.name}");
                        sceneEnemiesUpdated++;
                    }
                }

                // Find and update KuchisakeOnnaController components
                KuchisakeOnnaController[] kuchisakeControllers = FindObjectsOfType<KuchisakeOnnaController>(true);
                Log($"Found {kuchisakeControllers.Length} KuchisakeOnnaController components");
                foreach (KuchisakeOnnaController controller in kuchisakeControllers)
                {
                    if (SetCooldown(controller, "attackCooldown", cooldown))
                    {
                        Log($"  ✓ Updated KuchisakeOnnaController on: {controller.gameObject.name}");
                        sceneEnemiesUpdated++;
                    }
                }

                // Find and update DullahanMeleeAttack components
                DullahanMeleeAttack[] dullahanAttacks = FindObjectsOfType<DullahanMeleeAttack>(true);
                Log($"Found {dullahanAttacks.Length} DullahanMeleeAttack components");
                foreach (DullahanMeleeAttack attack in dullahanAttacks)
                {
                    if (SetCooldown(attack, "attackCooldown", cooldown))
                    {
                        Log($"  ✓ Updated DullahanMeleeAttack on: {attack.gameObject.name}");
                        sceneEnemiesUpdated++;
                    }
                }

                // Find and update JenglotAI components
                JenglotAI[] jenglotAIs = FindObjectsOfType<JenglotAI>(true);
                Log($"Found {jenglotAIs.Length} JenglotAI components");
                foreach (JenglotAI ai in jenglotAIs)
                {
                    if (SetCooldown(ai, "attackCooldown", cooldown))
                    {
                        Log($"  ✓ Updated JenglotAI on: {ai.gameObject.name}");
                        sceneEnemiesUpdated++;
                    }
                }

                totalEnemiesUpdated += sceneEnemiesUpdated;

                // Mark scene as dirty and save if changes were made
                if (sceneEnemiesUpdated > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Log($"✓ Saved scene: {sceneName} ({sceneEnemiesUpdated} enemies updated)");
                }
                else
                {
                    Log($"No changes needed for: {sceneName}");
                }
            }

            Log($"\n=== Setup Complete! ===");
            Log($"Total enemies updated: {totalEnemiesUpdated}");
            Log($"\nAll scenes have been saved with updated cooldowns.");

            EditorUtility.DisplayDialog("Enemy Cooldown Setup Complete",
                $"Successfully updated {totalEnemiesUpdated} enemy components across all floor scenes!\n\n" +
                $"Cooldown settings:\n" +
                $"• 4th Floor: 5.0s\n" +
                $"• 3rd Floor: 4.5s\n" +
                $"• 2nd Floor: 4.0s\n" +
                $"• 1st Floor: 3.0s",
                "OK");
        }
        catch (System.Exception e)
        {
            Log($"\n❌ ERROR: {e.Message}");
            Debug.LogError($"[SetEnemyCooldownsByFloor] Error: {e}");
            EditorUtility.DisplayDialog("Error", $"An error occurred:\n{e.Message}", "OK");
        }
        finally
        {
            isProcessing = false;
            Repaint();
        }
    }

    private float GetCooldownForScene(string sceneName)
    {
        // Check which floor this scene belongs to (case-insensitive)
        string lowerSceneName = sceneName.ToLower();

        if (lowerSceneName.Contains("4th floor"))
            return FLOOR_4_COOLDOWN;
        else if (lowerSceneName.Contains("3rd floor"))
            return FLOOR_3_COOLDOWN;
        else if (lowerSceneName.Contains("2nd floor"))
            return FLOOR_2_COOLDOWN;
        else if (lowerSceneName.Contains("1st floor"))
            return FLOOR_1_COOLDOWN;
        else
            return -1f; // Not a floor scene
    }

    private bool SetCooldown(MonoBehaviour component, string fieldName, float cooldownValue)
    {
        // Use SerializedObject to properly set the field value
        SerializedObject so = new SerializedObject(component);
        SerializedProperty cooldownProperty = so.FindProperty(fieldName);

        if (cooldownProperty != null)
        {
            // Check if value is already correct
            if (Mathf.Approximately(cooldownProperty.floatValue, cooldownValue))
            {
                return false; // No change needed
            }

            cooldownProperty.floatValue = cooldownValue;
            so.ApplyModifiedProperties();

            // Mark the component as dirty so Unity saves the change
            EditorUtility.SetDirty(component);
            return true;
        }
        else
        {
            Debug.LogWarning($"Could not find {fieldName} property on {component.GetType().Name} ({component.gameObject.name})");
            return false;
        }
    }

    private bool SetBoolean(MonoBehaviour component, string fieldName, bool boolValue)
    {
        // Use SerializedObject to properly set the boolean field value
        SerializedObject so = new SerializedObject(component);
        SerializedProperty boolProperty = so.FindProperty(fieldName);

        if (boolProperty != null)
        {
            // Check if value is already correct
            if (boolProperty.boolValue == boolValue)
            {
                return false; // No change needed
            }

            boolProperty.boolValue = boolValue;
            so.ApplyModifiedProperties();

            // Mark the component as dirty so Unity saves the change
            EditorUtility.SetDirty(component);
            return true;
        }
        else
        {
            // Field might not exist in all components (that's okay)
            return false;
        }
    }

    private void Log(string message)
    {
        log += message + "\n";
        Repaint();
        Debug.Log($"[SetEnemyCooldownsByFloor] {message}");
    }
}

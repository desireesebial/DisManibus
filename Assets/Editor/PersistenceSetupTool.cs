using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using SaveLoad;

/// <summary>
/// Unity Editor tool to automatically setup persistence for all interactive objects
/// Opens via: Tools > Setup Scene Persistence
/// </summary>
public class PersistenceSetupTool : EditorWindow
{
    private Vector2 scrollPosition;
    private string log = "";
    private bool isProcessing = false;

    [MenuItem("Tools/Setup Scene Persistence")]
    public static void ShowWindow()
    {
        var window = GetWindow<PersistenceSetupTool>("Persistence Setup");
        window.minSize = new Vector2(500, 400);
    }

    private void OnGUI()
    {
        GUILayout.Label("Automated Scene Persistence Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool will automatically add persistence components to:\n" +
            "• All doors (doorscript)\n" +
            "• All collectible items (ItemsPickable)\n" +
            "• All puzzles (SequentialTorchManager, HeadShrinePuzzle)\n\n" +
            "Unique IDs will be auto-generated based on scene name and object hierarchy path.",
            MessageType.Info);

        GUILayout.Space(10);

        GUI.enabled = !isProcessing;
        if (GUILayout.Button("Setup Persistence for All Floor Scenes", GUILayout.Height(40)))
        {
            SetupAllScenes();
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

    private void SetupAllScenes()
    {
        isProcessing = true;
        log = "=== Starting Persistence Setup ===\n";
        Repaint();

        try
        {
            // Find all floor scene files
            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene Floor", new[] { "Assets/Scenes" });
            List<string> scenePaths = new List<string>();

            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Floor"))
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

            int totalDoors = 0;
            int totalItems = 0;
            int totalPuzzles = 0;

            // Process each scene
            foreach (string scenePath in scenePaths)
            {
                Log($"\n--- Processing: {System.IO.Path.GetFileNameWithoutExtension(scenePath)} ---");

                // Open scene
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                string sceneName = scene.name;

                // Find and setup doors
                doorscript[] doors = FindObjectsOfType<doorscript>(true);
                Log($"Found {doors.Length} doors");
                foreach (doorscript door in doors)
                {
                    if (SetupDoorPersistence(door, sceneName))
                    {
                        totalDoors++;
                    }
                }

                // Find and setup items
                ItemsPickable[] items = FindObjectsOfType<ItemsPickable>(true);
                Log($"Found {items.Length} collectible items");
                foreach (ItemsPickable item in items)
                {
                    if (SetupItemPersistence(item, sceneName))
                    {
                        totalItems++;
                    }
                }

                // Find and setup torch puzzles
                SequentialTorchManager[] torchPuzzles = FindObjectsOfType<SequentialTorchManager>(true);
                Log($"Found {torchPuzzles.Length} torch puzzles");
                foreach (SequentialTorchManager puzzle in torchPuzzles)
                {
                    if (SetupTorchPuzzlePersistence(puzzle, sceneName))
                    {
                        totalPuzzles++;
                    }
                }

                // Find and setup head shrine puzzles
                HeadShrinePuzzle[] headPuzzles = FindObjectsOfType<HeadShrinePuzzle>(true);
                Log($"Found {headPuzzles.Length} head shrine puzzles");
                foreach (HeadShrinePuzzle puzzle in headPuzzles)
                {
                    if (SetupHeadShrinePuzzlePersistence(puzzle, sceneName))
                    {
                        totalPuzzles++;
                    }
                }

                // Mark scene as dirty and save
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Log($"✓ Saved scene: {sceneName}");
            }

            Log($"\n=== Setup Complete! ===");
            Log($"Total doors configured: {totalDoors}");
            Log($"Total items configured: {totalItems}");
            Log($"Total puzzles configured: {totalPuzzles}");
            Log($"\nAll scenes have been saved with persistence components added.");

            EditorUtility.DisplayDialog("Persistence Setup Complete",
                $"Successfully configured:\n" +
                $"• {totalDoors} doors\n" +
                $"• {totalItems} items\n" +
                $"• {totalPuzzles} puzzles\n\n" +
                $"All scenes have been saved.",
                "OK");
        }
        catch (System.Exception e)
        {
            Log($"\n❌ ERROR: {e.Message}");
            Debug.LogError($"[PersistenceSetupTool] Error: {e}");
            EditorUtility.DisplayDialog("Error", $"An error occurred:\n{e.Message}", "OK");
        }
        finally
        {
            isProcessing = false;
            Repaint();
        }
    }

    private bool SetupDoorPersistence(doorscript door, string sceneName)
    {
        // Check if already has persistence component
        PersistentDoorBridge existingBridge = door.GetComponent<PersistentDoorBridge>();
        if (existingBridge != null)
        {
            // Already has persistence, but check if ID is set
            if (string.IsNullOrEmpty(existingBridge.PersistentId))
            {
                string id = GenerateUniqueId(sceneName, "Door", door.gameObject);
                SetPersistentId(existingBridge, id);
                Log($"  ✓ Updated ID for door: {door.name} → {id}");
                return true;
            }
            return false; // Already configured
        }

        // Add persistence component
        PersistentDoorBridge bridge = door.gameObject.AddComponent<PersistentDoorBridge>();

        // Generate and set unique ID
        string uniqueId = GenerateUniqueId(sceneName, "Door", door.gameObject);
        SetPersistentId(bridge, uniqueId);

        Log($"  ✓ Added persistence to door: {door.name} → {uniqueId}");
        return true;
    }

    private bool SetupItemPersistence(ItemsPickable item, string sceneName)
    {
        // Check if already has persistence component
        PersistentItem existingItem = item.GetComponent<PersistentItem>();
        if (existingItem != null)
        {
            // Already has persistence, but check if ID is set
            if (string.IsNullOrEmpty(existingItem.PersistentId))
            {
                string id = GenerateUniqueId(sceneName, "Item", item.gameObject);
                SetPersistentId(existingItem, id);
                Log($"  ✓ Updated ID for item: {item.name} → {id}");
                return true;
            }
            return false; // Already configured
        }

        // Add persistence component
        PersistentItem persistentItem = item.gameObject.AddComponent<PersistentItem>();

        // Generate and set unique ID
        string uniqueId = GenerateUniqueId(sceneName, "Item", item.gameObject);
        SetPersistentId(persistentItem, uniqueId);

        Log($"  ✓ Added persistence to item: {item.name} → {uniqueId}");
        return true;
    }

    private bool SetupTorchPuzzlePersistence(SequentialTorchManager puzzle, string sceneName)
    {
        // Check if already has persistence component
        PersistentSequentialTorchPuzzle existingPuzzle = puzzle.GetComponent<PersistentSequentialTorchPuzzle>();
        if (existingPuzzle != null)
        {
            if (string.IsNullOrEmpty(existingPuzzle.PersistentId))
            {
                string id = GenerateUniqueId(sceneName, "TorchPuzzle", puzzle.gameObject);
                SetPersistentId(existingPuzzle, id);
                Log($"  ✓ Updated ID for torch puzzle: {puzzle.name} → {id}");
                return true;
            }
            return false;
        }

        // Add persistence component
        PersistentSequentialTorchPuzzle persistentPuzzle = puzzle.gameObject.AddComponent<PersistentSequentialTorchPuzzle>();

        // Generate and set unique ID
        string uniqueId = GenerateUniqueId(sceneName, "TorchPuzzle", puzzle.gameObject);
        SetPersistentId(persistentPuzzle, uniqueId);

        Log($"  ✓ Added persistence to torch puzzle: {puzzle.name} → {uniqueId}");
        return true;
    }

    private bool SetupHeadShrinePuzzlePersistence(HeadShrinePuzzle puzzle, string sceneName)
    {
        // Check if already has persistence component
        PersistentHeadShrinePuzzle existingPuzzle = puzzle.GetComponent<PersistentHeadShrinePuzzle>();
        if (existingPuzzle != null)
        {
            if (string.IsNullOrEmpty(existingPuzzle.PersistentId))
            {
                string id = GenerateUniqueId(sceneName, "HeadShrinePuzzle", puzzle.gameObject);
                SetPersistentId(existingPuzzle, id);
                Log($"  ✓ Updated ID for head shrine puzzle: {puzzle.name} → {id}");
                return true;
            }
            return false;
        }

        // Add persistence component
        PersistentHeadShrinePuzzle persistentPuzzle = puzzle.gameObject.AddComponent<PersistentHeadShrinePuzzle>();

        // Generate and set unique ID
        string uniqueId = GenerateUniqueId(sceneName, "HeadShrinePuzzle", puzzle.gameObject);
        SetPersistentId(persistentPuzzle, uniqueId);

        Log($"  ✓ Added persistence to head shrine puzzle: {puzzle.name} → {uniqueId}");
        return true;
    }

    private string GenerateUniqueId(string sceneName, string type, GameObject obj)
    {
        // Clean scene name (remove special characters and spaces)
        string cleanSceneName = sceneName.Replace(" ", "").Replace("(", "").Replace(")", "");

        // Get hierarchy path for uniqueness
        string path = GetGameObjectPath(obj);
        string cleanPath = path.Replace("/", "_").Replace(" ", "");

        // Generate ID: SceneName_Type_ObjectName_PathHash
        int pathHash = path.GetHashCode();
        string id = $"{cleanSceneName}_{type}_{obj.name}_{Mathf.Abs(pathHash)}";

        // Ensure ID is not too long (max 100 characters)
        if (id.Length > 100)
        {
            id = id.Substring(0, 100);
        }

        return id;
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

    private void SetPersistentId(PersistentEntity entity, string id)
    {
        // Use Unity's SerializedObject to properly set the field
        SerializedObject so = new SerializedObject(entity);
        SerializedProperty idProperty = so.FindProperty("persistentId");

        if (idProperty != null)
        {
            idProperty.stringValue = id;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning($"Could not find persistentId property on {entity.name}");
        }

        // Mark the component as dirty so Unity saves the change
        EditorUtility.SetDirty(entity);
    }

    private void Log(string message)
    {
        log += message + "\n";
        Repaint();
        Debug.Log($"[PersistenceSetupTool] {message}");
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using DisManibus.World.Lighting;
using System.Collections.Generic;

namespace DisManibus.Editor
{
    /// <summary>
    /// Editor tool to add BlinkLight component to all Point Lights across all scenes.
    /// Excludes puzzle lights (SequentialTorch lights on 1st floor).
    /// </summary>
    public class AddBlinkToAllLights : UnityEditor.Editor
    {
        private static readonly string[] ScenePaths = new string[]
        {
            "Assets/Scenes/1st Floor (BAD ENDING).unity",
            "Assets/Scenes/1st Floor (GOOD ENDING).unity",
            "Assets/Scenes/2nd Floor (Better Version).unity",
            "Assets/Scenes/3rd floor (better version).unity",
            "Assets/Scenes/4th Floor (better version).unity"
        };

        [MenuItem("Tools/Add Blink to All Point Lights")]
        public static void AddBlinkToPointLights()
        {
            if (!EditorUtility.DisplayDialog(
                "Add Blink to All Point Lights",
                "This will add the BlinkLight component to all Point Lights in all floor scenes.\n\n" +
                "Puzzle lights (with SequentialTorch component) will be excluded.\n\n" +
                "This operation will save all modified scenes. Continue?",
                "Yes, Add Blinks",
                "Cancel"))
            {
                return;
            }

            // Save current scene
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            int totalLightsProcessed = 0;
            int totalLightsSkipped = 0;
            int totalLightsAdded = 0;
            List<string> processedScenes = new List<string>();

            foreach (string scenePath in ScenePaths)
            {
                // Open the scene
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int sceneLightsAdded = 0;
                int sceneLightsSkipped = 0;

                // Find all Light components in the scene
                Light[] allLights = FindObjectsByType<Light>(FindObjectsSortMode.None);

                foreach (Light light in allLights)
                {
                    // Only process Point Lights
                    if (light.type != LightType.Point)
                    {
                        continue;
                    }

                    totalLightsProcessed++;

                    // Check if this light is part of a puzzle (has SequentialTorch component)
                    SequentialTorch torchComponent = light.GetComponent<SequentialTorch>();
                    if (torchComponent != null)
                    {
                        Debug.Log($"Skipping puzzle light: {light.gameObject.name} in scene {scene.name}");
                        sceneLightsSkipped++;
                        totalLightsSkipped++;
                        continue;
                    }

                    // Check if light is child of a SequentialTorch
                    torchComponent = light.GetComponentInParent<SequentialTorch>();
                    if (torchComponent != null)
                    {
                        Debug.Log($"Skipping puzzle light (child): {light.gameObject.name} in scene {scene.name}");
                        sceneLightsSkipped++;
                        totalLightsSkipped++;
                        continue;
                    }

                    // Check if BlinkLight already exists
                    BlinkLight existingBlink = light.GetComponent<BlinkLight>();
                    if (existingBlink != null)
                    {
                        Debug.Log($"BlinkLight already exists on: {light.gameObject.name} in scene {scene.name}");
                        sceneLightsSkipped++;
                        totalLightsSkipped++;
                        continue;
                    }

                    // Add BlinkLight component
                    BlinkLight blinkLight = light.gameObject.AddComponent<BlinkLight>();
                    blinkLight.targetLight = light;
                    blinkLight.minOffDuration = 1f;
                    blinkLight.maxOffDuration = 5f;
                    blinkLight.minTimeBetweenBlinks = 0.5f;
                    blinkLight.maxTimeBetweenBlinks = 3f;

                    Debug.Log($"Added BlinkLight to: {light.gameObject.name} in scene {scene.name}");
                    sceneLightsAdded++;
                    totalLightsAdded++;
                }

                // Save the scene if any changes were made
                if (sceneLightsAdded > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    processedScenes.Add($"{scene.name}: {sceneLightsAdded} lights added, {sceneLightsSkipped} skipped");
                }
                else
                {
                    processedScenes.Add($"{scene.name}: No changes (0 added, {sceneLightsSkipped} skipped)");
                }
            }

            // Show results
            string message = $"Blink lights setup complete!\n\n" +
                           $"Total lights processed: {totalLightsProcessed}\n" +
                           $"BlinkLight components added: {totalLightsAdded}\n" +
                           $"Lights skipped: {totalLightsSkipped}\n\n" +
                           $"Scene breakdown:\n" +
                           string.Join("\n", processedScenes);

            EditorUtility.DisplayDialog("Add Blink to Point Lights - Complete", message, "OK");
            Debug.Log($"AddBlinkToAllLights complete: {totalLightsAdded} components added, {totalLightsSkipped} lights skipped");
        }

        [MenuItem("Tools/Remove Blink from All Point Lights")]
        public static void RemoveBlinkFromPointLights()
        {
            if (!EditorUtility.DisplayDialog(
                "Remove Blink from All Point Lights",
                "This will remove all BlinkLight components from all floor scenes.\n\n" +
                "This operation will save all modified scenes. Continue?",
                "Yes, Remove Blinks",
                "Cancel"))
            {
                return;
            }

            // Save current scene
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            int totalComponentsRemoved = 0;
            List<string> processedScenes = new List<string>();

            foreach (string scenePath in ScenePaths)
            {
                // Open the scene
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int sceneComponentsRemoved = 0;

                // Find all BlinkLight components in the scene
                BlinkLight[] allBlinkLights = FindObjectsByType<BlinkLight>(FindObjectsSortMode.None);

                foreach (BlinkLight blinkLight in allBlinkLights)
                {
                    Debug.Log($"Removing BlinkLight from: {blinkLight.gameObject.name} in scene {scene.name}");
                    DestroyImmediate(blinkLight);
                    sceneComponentsRemoved++;
                    totalComponentsRemoved++;
                }

                // Save the scene if any changes were made
                if (sceneComponentsRemoved > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    processedScenes.Add($"{scene.name}: {sceneComponentsRemoved} removed");
                }
                else
                {
                    processedScenes.Add($"{scene.name}: No BlinkLight components found");
                }
            }

            // Show results
            string message = $"Blink lights removal complete!\n\n" +
                           $"Total BlinkLight components removed: {totalComponentsRemoved}\n\n" +
                           $"Scene breakdown:\n" +
                           string.Join("\n", processedScenes);

            EditorUtility.DisplayDialog("Remove Blink from Point Lights - Complete", message, "OK");
            Debug.Log($"RemoveBlinkFromPointLights complete: {totalComponentsRemoved} components removed");
        }
    }
}

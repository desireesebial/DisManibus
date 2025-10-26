using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AssignMaterialsToAllLODsScript : EditorWindow
{
    [MenuItem("Tools/Assign Materials to All Temple LODs")]
    public static void AssignMaterialsToAllLODs()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (!currentScene.IsValid())
        {
            EditorUtility.DisplayDialog("Error", "No active scene found!", "OK");
            return;
        }

        Debug.Log($"Assigning materials to all LOD levels in scene: {currentScene.name}");

        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        int fixedLODGroups = 0;
        int fixedRenderers = 0;

        foreach (GameObject rootObj in rootObjects)
        {
            // Find all LODGroup components
            LODGroup[] lodGroups = rootObj.GetComponentsInChildren<LODGroup>(true);

            foreach (LODGroup lodGroup in lodGroups)
            {
                LOD[] lods = lodGroup.GetLODs();

                if (lods.Length == 0)
                {
                    Debug.LogWarning($"LODGroup on {lodGroup.gameObject.name} has no LOD levels!");
                    continue;
                }

                // Get materials from LOD 0 renderers
                Dictionary<string, Material[]> lod0Materials = new Dictionary<string, Material[]>();

                foreach (Renderer renderer in lods[0].renderers)
                {
                    if (renderer != null)
                    {
                        string meshName = "";
                        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null)
                        {
                            meshName = meshFilter.sharedMesh.name;
                        }

                        // Store materials by mesh name or object name
                        string key = string.IsNullOrEmpty(meshName) ? renderer.gameObject.name : meshName;
                        lod0Materials[key] = renderer.sharedMaterials;

                        Debug.Log($"LOD 0 - {renderer.gameObject.name} (mesh: {meshName}) has {renderer.sharedMaterials.Length} materials");
                    }
                }

                // Now assign materials to LOD 1, 2, 3, etc.
                for (int lodLevel = 1; lodLevel < lods.Length; lodLevel++)
                {
                    foreach (Renderer renderer in lods[lodLevel].renderers)
                    {
                        if (renderer == null) continue;

                        // Check if renderer has no materials or all null
                        Material[] currentMats = renderer.sharedMaterials;
                        bool needsMaterials = currentMats.Length == 0 || System.Array.TrueForAll(currentMats, m => m == null);

                        if (needsMaterials)
                        {
                            // Try to find matching materials from LOD 0
                            string meshName = "";
                            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                            if (meshFilter != null && meshFilter.sharedMesh != null)
                            {
                                meshName = meshFilter.sharedMesh.name;
                            }

                            // Try to match by mesh name first
                            Material[] materialsToAssign = null;

                            // Look for exact mesh name match
                            foreach (var kvp in lod0Materials)
                            {
                                if (kvp.Key.Contains(meshName.Replace("_LOD1", "").Replace("_LOD2", "").Replace("_LOD3", "")) ||
                                    meshName.Contains(kvp.Key.Replace("_LOD0", "")))
                                {
                                    materialsToAssign = kvp.Value;
                                    Debug.Log($"LOD {lodLevel} - Matched {renderer.gameObject.name} (mesh: {meshName}) to LOD0 materials from {kvp.Key}");
                                    break;
                                }
                            }

                            // If no match, just use the first LOD0 materials as fallback
                            if (materialsToAssign == null && lod0Materials.Count > 0)
                            {
                                foreach (var kvp in lod0Materials)
                                {
                                    materialsToAssign = kvp.Value;
                                    Debug.Log($"LOD {lodLevel} - Using fallback materials for {renderer.gameObject.name}");
                                    break;
                                }
                            }

                            if (materialsToAssign != null && materialsToAssign.Length > 0)
                            {
                                renderer.sharedMaterials = materialsToAssign;
                                EditorUtility.SetDirty(renderer);
                                fixedRenderers++;
                                Debug.Log($"<color=cyan>Assigned {materialsToAssign.Length} materials to {renderer.gameObject.name} at LOD {lodLevel}</color>");
                            }
                        }
                    }
                }

                fixedLODGroups++;
            }
        }

        if (fixedRenderers > 0)
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            Debug.Log($"<color=green>Fixed {fixedRenderers} renderers across {fixedLODGroups} LOD groups!</color>");
            EditorUtility.DisplayDialog("Success!",
                $"Assigned materials to {fixedRenderers} renderers across {fixedLODGroups} LOD groups.\n\nThe pink LOD issue should now be fixed!",
                "OK");
        }
        else
        {
            Debug.LogWarning("No LOD renderers found that needed materials.");
            EditorUtility.DisplayDialog("No Changes",
                "No LOD renderers were found that needed materials assigned.",
                "OK");
        }
    }
}

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixTempleLODMaterials : EditorWindow
{
    [MenuItem("Tools/Fix Temple LOD Materials")]
    public static void FixLODMaterials()
    {
        // Map of old material GUIDs to new material names
        Dictionary<string, string> materialMapping = new Dictionary<string, string>()
        {
            { "705cc426cece971418e81d1ac6cbad7c", "wood" },
            { "b084e03f8bf432641bd39f418a6f0afc", "wood 2" },
            { "9343319050d3d55418a4af86be46bd7e", "Clay" },
            { "04d96e309631a4442be2713e129c9979", "Clay 2" },
            { "c8969ba269dd0ba4694b2a841ea26a0f", "wood 3" },
            { "cf396200624b09f42945b74a11a6bf9a", "metal" },
            { "2c3fc2cc956961f4e8b9f7916daceeb0", "wood 4" },
            { "76136738cd3d0f448a74949fcb04de56", "wood 5" },
            { "c8cbead2f6ed70d48bef46da5f2dbb8c", "thatch/thatch" },
            { "790878cb1c6268440808b019f8a91ea3", "Base" },
            { "681001c2dd8b729488649c43f2718375", "wood 6" },
            { "2c5a42bff143ee541b86748781b68566", "Clay 3" },
            { "0e74a68e451e99349a6f2833a4ea65f0", "Plaster" },
            { "918b5dcd99b86ec47b9f993f0cf60e56", "Stone" },
            { "a119d514e2af1f144a755bb7189bda6b", "thatch 2" },
            { "30f6cd63e96256743b57caa8cfe62701", "metal 2" },
            { "933532a4fcc9baf4fa0491de14d08ed7", "decals" }
        };

        string[] prefabPaths = new string[]
        {
            "Assets/Assets/1st Floor/Temples/Prefabs_LOD/Pagoda_Sanju_13m_LOD.prefab",
            "Assets/Assets/1st Floor/Temples/Prefabs_LOD/Pagoda_Tahoto_14m_LOD.prefab",
            "Assets/Assets/1st Floor/Temples/Prefabs_LOD/Pagoda_Tahoto_15m_LOD.prefab",
            "Assets/Assets/1st Floor/Temples/Prefabs_LOD/Pagoda_Tahoto_20m_LOD.prefab"
        };

        string baseMaterialPath = "Assets/Assets/1st Floor/Temples/Textures/";

        int fixedCount = 0;
        int totalRenderers = 0;

        foreach (string prefabPath in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"Could not load prefab: {prefabPath}");
                continue;
            }

            // Get all renderers in the prefab
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer renderer in renderers)
            {
                totalRenderers++;
                Material[] materials = renderer.sharedMaterials;
                bool materialsChanged = false;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null)
                    {
                        Debug.LogWarning($"Null material found in {prefabPath} on {renderer.gameObject.name}");
                        continue;
                    }

                    // Get the GUID of the current material
                    string assetPath = AssetDatabase.GetAssetPath(materials[i]);
                    string guid = AssetDatabase.AssetPathToGUID(assetPath);

                    // If we have a mapping for this GUID, replace it
                    if (materialMapping.ContainsKey(guid))
                    {
                        string newMatName = materialMapping[guid];
                        string newMatPath = baseMaterialPath + newMatName + ".mat";
                        Material newMat = AssetDatabase.LoadAssetAtPath<Material>(newMatPath);

                        if (newMat != null)
                        {
                            Debug.Log($"Replacing material {materials[i].name} (GUID: {guid}) with {newMatName} on {renderer.gameObject.name}");
                            materials[i] = newMat;
                            materialsChanged = true;
                            fixedCount++;
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find material at {newMatPath}");
                        }
                    }
                }

                if (materialsChanged)
                {
                    renderer.sharedMaterials = materials;
                    EditorUtility.SetDirty(renderer);
                }
            }

            EditorUtility.SetDirty(prefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Fixed {fixedCount} materials across {totalRenderers} renderers in temple LOD prefabs!</color>");
        EditorUtility.DisplayDialog("Fix Complete",
            $"Fixed {fixedCount} materials across {totalRenderers} renderers.\n\nThe temple LOD pink issue should now be resolved!",
            "OK");
    }
}

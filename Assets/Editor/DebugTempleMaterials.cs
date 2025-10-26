using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class DebugTempleMaterials : EditorWindow
{
    [MenuItem("Tools/Debug Temple Materials")]
    public static void DebugMaterials()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        Debug.Log("=== TEMPLE MATERIALS DEBUG ===");

        foreach (GameObject rootObj in rootObjects)
        {
            LODGroup[] lodGroups = rootObj.GetComponentsInChildren<LODGroup>(true);

            foreach (LODGroup lodGroup in lodGroups)
            {
                Debug.Log($"\n<color=yellow>LODGroup: {lodGroup.gameObject.name}</color>");
                LOD[] lods = lodGroup.GetLODs();

                for (int i = 0; i < lods.Length; i++)
                {
                    Debug.Log($"  LOD {i} ({lods[i].renderers.Length} renderers):");

                    foreach (Renderer renderer in lods[i].renderers)
                    {
                        if (renderer == null)
                        {
                            Debug.LogWarning($"    NULL RENDERER at LOD {i}");
                            continue;
                        }

                        Material[] mats = renderer.sharedMaterials;
                        string matInfo = "";

                        if (mats.Length == 0)
                        {
                            matInfo = "<color=red>NO MATERIALS!</color>";
                        }
                        else
                        {
                            for (int m = 0; m < mats.Length; m++)
                            {
                                if (mats[m] == null)
                                {
                                    matInfo += $"[{m}]=<color=red>NULL</color> ";
                                }
                                else
                                {
                                    string shaderName = mats[m].shader != null ? mats[m].shader.name : "NULL SHADER";
                                    if (shaderName.Contains("URP"))
                                    {
                                        matInfo += $"[{m}]={mats[m].name} (<color=green>{shaderName}</color>) ";
                                    }
                                    else
                                    {
                                        matInfo += $"[{m}]={mats[m].name} (<color=red>{shaderName}</color>) ";
                                    }
                                }
                            }
                        }

                        Debug.Log($"    {renderer.gameObject.name}: {matInfo}");
                    }
                }
            }
        }

        Debug.Log("\n=== END DEBUG ===");
    }

    [MenuItem("Tools/Fix Specific Pink Meshes")]
    public static void FixSpecificPinkMeshes()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        Shader urpShader = Shader.Find("TanukiDigital/URP/Standard_Packed_PBR");
        if (urpShader == null)
        {
            Debug.LogError("URP Shader not found!");
            return;
        }

        // Load default materials
        string basePath = "Assets/Assets/1st Floor/Temples/Textures/";
        Material baseMat = AssetDatabase.LoadAssetAtPath<Material>(basePath + "Base.mat");
        Material clayMat = AssetDatabase.LoadAssetAtPath<Material>(basePath + "Clay.mat");
        Material woodMat = AssetDatabase.LoadAssetAtPath<Material>(basePath + "wood.mat");

        int fixedCount = 0;

        foreach (GameObject rootObj in rootObjects)
        {
            MeshRenderer[] renderers = rootObj.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer renderer in renderers)
            {
                string objName = renderer.gameObject.name;

                // Check if this is one of the problematic meshes
                if (objName.Contains("Roof1_LOD1") || objName.Contains("Base_LOD1") || objName.Contains("Wall1_LOD0"))
                {
                    Material[] mats = renderer.sharedMaterials;
                    bool needsFixing = false;

                    // Check if has no materials or null materials
                    if (mats.Length == 0)
                    {
                        needsFixing = true;
                    }
                    else
                    {
                        foreach (Material mat in mats)
                        {
                            if (mat == null || mat.shader != urpShader)
                            {
                                needsFixing = true;
                                break;
                            }
                        }
                    }

                    if (needsFixing)
                    {
                        Debug.Log($"<color=cyan>Fixing {objName}...</color>");

                        // Assign appropriate material based on mesh name
                        Material matToAssign = baseMat; // Default

                        if (objName.Contains("Roof"))
                            matToAssign = clayMat;
                        else if (objName.Contains("Wall"))
                            matToAssign = woodMat;
                        else if (objName.Contains("Base"))
                            matToAssign = baseMat;

                        if (matToAssign != null)
                        {
                            renderer.sharedMaterial = matToAssign;
                            EditorUtility.SetDirty(renderer);
                            fixedCount++;
                            Debug.Log($"  Assigned {matToAssign.name} to {objName}");
                        }
                    }
                }
            }
        }

        if (fixedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            Debug.Log($"<color=green>Fixed {fixedCount} pink meshes!</color>");
            EditorUtility.DisplayDialog("Success", $"Fixed {fixedCount} pink meshes!", "OK");
        }
        else
        {
            Debug.Log("No pink meshes found that needed fixing.");
        }
    }
}

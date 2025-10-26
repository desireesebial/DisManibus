using UnityEngine;
using UnityEditor;
using System.IO;

public class UpdateTempleMaterials : EditorWindow
{
    [MenuItem("Tools/Update Temple Materials to URP")]
    public static void UpdateAllTempleMaterials()
    {
        string[] materialPaths = new string[]
        {
            "Assets/Assets/1st Floor/Temples/Textures/Base.mat",
            "Assets/Assets/1st Floor/Temples/Textures/Clay.mat",
            "Assets/Assets/1st Floor/Temples/Textures/Clay 2.mat",
            "Assets/Assets/1st Floor/Temples/Textures/Clay 3.mat",
            "Assets/Assets/1st Floor/Temples/Textures/Plaster.mat",
            "Assets/Assets/1st Floor/Temples/Textures/Stone.mat",
            "Assets/Assets/1st Floor/Temples/Textures/decals.mat",
            "Assets/Assets/1st Floor/Temples/Textures/metal.mat",
            "Assets/Assets/1st Floor/Temples/Textures/metal 2.mat",
            "Assets/Assets/1st Floor/Temples/Textures/thatch 2.mat",
            "Assets/Assets/1st Floor/Temples/Textures/wood.mat",
            "Assets/Assets/1st Floor/Temples/Textures/wood 2.mat",
            "Assets/Assets/1st Floor/Temples/Textures/wood 3.mat",
            "Assets/Assets/1st Floor/Temples/Textures/wood 4.mat",
            "Assets/Assets/1st Floor/Temples/Textures/wood 5.mat",
            "Assets/Assets/1st Floor/Temples/Textures/wood 6.mat",
            "Assets/Assets/1st Floor/Temples/Textures/thatch/thatch.mat"
        };

        Shader urpShader = Shader.Find("TanukiDigital/URP/Standard_Packed_PBR");

        if (urpShader == null)
        {
            Debug.LogError("Could not find shader: TanukiDigital/URP/Standard_Packed_PBR");
            return;
        }

        int updatedCount = 0;

        foreach (string matPath in materialPaths)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (mat != null)
            {
                mat.shader = urpShader;
                EditorUtility.SetDirty(mat);
                updatedCount++;
                Debug.Log($"Updated material: {matPath}");
            }
            else
            {
                Debug.LogWarning($"Could not find material: {matPath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"<color=green>Successfully updated {updatedCount} temple materials to URP shader!</color>");
        EditorUtility.DisplayDialog("Update Complete",
            $"Successfully updated {updatedCount} temple materials to use the URP shader.\n\nThe pink rendering issue should now be fixed!",
            "OK");
    }
}

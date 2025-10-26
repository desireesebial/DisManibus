using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class FixTempleInstancesInScene : EditorWindow
{
    [MenuItem("Tools/Fix Temple Instances in Current Scene")]
    public static void FixTempleInstancesInCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        if (!currentScene.IsValid())
        {
            EditorUtility.DisplayDialog("Error", "No active scene found!", "OK");
            return;
        }

        Debug.Log($"Fixing temples in scene: {currentScene.name}");

        // Find the URP shader
        Shader urpShader = Shader.Find("TanukiDigital/URP/Standard_Packed_PBR");

        if (urpShader == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find shader: TanukiDigital/URP/Standard_Packed_PBR\n\nMake sure the shader compiled successfully!", "OK");
            return;
        }

        // Find all GameObjects with "Temple" or "Pagoda" in their name
        GameObject[] rootObjects = currentScene.GetRootGameObjects();
        int fixedMaterials = 0;
        int fixedObjects = 0;

        foreach (GameObject rootObj in rootObjects)
        {
            // Check this object and all children
            MeshRenderer[] renderers = rootObj.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer renderer in renderers)
            {
                bool objectFixed = false;
                Material[] materials = renderer.sharedMaterials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        // Check if material is using wrong shader or is pink
                        if (materials[i].shader != urpShader)
                        {
                            // Check if this looks like a temple material (has the texture properties we need)
                            if (materials[i].HasProperty("_MainTex") &&
                                materials[i].HasProperty("_NTex") &&
                                materials[i].HasProperty("_MaskTex"))
                            {
                                Debug.Log($"Fixing material '{materials[i].name}' on {renderer.gameObject.name} - changing shader to URP");
                                materials[i].shader = urpShader;
                                fixedMaterials++;
                                objectFixed = true;
                            }
                        }
                    }
                }

                if (objectFixed)
                {
                    renderer.sharedMaterials = materials;
                    EditorUtility.SetDirty(renderer);
                    fixedObjects++;
                }
            }
        }

        if (fixedMaterials > 0)
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            Debug.Log($"<color=green>Fixed {fixedMaterials} materials on {fixedObjects} objects in scene '{currentScene.name}'</color>");
            EditorUtility.DisplayDialog("Success!",
                $"Fixed {fixedMaterials} materials on {fixedObjects} objects.\n\nThe pink temples should now be fixed at all LOD levels!",
                "OK");
        }
        else
        {
            Debug.LogWarning("No temple materials found that needed fixing.");
            EditorUtility.DisplayDialog("No Changes",
                "No temple materials were found that needed fixing.\n\nEither they're already correct, or the temples use different materials than expected.",
                "OK");
        }
    }
}

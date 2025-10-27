using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to generate a vignette texture for health vision effects
/// </summary>
public class VignetteTextureGenerator : EditorWindow
{
    private int textureSize = 512;
    private float vignetteStrength = 0.8f;
    private float vignetteSize = 0.6f;
    private Color vignetteColor = Color.black;
    private string textureName = "HealthVignette";

    [MenuItem("Tools/Generate Vignette Texture")]
    static void ShowWindow()
    {
        VignetteTextureGenerator window = GetWindow<VignetteTextureGenerator>("Vignette Generator");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Vignette Texture Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 128, 2048);
        vignetteStrength = EditorGUILayout.Slider("Vignette Strength", vignetteStrength, 0f, 1f);
        vignetteSize = EditorGUILayout.Slider("Vignette Size", vignetteSize, 0.1f, 1f);
        vignetteColor = EditorGUILayout.ColorField("Vignette Color", vignetteColor);
        textureName = EditorGUILayout.TextField("Texture Name", textureName);

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Vignette Texture", GUILayout.Height(40)))
        {
            GenerateVignetteTexture();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "This will create a radial gradient texture that can be used as a vignette overlay for health effects.\n\n" +
            "The texture will be saved to Assets/Textures/UI/",
            MessageType.Info
        );
    }

    void GenerateVignetteTexture()
    {
        Texture2D vignetteTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float maxDistance = textureSize / 2f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // Calculate distance from center
                float dx = x - center.x;
                float dy = y - center.y;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                // Normalize distance
                float normalizedDistance = distance / maxDistance;

                // Apply vignette curve
                float vignette = Mathf.Pow(normalizedDistance / vignetteSize, 2f);
                vignette = Mathf.Clamp01(vignette) * vignetteStrength;

                // Set pixel color
                Color pixelColor = vignetteColor;
                pixelColor.a = vignette;

                vignetteTexture.SetPixel(x, y, pixelColor);
            }
        }

        vignetteTexture.Apply();

        // Save texture to file
        string directoryPath = "Assets/Textures/UI";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string path = $"{directoryPath}/{textureName}.png";
        byte[] bytes = vignetteTexture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        // Refresh asset database
        AssetDatabase.Refresh();

        // Set texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        Debug.Log($"[VignetteGenerator] Vignette texture created at: {path}");
        EditorUtility.DisplayDialog("Success", $"Vignette texture created!\n\nLocation: {path}", "OK");

        // Select the texture in the project
        Object obj = AssetDatabase.LoadAssetAtPath<Object>(path);
        Selection.activeObject = obj;
        EditorGUIUtility.PingObject(obj);
    }
}

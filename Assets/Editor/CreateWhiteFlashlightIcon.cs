using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Creates a white version of the flashlight icon by inverting the black pixels
/// </summary>
public class CreateWhiteFlashlightIcon : EditorWindow
{
    [MenuItem("Tools/Create White Flashlight Icon")]
    public static void CreateWhiteIcon()
    {
        string sourcePath = "Assets/Textures/UI/flashlight.png";
        string outputPath = "Assets/Textures/UI/flashlight_white.png";

        // Load the source texture
        Texture2D sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);

        if (sourceTexture == null)
        {
            Debug.LogError("Could not load source flashlight.png");
            EditorUtility.DisplayDialog("Error", "Could not load flashlight.png", "OK");
            return;
        }

        // Make sure the texture is readable
        string assetPath = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        bool wasReadable = importer.isReadable;
        if (!importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
            AssetDatabase.Refresh();
            sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
        }

        // Create a new texture
        Texture2D whiteTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);

        // Get all pixels
        Color[] pixels = sourceTexture.GetPixels();

        // Invert the colors (make black into white, keep alpha)
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];

            // If the pixel is black (or close to black), make it white
            // Keep the alpha channel the same
            if (pixel.r < 0.5f && pixel.g < 0.5f && pixel.b < 0.5f)
            {
                pixels[i] = new Color(1f, 1f, 1f, pixel.a);
            }
            else
            {
                // Keep white/transparent areas as is
                pixels[i] = pixel;
            }
        }

        whiteTexture.SetPixels(pixels);
        whiteTexture.Apply();

        // Save as PNG
        byte[] pngData = whiteTexture.EncodeToPNG();
        string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), outputPath);
        File.WriteAllBytes(fullPath, pngData);

        // Restore original import settings if needed
        if (!wasReadable)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh();

        // Configure the new white icon as a sprite
        TextureImporter whiteImporter = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        whiteImporter.textureType = TextureImporterType.Sprite;
        whiteImporter.spriteImportMode = SpriteImportMode.Single;
        whiteImporter.SaveAndReimport();

        AssetDatabase.Refresh();

        Debug.Log("Created white flashlight icon at: " + outputPath);
        EditorUtility.DisplayDialog("Success", "Created flashlight_white.png!\n\nNow run 'Setup Flashlight Icon (Auto)' again.", "OK");
    }
}

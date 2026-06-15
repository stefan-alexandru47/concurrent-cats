#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class FloorAssetCreator
{
    static FloorAssetCreator()
    {
        // Delay execution until editor is fully loaded and ready to compile assets
        EditorApplication.delayCall += CreateFloorSprite;
    }

    [MenuItem("Tools/Create Floor Sprite")]
    public static void CreateFloorSprite()
    {
        string directoryPath = "Assets/Sprites";
        string filePath = directoryPath + "/floor_tile.png";

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (!File.Exists(filePath))
        {
            // Create a 16x16 white texture
            Texture2D tex = new Texture2D(16, 16);
            Color[] colors = new Color[16 * 16];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }
            tex.SetPixels(colors);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            
            // Force import
            AssetDatabase.ImportAsset(filePath);

            // Configure as Sprite
            TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            
            Debug.Log("Created solid floor_tile.png sprite asset successfully!");
        }
    }
}
#endif

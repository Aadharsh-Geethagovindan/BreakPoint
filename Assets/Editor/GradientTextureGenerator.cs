using UnityEngine;
using UnityEditor;

public static class GradientTextureGenerator
{
    [MenuItem("Tools/Generate UI Gradient Texture")]
    public static void GenerateGradient()
    {
        int width = 512;
        int height = 32;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        
        // Center-bright gradient (fades at both ends)
        for (int x = 0; x < width; x++)
        {
            float t = x / (float)(width - 1);
            float center = 0.5f;
            float fade = Mathf.Abs(t - center) / center;   // 0 in center, 1 at edges
            float alpha = Mathf.Clamp01(1f - fade * 2f);   // bright in middle, fades both sides
            Color col = new Color(1f, 1f, 1f, alpha);
            for (int y = 0; y < height; y++)
                tex.SetPixel(x, y, col);
        }


        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        string path = "Assets/UI_WhiteToTransparent.png";
        System.IO.File.WriteAllBytes(path, pngData);
        AssetDatabase.ImportAsset(path);

        // Configure as Sprite for UI
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();

        Debug.Log($"Generated gradient texture at {path}");
    }
}

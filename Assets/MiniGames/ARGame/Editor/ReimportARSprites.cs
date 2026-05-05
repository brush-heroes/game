using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using System.IO;

public static class ReimportARSprites
{
    [MenuItem("Tools/ARGame/Reimport AR Sprites")]
    public static void Execute()
    {
        // Single sprites
        ImportSingle("Assets/MiniGames/ARGame/Assets/toothbrush/static.png");

        // Sprite sheets — brushing toothbrush: 4 frames, 1 row
        ImportSheet("Assets/MiniGames/ARGame/Assets/toothbrush/brushing.png", 4, 1);

        // Mouth guide sprites: 6 frames, 1 row each
        string[] guides = {
            "Assets/MiniGames/ARGame/Assets/mouth_guide/brushing_lower_front.png",
            "Assets/MiniGames/ARGame/Assets/mouth_guide/brushing_lower_left.png",
            "Assets/MiniGames/ARGame/Assets/mouth_guide/brushing_lower_right.png",
            "Assets/MiniGames/ARGame/Assets/mouth_guide/brushing_upper_front.png",
            "Assets/MiniGames/ARGame/Assets/mouth_guide/brushing_upper_left.png",
            "Assets/MiniGames/ARGame/Assets/mouth_guide/brushing_upper_right.png",
        };
        foreach (var path in guides)
            ImportSheet(path, 6, 1);

        AssetDatabase.Refresh();
        Debug.Log("[ReimportARSprites] Done.");
    }

    static void ImportSingle(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning("[ReimportARSprites] Not found: " + path); return; }
        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled       = false;
        imp.filterMode          = FilterMode.Bilinear;
        imp.SaveAndReimport();
        Debug.Log("[ReimportARSprites] Single: " + path);
    }

    static void ImportSheet(string path, int cols, int rows)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning("[ReimportARSprites] Not found: " + path); return; }
        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Multiple;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled       = false;
        imp.filterMode          = FilterMode.Bilinear;
        imp.SaveAndReimport();

        imp.GetSourceTextureWidthAndHeight(out int W, out int H);
        int cw = W / cols, ch = H / rows;
        string baseName = Path.GetFileNameWithoutExtension(path);

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(imp);
        provider.InitSpriteEditorDataProvider();

        var rects = new List<SpriteRect>();
        for (int r = 0; r < rows; r++)
        for (int c = 0; c < cols; c++)
            rects.Add(new SpriteRect
            {
                name      = baseName + "_" + (r * cols + c),
                rect      = new Rect(c * cw, H - (r + 1) * ch, cw, ch),
                pivot     = new Vector2(0.5f, 0.5f),
                alignment = SpriteAlignment.Custom
            });

        provider.SetSpriteRects(rects.ToArray());
        provider.Apply();
        (provider.targetObject as AssetImporter)?.SaveAndReimport();
        Debug.Log($"[ReimportARSprites] Sheet {cols}x{rows}: " + path);
    }
}

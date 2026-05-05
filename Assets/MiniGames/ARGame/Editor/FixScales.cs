using System.IO;
using UnityEngine;
using UnityEditor;

/// Tools > AR Game > Fix Scales & Sprite References
public class FixScales
{
    const string BACTERIA_PREFAB  = "Assets/MiniGames/ARGame/Prefabs/bacteria.prefab";
    const string TOOTHBRUSH_BASE  = "Assets/MiniGames/ARGame/Assets/toothbrush/";
    const string BACTERIA_BASE    = "Assets/MiniGames/ARGame/Assets/bacteria/";

    [MenuItem("Tools/AR Game/Fix Scales & Sprite References")]
    public static void Execute()
    {
        DiagnoseAndFix();
        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[FixScales] Done. Save the scene (Ctrl+S).");
    }

    static void DiagnoseAndFix()
    {
        // ── 1. Brush visual ──────────────────────────────────────────────────
        var brushCtrl = Object.FindObjectOfType<VirtualBrushController>();
        if (brushCtrl != null)
        {
            Transform vis = brushCtrl.transform.Find("BrushVisual");
            if (vis != null)
            {
                // Print current state
                var sr = vis.GetComponent<SpriteRenderer>();
                var anim = vis.GetComponent<BrushAnimator>();

                Debug.Log($"[FixScales] BrushVisual current scale: {vis.localScale}");
                Debug.Log($"[FixScales] BrushVisual SpriteRenderer.sprite: {(sr?.sprite != null ? sr.sprite.name + $" ({sr.sprite.rect.width}x{sr.sprite.rect.height})" : "NULL")}");

                // Check if brushingFrames are null
                if (anim != null)
                {
                    var so = new SerializedObject(anim);
                    var framesArr = so.FindProperty("brushingFrames");
                    int nullCount = 0;
                    for (int i = 0; i < framesArr.arraySize; i++)
                        if (framesArr.GetArrayElementAtIndex(i).objectReferenceValue == null) nullCount++;
                    Debug.Log($"[FixScales] BrushAnimator brushingFrames: {framesArr.arraySize} entries, {nullCount} are null");

                    // Re-assign brushing frames (fixes the first-run race condition)
                    for (int i = 0; i < 4; i++)
                    {
                        Sprite spr = LoadFrame(TOOTHBRUSH_BASE + "brushing.png", i);
                        framesArr.GetArrayElementAtIndex(i).objectReferenceValue = spr;
                        Debug.Log($"[FixScales]   brushing frame {i}: {(spr != null ? spr.name + $" ({spr.rect.width}x{spr.rect.height})" : "NULL")}");
                    }
                    so.ApplyModifiedProperties();
                }

                // Print sprite dimensions to help choose scale
                Sprite staticSpr = AssetDatabase.LoadAssetAtPath<Sprite>(TOOTHBRUSH_BASE + "static.png");
                if (staticSpr != null)
                    Debug.Log($"[FixScales] static.png sprite size: {staticSpr.rect.width}x{staticSpr.rect.height} px, PPU: {staticSpr.pixelsPerUnit}");

                Sprite brushFrame0 = LoadFrame(TOOTHBRUSH_BASE + "brushing.png", 0);
                if (brushFrame0 != null)
                    Debug.Log($"[FixScales] brushing frame0 size: {brushFrame0.rect.width}x{brushFrame0.rect.height} px, PPU: {brushFrame0.pixelsPerUnit}");

                // Compute a scale that makes both sprites roughly the same visual size (~5 cm)
                // We'll normalize by making the static sprite's world height = 0.05m
                if (staticSpr != null && brushFrame0 != null)
                {
                    float staticWorldH  = staticSpr.rect.height  / staticSpr.pixelsPerUnit;
                    float brushingWorldH = brushFrame0.rect.height / brushFrame0.pixelsPerUnit;

                    // Target: brush should appear ~8cm tall in world space
                    float targetWorldSize = 0.08f;
                    float staticScale  = targetWorldSize / staticWorldH;
                    float brushingScale = targetWorldSize / brushingWorldH;

                    // Use the average so both look similar; the animator handles switching
                    float finalScale = (staticScale + brushingScale) / 2f;
                    Debug.Log($"[FixScales] Recommended BrushVisual scale: {finalScale:F4} " +
                              $"(static would be {staticWorldH * finalScale * 100:F1}cm, " +
                              $"brushing {brushingWorldH * finalScale * 100:F1}cm)");

                    vis.localScale = Vector3.one * finalScale;
                    Debug.Log($"[FixScales] Applied BrushVisual scale: {finalScale:F4}");
                }
                else
                {
                    Debug.LogWarning("[FixScales] Could not load sprites to compute scale. Using fallback 0.04.");
                    vis.localScale = Vector3.one * 0.04f;
                }
            }
            else Debug.LogWarning("[FixScales] BrushVisual not found on VirtualToothbrush.");
        }

        // ── 2. Bacteria prefab ───────────────────────────────────────────────
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(BACTERIA_PREFAB);
        try
        {
            Transform bvis = prefabRoot.transform.Find("BacteriaVisual");
            if (bvis != null)
            {
                var sr = bvis.GetComponent<SpriteRenderer>();
                Debug.Log($"[FixScales] BacteriaVisual current scale: {bvis.localScale}");
                Debug.Log($"[FixScales] BacteriaVisual SpriteRenderer.sprite: {(sr?.sprite != null ? sr.sprite.name : "NULL")}");

                // Check frame sizes
                Sprite born0 = LoadFrame(BACTERIA_BASE + "born_1.png", 0);
                if (born0 != null)
                {
                    float worldH = born0.rect.height / born0.pixelsPerUnit;
                    // Target: bacteria ~3cm tall
                    float targetSize = 0.03f;
                    float scale = targetSize / worldH;
                    bvis.localScale = Vector3.one * scale;
                    Debug.Log($"[FixScales] Applied BacteriaVisual scale: {scale:F4} " +
                              $"(born frame0: {born0.rect.width}x{born0.rect.height}px, PPU:{born0.pixelsPerUnit}, " +
                              $"world height = {worldH * scale * 100:F1}cm)");
                }
                else
                {
                    Debug.LogWarning("[FixScales] Could not load born_1 frame. Using fallback 0.08.");
                    bvis.localScale = Vector3.one * 0.08f;
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, BACTERIA_PREFAB);
            }
            else Debug.LogWarning("[FixScales] BacteriaVisual not found in bacteria.prefab.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    static Sprite LoadFrame(string texPath, int index)
    {
        string name = Path.GetFileNameWithoutExtension(texPath) + "_" + index;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath(texPath))
            if (a is Sprite s && s.name == name) return s;
        return null;
    }
}

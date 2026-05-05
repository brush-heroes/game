using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One-shot editor utility that configures sprites, builds the Guide Panel UI,
/// updates the VirtualToothbrush and bacteria prefab to use 2-D sprites.
/// Run via: Tools ▶ AR Game ▶ Setup Scene
/// </summary>
public class ARGameSetup
{
    const string BACTERIA_PREFAB  = "Assets/MiniGames/ARGame/Prefabs/bacteria.prefab";
    const string BACTERIA_BASE    = "Assets/MiniGames/ARGame/Assets/bacteria/";
    const string TOOTHBRUSH_BASE  = "Assets/MiniGames/ARGame/Assets/toothbrush/";
    const string GUIDE_BASE       = "Assets/MiniGames/ARGame/Assets/mouth_guide/";

    // ── Menu entry ───────────────────────────────────────────────────────────

    [MenuItem("Tools/AR Game/Fix Scales and Brush Frames")]
    public static void FixScalesAndFrames()
    {
        // Re-assign brushing frames (may have been null if first run happened before slicing)
        var brushCtrl = Object.FindObjectOfType<VirtualBrushController>();
        if (brushCtrl != null)
        {
            Transform vis = brushCtrl.transform.Find("BrushVisual");
            if (vis != null)
            {
                // Print diagnostic info
                Sprite staticSpr  = AssetDatabase.LoadAssetAtPath<Sprite>(TOOTHBRUSH_BASE + "static.png");
                Sprite brushF0    = LoadFrameSprite(TOOTHBRUSH_BASE + "brushing.png", 0);

                if (staticSpr  != null) Debug.Log("[Fix] static.png  " + staticSpr.rect.width  + "x" + staticSpr.rect.height  + "px  PPU:" + staticSpr.pixelsPerUnit);
                if (brushF0    != null) Debug.Log("[Fix] brushing[0] " + brushF0.rect.width    + "x" + brushF0.rect.height    + "px  PPU:" + brushF0.pixelsPerUnit);

                // Re-assign brushing frames
                var ba = vis.GetComponent<BrushAnimator>();
                if (ba != null)
                {
                    var so = new SerializedObject(ba);
                    var arr = so.FindProperty("brushingFrames");
                    arr.arraySize = 4;
                    for (int i = 0; i < 4; i++)
                    {
                        Sprite s = LoadFrameSprite(TOOTHBRUSH_BASE + "brushing.png", i);
                        arr.GetArrayElementAtIndex(i).objectReferenceValue = s;
                        Debug.Log("[Fix] brushing frame " + i + ": " + (s != null ? s.name : "NULL"));
                    }
                    so.ApplyModifiedProperties();
                }

                // Scale so static sprite appears ~8cm in world space
                // (both static and brushing frames should now be similar height after correct slicing)
                if (staticSpr != null)
                {
                    float worldH = staticSpr.rect.height / staticSpr.pixelsPerUnit;
                    float s2     = 0.08f / worldH;
                    vis.localScale = Vector3.one * s2;
                    Debug.Log("[Fix] BrushVisual scale set to " + s2 +
                              " (static world H = " + (worldH * s2) + "m)");
                }
                else
                {
                    vis.localScale = Vector3.one * 0.016f;
                    Debug.Log("[Fix] BrushVisual scale fallback 0.016");
                }
            }
        }

        // Fix bacteria scale: target ~3cm tall
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(BACTERIA_PREFAB);
        try
        {
            Transform bvis = prefabRoot.transform.Find("BacteriaVisual");
            if (bvis != null)
            {
                Sprite born0 = LoadFrameSprite(BACTERIA_BASE + "born_1.png", 0);
                if (born0 != null) Debug.Log("[Fix] born_1[0]   " + born0.rect.width + "x" + born0.rect.height + "px  PPU:" + born0.pixelsPerUnit);

                // Target 5 cm in world space for bacteria
                float scale = born0 != null ? (0.05f / (born0.rect.height / born0.pixelsPerUnit)) : 0.08f;
                bvis.localScale = Vector3.one * scale;
                Debug.Log("[Fix] BacteriaVisual scale set to " + scale +
                          " (world size = " + (born0 != null ? (born0.rect.height / born0.pixelsPerUnit * scale) : 0) + "m)");

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, BACTERIA_PREFAB);
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(prefabRoot); }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[Fix] Done — save scene (Ctrl+S).");
    }

    [MenuItem("Tools/AR Game/Setup Scene")]
    public static void Execute()
    {
        SetupSpriteImports();
        SetupCanvas();
        SetupVirtualToothbrush();
        SetupBacteriaPrefab();
        FixSessionManager();

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("[ARGameSetup] ✓ Setup complete! Save the scene (Ctrl+S).");
    }

    // ── 1. Sprite imports ────────────────────────────────────────────────────

    static void SetupSpriteImports()
    {
        // 4-frame sprite sheets (horizontal strip)
        string[] sheets4 =
        {
            BACTERIA_BASE   + "born_1.png",
            BACTERIA_BASE   + "living_2.png",
            BACTERIA_BASE   + "dying_3.png",
            BACTERIA_BASE   + "brushed_4.png",
            TOOTHBRUSH_BASE + "brushing.png",
            GUIDE_BASE      + "brushing_upper_front.png",
            GUIDE_BASE      + "brushing_lower_front.png",
            GUIDE_BASE      + "brushing_upper_left.png",
            GUIDE_BASE      + "brushing_lower_left.png",
            GUIDE_BASE      + "brushing_upper_right.png",
            GUIDE_BASE      + "brushing_lower_right.png",
            GUIDE_BASE      + "completion.png",
            GUIDE_BASE      + "failed.png",
        };
        foreach (string p in sheets4) SliceSpriteSheet(p, 4);

        // Single-frame sprites
        string[] singles =
        {
            TOOTHBRUSH_BASE + "static.png",
            GUIDE_BASE      + "default.png",
            "Assets/MiniGames/ARGame/Assets/logos/dinamico.png",
            "Assets/MiniGames/ARGame/Assets/logos/guiado.png",
            GUIDE_BASE      + "upper_front.png",
            GUIDE_BASE      + "lower_front.png",
            GUIDE_BASE      + "upper_left.png",
            GUIDE_BASE      + "lower_left.png",
            GUIDE_BASE      + "upper_right.png",
            GUIDE_BASE      + "lower_right.png",
        };
        foreach (string p in singles) SetSingleSprite(p);

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    static void SliceSpriteSheet(string path, int frames)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning("[ARGameSetup] Missing: " + path); return; }

        imp.textureType    = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.filterMode     = FilterMode.Bilinear;
        imp.isReadable     = true;
        imp.SaveAndReimport();

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) { Debug.LogWarning("[ARGameSetup] Could not load: " + path); return; }

        int w = tex.width, h = tex.height;
        SpriteMetaData[] meta;
        string baseName = Path.GetFileNameWithoutExtension(path);

        // Detect layout: landscape image → horizontal strip, portrait → vertical.
        // Uses floor division — last frame absorbs any remainder pixels.
        meta = new SpriteMetaData[frames];
        if (w >= h)
        {
            // Horizontal strip (frames side by side)
            int fw = w / frames;
            for (int i = 0; i < frames; i++)
            {
                int x     = i * fw;
                int width = (i == frames - 1) ? w - x : fw;
                meta[i] = MakeMeta(baseName + "_" + i, x, 0, width, h);
            }
        }
        else
        {
            // Vertical strip (frames stacked)
            int fh = h / frames;
            for (int i = 0; i < frames; i++)
            {
                int rowFromBottom = frames - 1 - i;
                int y      = rowFromBottom * fh;
                int height = (rowFromBottom == 0) ? h - (frames - 1) * fh : fh;
                meta[i] = MakeMeta(baseName + "_" + i, 0, y, w, height);
            }
        }

        imp.spritesheet = meta;
        imp.isReadable  = false;
        imp.SaveAndReimport();
        Debug.Log($"[ARGameSetup] Sliced {path} → {frames} frames");
    }

    static SpriteMetaData MakeMeta(string name, float x, float y, float w, float h)
        => new SpriteMetaData
        {
            name      = name,
            rect      = new Rect(x, y, w, h),
            pivot     = new Vector2(0.5f, 0.5f),
            alignment = (int)SpriteAlignment.Center
        };

    static void SetSingleSprite(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) return;
        imp.textureType    = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.filterMode     = FilterMode.Bilinear;
        imp.SaveAndReimport();
    }

    // ── 2. Canvas — Guide Panel ──────────────────────────────────────────────

    static void SetupCanvas()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[ARGameSetup] No Canvas in scene."); return; }

        if (canvas.transform.Find("GuidePanel") != null)
        { Debug.Log("[ARGameSetup] GuidePanel already exists — skipped."); return; }

        SessionManager sessionMgr = Object.FindObjectOfType<SessionManager>();

        // ── GuidePanel root ──
        GameObject panel = NewGO("GuidePanel", canvas.transform);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.72f);
        var panelRT = panel.GetComponent<RectTransform>();
        AnchorStretchBottom(panelRT, 270f);
        var guideUI = panel.AddComponent<BrushingGuideUI>();

        // ── GuideImage ──
        GameObject imgGO = NewGO("GuideImage", panel.transform);
        var guideImg = imgGO.AddComponent<Image>();
        guideImg.preserveAspect = true;
        imgGO.AddComponent<SpriteAnimator>();
        StretchFill(imgGO.GetComponent<RectTransform>(), 8f);

        // ── Alert overlay (sits above guide panel, on the Canvas directly) ──
        GameObject alert = NewGO("AlertOverlay", canvas.transform);
        var alertImg = alert.AddComponent<Image>();
        alertImg.color = new Color(1f, 0.45f, 0f, 0.88f);
        var alertRT = alert.GetComponent<RectTransform>();
        alertRT.anchorMin = new Vector2(0.08f, 0.38f);
        alertRT.anchorMax = new Vector2(0.92f, 0.50f);
        alertRT.offsetMin = alertRT.offsetMax = Vector2.zero;
        var alertTextComp = CreateTMPLabel(alert.transform, "¡Sigue cepillando!", 36, Color.white, FontStyles.Bold);
        alert.SetActive(false);

        // ── Assign serialized fields ──
        // Note: Start/Back buttons are created by ARGameUISetup2 (Setup Full UI).
        SerializedObject so = new SerializedObject(guideUI);
        SetRef(so, "sessionManager", sessionMgr);
        SetRef(so, "guideImage",     guideImg);
        SetRef(so, "alertPanel",     alert);
        SetRef(so, "alertText",      alertTextComp);
        SetRef(so, "defaultSprite",  LoadSprite(GUIDE_BASE + "default.png"));
        so.ApplyModifiedProperties();

        // Completion & failed frames
        AssignFrameArray(so, "completionFrames", GUIDE_BASE + "completion.png", 4);
        AssignFrameArray(so, "failedFrames",     GUIDE_BASE + "failed.png",     4);

        // Zone data
        AssignZoneData(so);

        so.ApplyModifiedProperties();
        Debug.Log("[ARGameSetup] Guide panel built.");
    }

    static void AssignZoneData(SerializedObject so)
    {
        var entries = new (MouthZone zone, string ind, string anim)[]
        {
            (MouthZone.UpperLeft,  "upper_left.png",  "brushing_upper_left.png"),
            (MouthZone.UpperRight, "upper_right.png", "brushing_upper_right.png"),
            (MouthZone.FrontUpper, "upper_front.png", "brushing_upper_front.png"),
            (MouthZone.LowerLeft,  "lower_left.png",  "brushing_lower_left.png"),
            (MouthZone.LowerRight, "lower_right.png", "brushing_lower_right.png"),
            (MouthZone.FrontLower, "lower_front.png", "brushing_lower_front.png"),
        };

        var arr = so.FindProperty("zoneGuideData");
        arr.arraySize = entries.Length;
        for (int i = 0; i < entries.Length; i++)
        {
            var (zone, ind, anim) = entries[i];
            var el = arr.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("zone").enumValueIndex = (int)zone;
            el.FindPropertyRelative("zoneIndicator").objectReferenceValue =
                LoadSprite(GUIDE_BASE + ind);
            FillFrameArrayOnProp(el.FindPropertyRelative("brushingFrames"),
                GUIDE_BASE + anim, 4);
        }
        so.ApplyModifiedProperties();
    }

    // ── 3. VirtualToothbrush ─────────────────────────────────────────────────

    static void SetupVirtualToothbrush()
    {
        var ctrl = Object.FindObjectOfType<VirtualBrushController>();
        if (ctrl == null) { Debug.LogWarning("[ARGameSetup] VirtualBrushController not found."); return; }

        Transform t = ctrl.transform;

        // Disable the placeholder Capsule mesh
        Transform capsule = t.Find("Capsule");
        if (capsule != null)
            foreach (var r in capsule.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;

        if (t.Find("BrushVisual") != null)
        { Debug.Log("[ARGameSetup] BrushVisual already exists — skipped."); return; }

        GameObject vis = NewGO("BrushVisual", t);
        vis.transform.localPosition = Vector3.zero;
        vis.transform.localScale    = Vector3.one * 0.06f;

        vis.AddComponent<SpriteRenderer>();
        vis.AddComponent<SpriteAnimator>();
        var anim = vis.AddComponent<BrushAnimator>();
        vis.AddComponent<Billboard>();

        SerializedObject so = new SerializedObject(anim);
        SetRef(so, "staticSprite", LoadSprite(TOOTHBRUSH_BASE + "static.png"));
        AssignFrameArray(so, "brushingFrames", TOOTHBRUSH_BASE + "brushing.png", 4);
        so.ApplyModifiedProperties();

        Debug.Log("[ARGameSetup] VirtualToothbrush visual updated.");
    }

    // ── 4. Bacteria prefab ───────────────────────────────────────────────────

    static void SetupBacteriaPrefab()
    {
        if (!File.Exists(Path.Combine(Application.dataPath.Replace("/Assets", ""), BACTERIA_PREFAB)))
        { Debug.LogWarning("[ARGameSetup] Bacteria prefab not found: " + BACTERIA_PREFAB); return; }

        GameObject root = PrefabUtility.LoadPrefabContents(BACTERIA_PREFAB);
        try
        {
            // Disable existing mesh renderers
            foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))
                mr.enabled = false;

            if (root.transform.Find("BacteriaVisual") == null)
            {
                GameObject vis = new GameObject("BacteriaVisual");
                vis.transform.SetParent(root.transform, false);
                vis.transform.localPosition = Vector3.zero;
                vis.transform.localScale    = Vector3.one * 0.04f;

                vis.AddComponent<SpriteRenderer>();
                vis.AddComponent<SpriteAnimator>();
                var ba = vis.AddComponent<BacteriaAnimator>();
                vis.AddComponent<Billboard>();

                SerializedObject so = new SerializedObject(ba);
                AssignFrameArray(so, "bornFrames",   BACTERIA_BASE + "born_1.png",   4);
                AssignFrameArray(so, "livingFrames", BACTERIA_BASE + "living_2.png", 4);
                AssignFrameArray(so, "dyingFrames",  BACTERIA_BASE + "dying_3.png",  4);
                so.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(root, BACTERIA_PREFAB);
            Debug.Log("[ARGameSetup] Bacteria prefab updated.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ── 5. SessionManager — disable auto-start ───────────────────────────────

    static void FixSessionManager()
    {
        var sm = Object.FindObjectOfType<SessionManager>();
        if (sm == null) return;
        var so = new SerializedObject(sm);
        so.FindProperty("autoStart").boolValue = false;
        so.ApplyModifiedProperties();
        Debug.Log("[ARGameSetup] autoStart = false.");
    }

    // ── Sprite helpers ───────────────────────────────────────────────────────

    static Sprite LoadSprite(string path)
        => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    static Sprite LoadFrameSprite(string texPath, int index)
    {
        string name = Path.GetFileNameWithoutExtension(texPath) + "_" + index;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath(texPath))
            if (a is Sprite s && s.name == name) return s;
        return null;
    }

    static void AssignFrameArray(SerializedObject so, string prop, string texPath, int count)
    {
        FillFrameArrayOnProp(so.FindProperty(prop), texPath, count);
        so.ApplyModifiedProperties();
    }

    static void FillFrameArrayOnProp(SerializedProperty arr, string texPath, int count)
    {
        arr.arraySize = count;
        for (int i = 0; i < count; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = LoadFrameSprite(texPath, i);
    }

    // ── UI helpers ───────────────────────────────────────────────────────────

    static GameObject NewGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    // Full-width panel pinned to the bottom with a fixed pixel height.
    static void AnchorStretchBottom(RectTransform rt, float height)
    {
        rt.anchorMin  = new Vector2(0, 0);
        rt.anchorMax  = new Vector2(1, 0);
        rt.pivot      = new Vector2(0.5f, 0);
        rt.offsetMin  = Vector2.zero;
        rt.offsetMax  = new Vector2(0, height);
    }

    // Fills the parent RectTransform with optional padding.
    static void StretchFill(RectTransform rt, float pad)
    {
        rt.anchorMin  = Vector2.zero;
        rt.anchorMax  = Vector2.one;
        rt.offsetMin  = new Vector2(pad, pad);
        rt.offsetMax  = new Vector2(-pad, -pad);
    }

    static GameObject CreateStyledButton(string name, Transform parent, string label, Color bg)
    {
        var btn = NewGO(name, parent);
        var img = btn.AddComponent<Image>();
        img.color = bg;
        btn.AddComponent<Button>();
        CreateTMPLabel(btn.transform, label, 42, Color.white, FontStyles.Bold);
        return btn;
    }

    static TextMeshProUGUI CreateTMPLabel(Transform parent, string text, float size,
        Color color, FontStyles style = FontStyles.Normal)
    {
        var go  = NewGO("Label", parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        StretchFill(go.GetComponent<RectTransform>(), 6f);
        return tmp;
    }

    static void SetRef(SerializedObject so, string prop, Object value)
        => so.FindProperty(prop).objectReferenceValue = value;
}

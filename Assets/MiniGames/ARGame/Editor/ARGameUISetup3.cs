using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

/// Setup step 3: dual-mode start buttons, zone notification overlay, GuidedModeController.
/// Run via: Tools ▶ AR Game ▶ Setup Mode Buttons + Guided
/// Prerequisites: ARGameSetup and ARGameUISetup2 must have been run first.
public class ARGameUISetup3
{
    const string BACTERIA_PREFAB = "Assets/MiniGames/ARGame/Prefabs/bacteria.prefab";
    const string BACTERIA_BASE   = "Assets/MiniGames/ARGame/Assets/bacteria/";
    const string GUIDE_BASE      = "Assets/MiniGames/ARGame/Assets/mouth_guide/";
    const string LOGOS_BASE      = "Assets/MiniGames/ARGame/Assets/logos/";
    const string SCENE_PATH      = "Assets/MiniGames/ARGame/Scenes/ARGame.unity";

    [MenuItem("Tools/AR Game/Setup Mode Buttons + Guided")]
    public static void Execute()
    {
        // 1. Import new sprites
        SliceNewSprites();

        // 2. Assign brushedFrames on bacteria prefab
        UpdateBacteriaPrefab();

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[ARGameUISetup3] No Canvas found."); return; }

        var sm        = Object.FindObjectOfType<SessionManager>();
        var bs        = Object.FindObjectOfType<BacteriaSpawner>();
        var mzm       = Object.FindObjectOfType<MouthZoneManager>();
        var brushCtrl = Object.FindObjectOfType<VirtualBrushController>();
        var gsm       = Object.FindObjectOfType<GameScreenManager>();

        if (gsm == null)
        {
            Debug.LogError("[ARGameUISetup3] GameScreenManager not found — run Setup Full UI first.");
            return;
        }

        // 3. Replace single StartButton with DynamicButton + GuideButton
        UpdateStartScreen(canvas.transform, gsm);

        // 4. Full-screen zone transition notification overlay
        CreateZoneNotification(canvas.transform, sm);

        // 5. GuidedModeController on VirtualBrushController GO
        var guided = SetupGuidedModeController(sm, bs, mzm, brushCtrl);

        // 6. Wire timerPanel, scorePanel, guidedController on GameScreenManager
        UpdateGameScreenManager(gsm, guided, canvas.transform);

        // 7. Save scene to the correct path
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.SaveAssets();

        Debug.Log("[ARGameUISetup3] Done — scene saved to " + SCENE_PATH);
    }

    // ── 1. Sprite slicing ────────────────────────────────────────────────────

    static void SliceNewSprites()
    {
        SliceSpriteSheet(BACTERIA_BASE + "brushed_4.png", 4);
        SetSingleSprite(LOGOS_BASE + "dinamico.png");
        SetSingleSprite(LOGOS_BASE + "guiado.png");
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    static void SliceSpriteSheet(string path, int frames)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning("[ARGameUISetup3] Missing: " + path); return; }

        imp.textureType      = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Multiple;
        imp.filterMode       = FilterMode.Bilinear;
        imp.isReadable       = true;
        imp.SaveAndReimport();

        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (tex == null) { Debug.LogWarning("[ARGameUISetup3] Could not load: " + path); return; }

        int w = tex.width, h = tex.height;
        string baseName = Path.GetFileNameWithoutExtension(path);
        var meta = new SpriteMetaData[frames];

        if (w >= h)
        {
            int fw = w / frames;
            for (int i = 0; i < frames; i++)
            {
                int x     = i * fw;
                int width = (i == frames - 1) ? w - x : fw;
                meta[i]   = MakeMeta(baseName + "_" + i, x, 0, width, h);
            }
        }
        else
        {
            int fh = h / frames;
            for (int i = 0; i < frames; i++)
            {
                int row    = frames - 1 - i;
                int y      = row * fh;
                int height = (row == 0) ? h - (frames - 1) * fh : fh;
                meta[i]    = MakeMeta(baseName + "_" + i, 0, y, w, height);
            }
        }

        imp.spritesheet = meta;
        imp.isReadable  = false;
        imp.SaveAndReimport();
        Debug.Log("[ARGameUISetup3] Sliced " + path + " → " + frames + " frames");
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
        if (imp == null) { Debug.LogWarning("[ARGameUISetup3] Missing: " + path); return; }
        imp.textureType      = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.filterMode       = FilterMode.Bilinear;
        imp.SaveAndReimport();
    }

    // ── 2. Bacteria prefab — add brushedFrames ───────────────────────────────

    static void UpdateBacteriaPrefab()
    {
        if (!File.Exists(Path.Combine(Application.dataPath.Replace("/Assets", ""), BACTERIA_PREFAB)))
        { Debug.LogWarning("[ARGameUISetup3] Bacteria prefab not found."); return; }

        var root = PrefabUtility.LoadPrefabContents(BACTERIA_PREFAB);
        try
        {
            var vis = root.transform.Find("BacteriaVisual");
            if (vis == null) { Debug.LogWarning("[ARGameUISetup3] BacteriaVisual not found in prefab."); return; }

            var ba = vis.GetComponent<BacteriaAnimator>();
            if (ba == null) { Debug.LogWarning("[ARGameUISetup3] BacteriaAnimator not found."); return; }

            var so  = new SerializedObject(ba);
            var arr = so.FindProperty("brushedFrames");
            arr.arraySize = 4;
            for (int i = 0; i < 4; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue =
                    LoadFrameSprite(BACTERIA_BASE + "brushed_4.png", i);
            so.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(root, BACTERIA_PREFAB);
            Debug.Log("[ARGameUISetup3] brushedFrames assigned on bacteria prefab.");
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    // ── 3. StartScreen: replace single button with two mode buttons ──────────

    static void UpdateStartScreen(Transform canvasRoot, GameScreenManager gsm)
    {
        var startScreen = canvasRoot.Find("StartScreen");
        if (startScreen == null)
        { Debug.LogError("[ARGameUISetup3] StartScreen not found — run Setup Full UI first."); return; }

        if (startScreen.Find("DynamicButton") != null)
        { Debug.Log("[ARGameUISetup3] DynamicButton already exists — skipped."); return; }

        // Remove legacy single start button
        var oldBtn = startScreen.Find("StartButton");
        if (oldBtn != null)
        {
            Object.DestroyImmediate(oldBtn.gameObject);
            Debug.Log("[ARGameUISetup3] Removed old StartButton.");
        }

        // ── DynamicButton (upper) ──
        var dynBtn = CreateModeButton("DynamicButton", startScreen,
            LOGOS_BASE + "dinamico.png", "Dinámico",
            new Color(0.12f, 0.72f, 0.22f));
        SetArea(dynBtn.GetComponent<RectTransform>(), 0.05f, 0.20f, 0.95f, 0.34f);

        // ── GuideButton (lower) ──
        var guideBtn = CreateModeButton("GuideButton", startScreen,
            LOGOS_BASE + "guiado.png", "Solo guía",
            new Color(0.22f, 0.48f, 0.85f));
        SetArea(guideBtn.GetComponent<RectTransform>(), 0.05f, 0.04f, 0.95f, 0.18f);

        UnityEventTools.AddPersistentListener(
            dynBtn.GetComponent<Button>().onClick, gsm.OnDynamicButtonPressed);
        UnityEventTools.AddPersistentListener(
            guideBtn.GetComponent<Button>().onClick, gsm.OnGuidedButtonPressed);

        Debug.Log("[ARGameUISetup3] Mode buttons created.");
    }

    static GameObject CreateModeButton(string name, Transform parent,
        string logoPath, string label, Color bg)
    {
        var go = NewGO(name, parent);
        go.AddComponent<Image>().color = bg;
        go.AddComponent<Button>();

        // Logo — left 25 %
        var logoGO  = NewGO("LogoImage", go.transform);
        var logoImg = logoGO.AddComponent<Image>();
        logoImg.sprite         = AssetDatabase.LoadAssetAtPath<Sprite>(logoPath);
        logoImg.preserveAspect = true;
        logoImg.raycastTarget  = false;
        SetArea(logoGO.GetComponent<RectTransform>(), 0.02f, 0.08f, 0.26f, 0.92f);

        // Label — right 72 %
        var lblGO  = NewGO("Label", go.transform);
        var lblTMP = lblGO.AddComponent<TextMeshProUGUI>();
        lblTMP.text          = label;
        lblTMP.fontSize      = 38f;
        lblTMP.fontStyle     = FontStyles.Bold;
        lblTMP.color         = Color.white;
        lblTMP.alignment     = TextAlignmentOptions.MidlineLeft;
        lblTMP.raycastTarget = false;
        SetArea(lblGO.GetComponent<RectTransform>(), 0.28f, 0.05f, 0.97f, 0.95f);

        return go;
    }

    // ── 4. ZoneNotification full-screen overlay ──────────────────────────────

    static void CreateZoneNotification(Transform canvasRoot, SessionManager sm)
    {
        if (canvasRoot.Find("ZoneNotification") != null)
        { Debug.Log("[ARGameUISetup3] ZoneNotification already exists — skipped."); return; }

        // Root: full-screen CanvasGroup, starts invisible
        var notifGO = NewGO("ZoneNotification", canvasRoot);
        var cg      = notifGO.AddComponent<CanvasGroup>();
        cg.alpha          = 0f;
        cg.interactable   = false;
        cg.blocksRaycasts = false;
        SetFullScreen(notifGO.GetComponent<RectTransform>());

        // Darkening backdrop
        var bg = NewGO("Background", notifGO.transform);
        bg.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.70f);
        SetFullScreen(bg.GetComponent<RectTransform>());

        // Center card
        var card = NewGO("Card", notifGO.transform);
        card.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.16f, 0.95f);
        SetArea(card.GetComponent<RectTransform>(), 0.12f, 0.33f, 0.88f, 0.67f);

        // Zone indicator image (upper half of card)
        var zoneImgGO  = NewGO("ZoneImage", card.transform);
        var zoneImg    = zoneImgGO.AddComponent<Image>();
        zoneImg.preserveAspect = true;
        zoneImg.raycastTarget  = false;
        SetArea(zoneImgGO.GetComponent<RectTransform>(), 0.30f, 0.45f, 0.70f, 0.95f);

        // Zone name text (lower half of card)
        var zoneTxtGO = NewGO("ZoneText", card.transform);
        var zoneTMP   = zoneTxtGO.AddComponent<TextMeshProUGUI>();
        zoneTMP.text         = "Siguiente zona:\n<b>—</b>";
        zoneTMP.fontSize     = 30f;
        zoneTMP.color        = Color.white;
        zoneTMP.alignment    = TextAlignmentOptions.Center;
        zoneTMP.raycastTarget = false;
        SetArea(zoneTxtGO.GetComponent<RectTransform>(), 0.04f, 0.02f, 0.96f, 0.48f);

        // ZoneNotificationUI component
        var znUI = notifGO.AddComponent<ZoneNotificationUI>();
        var znSO = new SerializedObject(znUI);
        SetRef(znSO, "sessionManager", sm);
        SetRef(znSO, "overlay",        cg);
        SetRef(znSO, "zoneImage",      zoneImg);
        SetRef(znSO, "zoneText",       zoneTMP);

        // Zone indicator sprites (one per zone)
        var znEntries = new (MouthZone zone, string ind)[]
        {
            (MouthZone.UpperLeft,  "upper_left.png"),
            (MouthZone.UpperRight, "upper_right.png"),
            (MouthZone.FrontUpper, "upper_front.png"),
            (MouthZone.LowerLeft,  "lower_left.png"),
            (MouthZone.LowerRight, "lower_right.png"),
            (MouthZone.FrontLower, "lower_front.png"),
        };
        var znArr = znSO.FindProperty("zoneGuideData");
        znArr.arraySize = znEntries.Length;
        for (int i = 0; i < znEntries.Length; i++)
        {
            var (zone, ind) = znEntries[i];
            var el = znArr.GetArrayElementAtIndex(i);
            el.FindPropertyRelative("zone").enumValueIndex = (int)zone;
            el.FindPropertyRelative("zoneIndicator").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>(GUIDE_BASE + ind);
        }
        znSO.ApplyModifiedProperties();

        Debug.Log("[ARGameUISetup3] ZoneNotification overlay created.");
    }

    // ── 5. GuidedModeController ──────────────────────────────────────────────

    static GuidedModeController SetupGuidedModeController(
        SessionManager sm, BacteriaSpawner bs,
        MouthZoneManager mzm, VirtualBrushController brushCtrl)
    {
        if (brushCtrl == null)
        { Debug.LogError("[ARGameUISetup3] VirtualBrushController not found."); return null; }

        var guided = brushCtrl.GetComponent<GuidedModeController>();
        if (guided == null) guided = brushCtrl.gameObject.AddComponent<GuidedModeController>();

        var so = new SerializedObject(guided);
        SetRef(so, "sessionManager",   sm);
        SetRef(so, "bacteriaSpawner",  bs);
        SetRef(so, "mouthZoneManager", mzm);
        SetRef(so, "brushController",  brushCtrl);
        so.ApplyModifiedProperties();

        Debug.Log("[ARGameUISetup3] GuidedModeController configured on " + brushCtrl.gameObject.name);
        return guided;
    }

    // ── 6. Update GameScreenManager ──────────────────────────────────────────

    static void UpdateGameScreenManager(GameScreenManager gsm,
        GuidedModeController guided, Transform canvasRoot)
    {
        var so = new SerializedObject(gsm);

        if (guided != null) SetRef(so, "guidedController", guided);

        var gameHUD = canvasRoot.Find("GameHUD");
        if (gameHUD != null)
        {
            var topBar = gameHUD.Find("TopBar");
            if (topBar != null)
            {
                var timerT = topBar.Find("TimerPanel");
                var scoreT = topBar.Find("ScorePanel");
                if (timerT != null) SetRef(so, "timerPanel", timerT.gameObject);
                if (scoreT != null) SetRef(so, "scorePanel", scoreT.gameObject);
            }
        }

        so.ApplyModifiedProperties();
        Debug.Log("[ARGameUISetup3] GameScreenManager: guidedController, timerPanel, scorePanel wired.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static GameObject NewGO(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void SetFullScreen(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetArea(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetRef(SerializedObject so, string prop, Object value)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = value;
        else Debug.LogWarning("[ARGameUISetup3] Property not found: " + prop);
    }

    static Sprite LoadFrameSprite(string texPath, int index)
    {
        string name = Path.GetFileNameWithoutExtension(texPath) + "_" + index;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath(texPath))
            if (a is Sprite s && s.name == name) return s;
        return null;
    }
}

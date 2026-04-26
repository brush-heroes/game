using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using TMPro;

/// Tools ▶ Menu ▶ Setup Menu Scene
public static class MenuSetup
{
    const string SCENE_PATH = "Assets/Core/Scenes/MenuScene.unity";
    const string IMG        = "Assets/Menu/Assets/images/";
    const string VID        = "Assets/Menu/Assets/videos/background.mp4";

    // ── Entry point ───────────────────────────────────────────────────────────

    [MenuItem("Tools/Menu/Setup Menu Scene")]
    public static void Execute()
    {
        ConfigureSprites();
        AssetDatabase.Refresh();
        BuildScene();
        Debug.Log("[MenuSetup] Complete.");
    }

    // ── Sprite import ─────────────────────────────────────────────────────────

    static void ConfigureSprites()
    {
        foreach (var f in new[]
        {
            "calendar.png", "moon.png", "sun.png", "star.png", "racha.png",
            "thumbnail_ar.png", "thumbnail_brushing_game.png", "thumbnail_dental_floss.png"
        })
            ImportSingle(IMG + f);

        ImportSheet(IMG + "cheering.png",   4, 1);
        ImportSheet(IMG + "sad.png",        4, 1);
        ImportSheet(IMG + "saludating.png", 4, 1);
        ImportSheet(IMG + "nutral.png",     4, 1);
    }

    static void ImportSingle(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning("[MenuSetup] Missing: " + path); return; }
        imp.textureType         = TextureImporterType.Sprite;
        imp.spriteImportMode    = SpriteImportMode.Single;
        imp.alphaIsTransparency = true;
        imp.mipmapEnabled       = false;
        imp.filterMode          = FilterMode.Bilinear;
        imp.SaveAndReimport();
    }

    static void ImportSheet(string path, int cols, int rows)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null) { Debug.LogWarning("[MenuSetup] Missing: " + path); return; }
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
    }

    // ── Scene builder ─────────────────────────────────────────────────────────

    static void BuildScene()
    {
        var scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        foreach (var go in scene.GetRootGameObjects())
            Object.DestroyImmediate(go);

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Main Camera (required for Game view even with ScreenSpaceOverlay canvas)
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = Color.black;
        cam.orthographic     = true;
        cam.depth            = -1;
        camGO.AddComponent<AudioListener>();

        // EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // ── Video background ──────────────────────────────────────────────────
        var bgGO  = MakeGO(canvasGO.transform, "VideoBg");
        Stretch(bgGO.RT());
        var rawImg  = bgGO.AddComponent<RawImage>();
        rawImg.color = Color.white;
        var vidPlayer = bgGO.AddComponent<VideoPlayer>();
        vidPlayer.renderMode    = VideoRenderMode.RenderTexture;
        vidPlayer.isLooping     = true;
        vidPlayer.audioOutputMode = VideoAudioOutputMode.None;
        var clip = AssetDatabase.LoadAssetAtPath<VideoClip>(VID);
        if (clip != null) vidPlayer.clip = clip;
        else Debug.LogWarning("[MenuSetup] Video not found: " + VID);

        // A thin dark overlay so panels stand out even if video is bright
        var overlayGO = MakeGO(canvasGO.transform, "BgOverlay");
        Stretch(overlayGO.RT());
        var overlayImg = overlayGO.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.08f);

        // ── PageContainer (full screen minus bottom nav) ──────────────────────
        var pageContainerGO = MakeGO(canvasGO.transform, "PageContainer");
        var pageRT = pageContainerGO.RT();
        pageRT.anchorMin = new Vector2(0, 0);
        pageRT.anchorMax = new Vector2(1, 1);
        pageRT.offsetMin = new Vector2(0, 150);  // leave room for nav bar
        pageRT.offsetMax = Vector2.zero;

        // ── Minijuegos page ───────────────────────────────────────────────────
        var miniPage = MakeGO(pageContainerGO.transform, "MinijuegosPage");
        Stretch(miniPage.RT());

        // ── Hero panel — all children explicitly positioned, NO inner layout groups ──
        // Explicit anchors mean widths/heights are always correct regardless of
        // when CanvasScaler or layout passes run.
        const float HERO_H   = 360f;
        const float LEFT_X   =  40f;   // left edge margin
        const float BADGE_H  =  58f;

        var heroGO = MakeGO(miniPage.transform, "HeroPanel");
        var heroRT = heroGO.RT();
        heroRT.anchorMin        = new Vector2(0, 1);
        heroRT.anchorMax        = new Vector2(1, 1);
        heroRT.pivot            = new Vector2(0.5f, 1);
        heroRT.anchoredPosition = Vector2.zero;
        heroRT.sizeDelta        = new Vector2(0, HERO_H);
        var heroBg = heroGO.AddComponent<Image>();
        heroBg.color = UIStyleKit.HeroPanelBg;

        // Mascot — right side, vertically centred, large
        var mascotImgGO = MakeGO(heroGO.transform, "MascotImage");
        var mascotRT    = mascotImgGO.RT();
        mascotRT.anchorMin        = new Vector2(1, 0.5f);
        mascotRT.anchorMax        = new Vector2(1, 0.5f);
        mascotRT.pivot            = new Vector2(1f,  0.5f);
        mascotRT.anchoredPosition = new Vector2(-20f, 0f);
        mascotRT.sizeDelta        = new Vector2(280f, 280f);
        var mascotImg = mascotImgGO.AddComponent<Image>();
        mascotImg.preserveAspect = true;
        mascotImg.sprite = LoadFrame(IMG + "nutral.png", 0);
        mascotImgGO.AddComponent<SpriteAnimator>();

        // Streak badge — top-left
        var streakBadgeGO = MakeGO(heroGO.transform, "StreakBadge");
        PlaceLeft(streakBadgeGO.RT(), LEFT_X, -50f, 210f, BADGE_H);
        var streakBadgeImg = streakBadgeGO.AddComponent<Image>();
        UIStyleKit.ApplyBadge(streakBadgeImg, UIStyleKit.GoldStreak);
        var streakHLG = streakBadgeGO.AddComponent<HorizontalLayoutGroup>();
        streakHLG.childForceExpandHeight = true;
        streakHLG.childForceExpandWidth  = false;
        streakHLG.padding   = new RectOffset(14, 14, 0, 0);
        streakHLG.spacing   = 8;
        streakHLG.childAlignment = TextAnchor.MiddleLeft;

        var rachaIcon = MakeImage(streakBadgeGO.transform, "RachaIcon", 28, 28);
        rachaIcon.sprite = LoadSingle(IMG + "racha.png");
        rachaIcon.preserveAspect = true;
        rachaIcon.gameObject.AddComponent<LayoutElement>().preferredWidth = 28;

        var streakTMP = MakeTMP(streakBadgeGO.transform, "StreakText", "5",
            36, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);
        streakTMP.gameObject.AddComponent<LayoutElement>().preferredWidth = 48;

        var daysTMP = MakeTMP(streakBadgeGO.transform, "DaysLabel", " días",
            24, FontStyles.Normal, new Color(1f,1f,1f,0.85f), TextAlignmentOptions.Left);
        daysTMP.gameObject.AddComponent<LayoutElement>().preferredWidth = 60;

        // Stars badge — below streak
        var starsBadgeGO = MakeGO(heroGO.transform, "StarsBadge");
        PlaceLeft(starsBadgeGO.RT(), LEFT_X, -128f, 230f, BADGE_H);
        var starsBadgeImg = starsBadgeGO.AddComponent<Image>();
        UIStyleKit.ApplyBadge(starsBadgeImg, UIStyleKit.BlueStars);
        var starsHLG = starsBadgeGO.AddComponent<HorizontalLayoutGroup>();
        starsHLG.childForceExpandHeight = true;
        starsHLG.childForceExpandWidth  = false;
        starsHLG.padding   = new RectOffset(14, 14, 0, 0);
        starsHLG.spacing   = 8;
        starsHLG.childAlignment = TextAnchor.MiddleLeft;

        var starIcon = MakeImage(starsBadgeGO.transform, "StarIcon", 28, 28);
        starIcon.sprite = LoadSingle(IMG + "star.png");
        starIcon.gameObject.AddComponent<LayoutElement>().preferredWidth = 28;

        var starsTMP = MakeTMP(starsBadgeGO.transform, "StarsText", "1250",
            36, FontStyles.Bold, Color.white, TextAlignmentOptions.Left);
        starsTMP.gameObject.AddComponent<LayoutElement>().preferredWidth = 130;

        // Morning / Evening mission chips — row below stars
        var morningChipGO = MakeGO(heroGO.transform, "MorningChip");
        PlaceLeft(morningChipGO.RT(), LEFT_X, -210f, 170f, 48f);
        var morningLbl = BuildChip(morningChipGO, IMG + "sun.png",  "Mañana");

        var eveningChipGO = MakeGO(heroGO.transform, "EveningChip");
        PlaceLeft(eveningChipGO.RT(), LEFT_X + 186f, -210f, 160f, 48f);
        var eveningLbl = BuildChip(eveningChipGO, IMG + "moon.png", "Tarde");

        // Hero message — bottom-left
        var msgGO = MakeGO(heroGO.transform, "HeroMessage");
        var msgRT = msgGO.RT();
        msgRT.anchorMin        = new Vector2(0, 0);
        msgRT.anchorMax        = new Vector2(0.65f, 0);
        msgRT.pivot            = new Vector2(0, 0);
        msgRT.anchoredPosition = new Vector2(LEFT_X, 24f);
        msgRT.sizeDelta        = new Vector2(0, 44f);
        var msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
        msgTMP.text = "¡Sigue cepillando cada día!";
        msgTMP.fontSize = 28f; msgTMP.fontStyle = FontStyles.Normal;
        msgTMP.color = UIStyleKit.TextSecDark;
        msgTMP.alignment = TextAlignmentOptions.Left;
        msgTMP.enableWordWrapping = false;
        msgTMP.overflowMode = TextOverflowModes.Ellipsis;

        // MascotController wired later (after variable set)
        var mascotCtrl = mascotImgGO.AddComponent<MascotController>();

        // Dummy vars so existing wiring code below still compiles
        TextMeshProUGUI morningLbl2 = morningLbl, eveningLbl2 = eveningLbl;

        // ── Scroll section (below hero panel) ────────────────────────────────
        var scrollSectionGO = MakeGO(miniPage.transform, "ScrollSection");
        var scrollSectionRT = scrollSectionGO.RT();
        scrollSectionRT.anchorMin = Vector2.zero;
        scrollSectionRT.anchorMax = Vector2.one;
        scrollSectionRT.offsetMin = new Vector2(0, 0);
        scrollSectionRT.offsetMax = new Vector2(0, -360);

        // ScrollRect
        var scrollGO = MakeGO(scrollSectionGO.transform, "ScrollRect");
        Stretch(scrollGO.RT());
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical   = true;
        scrollRect.scrollSensitivity = 30f;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        // No scrollbar needed for clean look

        // No opaque background — let the video show through; viewport Image handles raycasts.

        // Viewport
        var viewportGO = MakeGO(scrollGO.transform, "Viewport");
        Stretch(viewportGO.RT());
        var viewportImg = viewportGO.AddComponent<Image>();
        viewportImg.color = Color.white;
        viewportGO.AddComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = viewportGO.RT();

        // Content — explicit height, NO VerticalLayoutGroup.
        // Cards use stretch anchors so their width is always = parent.width,
        // independent of the VLG layout pass timing relative to CanvasScaler.
        var contentGO = MakeGO(viewportGO.transform, "Content");
        var contentRT = contentGO.RT();
        contentRT.anchorMin        = new Vector2(0, 1);
        contentRT.anchorMax        = new Vector2(1, 1);
        contentRT.pivot            = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        // Edit-mode fallback; ContentAutoHeight overrides this at runtime.
        contentRT.sizeDelta = new Vector2(0, 1356f);
        scrollRect.content  = contentRT;
        contentGO.AddComponent<ContentAutoHeight>();

        // Section header — stretch X, 24 px from top, 28 px side margins
        var sectionTitle = MakeGO(contentGO.transform, "SectionTitle");
        var stRT = sectionTitle.RT();
        stRT.anchorMin        = new Vector2(0, 1);
        stRT.anchorMax        = new Vector2(1, 1);
        stRT.pivot            = new Vector2(0.5f, 1);
        stRT.anchoredPosition = new Vector2(0, -24f);
        stRT.sizeDelta        = new Vector2(-56f, 60f);
        var sectionTMP = sectionTitle.AddComponent<TextMeshProUGUI>();
        sectionTMP.text      = "Minijuegos";
        sectionTMP.fontSize  = 48;
        sectionTMP.fontStyle = FontStyles.Bold;
        sectionTMP.color     = UIStyleKit.TextOnLight;
        sectionTMP.alignment = TextAlignmentOptions.Left;

        // Game cards — each wrap uses stretch anchors so width = content.width - 56 always
        // Positions: y = -(topPad + titleH + spacing + index*(cardH+spacing))
        const float TOP_PAD = 24f, TITLE_H = 60f, SPACING = 24f, CARD_H = 380f;
        float firstCardY = -(TOP_PAD + TITLE_H + SPACING);
        BuildGameCard(contentGO.transform, "ARCard",    "Cepillo AR",
            "Detecta bacterias con realidad aumentada",
            LoadSingle(IMG + "thumbnail_ar.png"), "ARGame",
            firstCardY, new Color(0.10f, 0.45f, 0.80f, 1f));
        BuildGameCard(contentGO.transform, "BrushCard", "Cepillo Guiado",
            "Aprende la técnica correcta de cepillado",
            LoadSingle(IMG + "thumbnail_brushing_game.png"), "BrushingGame",
            firstCardY - (CARD_H + SPACING), new Color(0.18f, 0.68f, 0.42f, 1f));
        BuildGameCard(contentGO.transform, "FlossCard", "Hilo Dental",
            "Practica el uso del hilo dental",
            LoadSingle(IMG + "thumbnail_dental_floss.png"), "DentalFlossGame",
            firstCardY - 2 * (CARD_H + SPACING), new Color(0.52f, 0.28f, 0.86f, 1f));

        // ── Calendario page ───────────────────────────────────────────────────
        var calPage = MakeGO(pageContainerGO.transform, "CalendarioPage");
        Stretch(calPage.RT());
        calPage.SetActive(false);

        // Calendar header panel
        var calHeaderGO = MakeGO(calPage.transform, "CalHeader");
        var calHeaderRT = calHeaderGO.RT();
        calHeaderRT.anchorMin        = new Vector2(0, 1);
        calHeaderRT.anchorMax        = new Vector2(1, 1);
        calHeaderRT.pivot            = new Vector2(0.5f, 1);
        calHeaderRT.anchoredPosition = Vector2.zero;
        calHeaderRT.sizeDelta        = new Vector2(0, 140);
        var calHeaderBg = calHeaderGO.AddComponent<Image>();
        calHeaderBg.color = UIStyleKit.HeroPanelBg;
        var calHdrLayout = calHeaderGO.AddComponent<HorizontalLayoutGroup>();
        calHdrLayout.childForceExpandHeight = true;
        calHdrLayout.childForceExpandWidth  = false;
        calHdrLayout.padding  = new RectOffset(24, 24, 16, 16);
        calHdrLayout.spacing  = 16;
        calHdrLayout.childAlignment = TextAnchor.MiddleCenter;

        var prevBtn = MakeNavArrow(calHeaderGO.transform, "PrevBtn", "‹");
        var monthLbl = MakeTMP(calHeaderGO.transform, "MonthLabel", "Abril 2026",
            52, FontStyles.Bold, UIStyleKit.TextOnDark, TextAlignmentOptions.Center);
        monthLbl.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        var nextBtn = MakeNavArrow(calHeaderGO.transform, "NextBtn", "›");

        // Calendar legend
        var legendGO = MakeGO(calPage.transform, "CalLegend");
        var legendRT = legendGO.RT();
        legendRT.anchorMin        = new Vector2(0, 1);
        legendRT.anchorMax        = new Vector2(1, 1);
        legendRT.pivot            = new Vector2(0.5f, 1);
        legendRT.anchoredPosition = new Vector2(0, -140);
        legendRT.sizeDelta        = new Vector2(0, 64);
        var legendLayout = legendGO.AddComponent<HorizontalLayoutGroup>();
        legendLayout.childForceExpandHeight = false;
        legendLayout.childForceExpandWidth  = false;
        legendLayout.padding  = new RectOffset(28, 28, 12, 12);
        legendLayout.spacing  = 20;
        legendLayout.childAlignment = TextAnchor.MiddleLeft;
        BuildLegendChip(legendGO.transform, new Color(0.22f,0.80f,0.40f), "≥80%");
        BuildLegendChip(legendGO.transform, new Color(1f,0.82f,0.18f),    "≥50%");
        BuildLegendChip(legendGO.transform, new Color(1f,0.52f,0.10f),    ">0%");
        BuildLegendChip(legendGO.transform, new Color(0.88f,0.22f,0.22f), "Perdido");
        BuildLegendChip(legendGO.transform, UIStyleKit.Accent,             "Hoy");

        // Calendar grid background
        var calBgGO = MakeGO(calPage.transform, "CalGridBg");
        var calBgRT = calBgGO.RT();
        calBgRT.anchorMin = Vector2.zero;
        calBgRT.anchorMax = Vector2.one;
        calBgRT.offsetMin = new Vector2(0, 0);
        calBgRT.offsetMax = new Vector2(0, -204);
        var calBgImg = calBgGO.AddComponent<Image>();
        calBgImg.color = new Color(0.93f, 0.95f, 0.98f, 0.92f);

        // Grid
        var gridGO = MakeGO(calPage.transform, "CalendarGrid");
        var gridRT = gridGO.RT();
        gridRT.anchorMin = Vector2.zero;
        gridRT.anchorMax = Vector2.one;
        gridRT.offsetMin = new Vector2(20, 16);
        gridRT.offsetMax = new Vector2(-20, -210);
        var glg = gridGO.AddComponent<GridLayoutGroup>();
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 7;
        glg.cellSize        = new Vector2(138, 104);
        glg.spacing         = new Vector2(6, 6);
        glg.padding         = new RectOffset(4, 4, 4, 4);
        glg.startCorner     = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis       = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment  = TextAnchor.UpperLeft;

        // ── Bottom nav bar ────────────────────────────────────────────────────
        var navBarGO = MakeGO(canvasGO.transform, "BottomNavBar");
        var navRT    = navBarGO.RT();
        navRT.anchorMin = new Vector2(0, 0);
        navRT.anchorMax = new Vector2(1, 0);
        navRT.pivot     = new Vector2(0.5f, 0);
        navRT.anchoredPosition = Vector2.zero;
        navRT.sizeDelta = new Vector2(0, 150);
        var navBg = navBarGO.AddComponent<Image>();
        navBg.color = UIStyleKit.NavBg;

        // Top shadow strip on nav bar
        var navShadowGO = MakeGO(navBarGO.transform, "NavShadow");
        var navShadowRT = navShadowGO.RT();
        navShadowRT.anchorMin = new Vector2(0, 1);
        navShadowRT.anchorMax = new Vector2(1, 1);
        navShadowRT.pivot     = new Vector2(0.5f, 1);
        navShadowRT.anchoredPosition = Vector2.zero;
        navShadowRT.sizeDelta = new Vector2(0, 4);
        var navShadowImg = navShadowGO.AddComponent<Image>();
        navShadowImg.color = UIStyleKit.NavShadow;

        // Nav layout
        var navLayout = navBarGO.AddComponent<HorizontalLayoutGroup>();
        navLayout.childForceExpandWidth  = true;
        navLayout.childForceExpandHeight = true;
        navLayout.padding  = new RectOffset(0, 0, 8, 16);
        navLayout.spacing  = 0;
        navLayout.childAlignment = TextAnchor.MiddleCenter;

        // Nav buttons
        var (navMiniGO, navMiniLabel, navMiniIndicator) = BuildNavButton(navBarGO.transform, "NavMinijuegos", "⚡", "Juegos",   true);
        var (navCalGO,  navCalLabel,  navCalIndicator)  = BuildNavButton(navBarGO.transform, "NavCalendario", "📅", "Progreso", false);

        // ── Wire components ───────────────────────────────────────────────────

        // CalendarView
        var calView = gridGO.AddComponent<CalendarView>();
        var calSO   = new SerializedObject(calView);
        calSO.Update();
        SetRef(calSO, "monthLabel", monthLbl);
        SetRef(calSO, "gridParent", gridGO.transform);
        calSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(gridGO);

        UnityEventTools.AddPersistentListener(prevBtn.onClick, calView.PrevMonth);
        UnityEventTools.AddPersistentListener(nextBtn.onClick, calView.NextMonth);

        // MascotController — already added in hero panel section; just wire frames
        WireMascotFrames(mascotCtrl);

        // BottomNavController
        var navCtrl = navBarGO.AddComponent<BottomNavController>();
        var navCtrlSO = new SerializedObject(navCtrl);
        navCtrlSO.Update();
        SetRef(navCtrlSO, "minijuegosPage",     miniPage);
        SetRef(navCtrlSO, "calendarioPage",     calPage);
        SetRef(navCtrlSO, "labelMinijuegos",    navMiniLabel);
        SetRef(navCtrlSO, "labelCalendario",    navCalLabel);
        SetRef(navCtrlSO, "indicatorMinijuegos", navMiniIndicator);
        SetRef(navCtrlSO, "indicatorCalendario",  navCalIndicator);
        navCtrlSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(navBarGO);

        // Wire nav buttons
        var miniBtnComp = navMiniGO.GetComponent<Button>();
        var calBtnComp  = navCalGO.GetComponent<Button>();
        if (miniBtnComp != null)
            UnityEventTools.AddPersistentListener(miniBtnComp.onClick, navCtrl.ShowMinijuegos);
        if (calBtnComp  != null)
            UnityEventTools.AddPersistentListener(calBtnComp.onClick,  navCtrl.ShowCalendario);

        // MenuManager
        var menuMgr = canvasGO.AddComponent<MenuManager>();
        var mgSO    = new SerializedObject(menuMgr);
        mgSO.Update();
        SetRef(mgSO, "streakText",   FindDeepTMP(canvasGO.transform, "StreakText"));
        SetRef(mgSO, "starsText",    FindDeepTMP(canvasGO.transform, "StarsText"));
        SetRef(mgSO, "morningLabel", morningLbl2);
        SetRef(mgSO, "eveningLabel", eveningLbl2);
        SetRef(mgSO, "heroMessage",  FindDeepTMP(canvasGO.transform, "HeroMessage"));
        SetRef(mgSO, "mascot",       mascotCtrl);
        SetRef(mgSO, "calendar",     calView);
        SetRef(mgSO, "videoDisplay", rawImg);
        SetRef(mgSO, "videoPlayer",  vidPlayer);
        mgSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(canvasGO);

        // Force layout rebuild so ContentSizeFitter calculates scroll content height
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(canvasGO.GetComponent<RectTransform>());

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        Debug.Log("[MenuSetup] Scene saved → " + SCENE_PATH);
    }

    // ── Card builder ──────────────────────────────────────────────────────────

    static void BuildGameCard(Transform parent, string name, string title, string desc,
                              Sprite thumb, string scene, float yPos, Color headerColor)
    {
        // Wrapper: anchor-based width, no VLG. Width = parent - 56px margins always correct.
        var wrapGO = MakeGO(parent, name + "_Wrap");
        var wrapRT = wrapGO.RT();
        wrapRT.anchorMin        = new Vector2(0, 1);
        wrapRT.anchorMax        = new Vector2(1, 1);
        wrapRT.pivot            = new Vector2(0.5f, 1);
        wrapRT.anchoredPosition = new Vector2(0, yPos);
        wrapRT.sizeDelta        = new Vector2(-56f, 380f);

        // Shadow
        var shadowGO = MakeGO(wrapGO.transform, "Shadow");
        var shadowRT = shadowGO.RT();
        shadowRT.anchorMin = Vector2.zero; shadowRT.anchorMax = Vector2.one;
        shadowRT.offsetMin = new Vector2(8, -16); shadowRT.offsetMax = new Vector2(-8, 8);
        shadowGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        // Card: Mask + white Image.  ApplyCard (runtime) sets rounded sprite → children clip to it.
        var cardGO = MakeGO(wrapGO.transform, name);
        Stretch(cardGO.RT());
        var cardBg = cardGO.AddComponent<Image>();
        cardBg.color = Color.white;

        var btn = cardGO.AddComponent<Button>();
        btn.targetGraphic = cardBg;
        var btnColors = btn.colors;
        btnColors.highlightedColor = new Color(0.94f, 0.94f, 0.96f);
        btnColors.pressedColor     = new Color(0.88f, 0.88f, 0.92f);
        btn.colors = btnColors;

        // Colored header — top 68% of card (260 / 380).  All sizes are anchor-based.
        const float FOOTER_FRAC = 120f / 380f;          // ≈ 0.3158
        var topGO = MakeGO(cardGO.transform, "ColoredTop");
        var topRT = topGO.RT();
        topRT.anchorMin = new Vector2(0, FOOTER_FRAC);
        topRT.anchorMax = Vector2.one;
        topRT.offsetMin = topRT.offsetMax = Vector2.zero;
        topGO.AddComponent<Image>().color = headerColor;

        // Thumbnail fills the header completely (no aspect preserve — acts like a cover photo)
        var thumbGO = MakeGO(topGO.transform, "Thumbnail");
        Stretch(thumbGO.RT());
        var thumbImg = thumbGO.AddComponent<Image>();
        thumbImg.sprite         = thumb;
        thumbImg.preserveAspect = false;
        thumbImg.color          = Color.white;

        // Footer — bottom 32% of card
        var footerGO = MakeGO(cardGO.transform, "Footer");
        var footerRT = footerGO.RT();
        footerRT.anchorMin = Vector2.zero;
        footerRT.anchorMax = new Vector2(1, FOOTER_FRAC);
        footerRT.offsetMin = footerRT.offsetMax = Vector2.zero;
        footerGO.AddComponent<Image>().color = Color.white;

        // Title: top half, left 65%
        var titleGO = MakeGO(footerGO.transform, "CardTitle");
        var titleRT = titleGO.RT();
        titleRT.anchorMin = new Vector2(0, 0.52f);
        titleRT.anchorMax = new Vector2(0.65f, 1f);
        titleRT.offsetMin = new Vector2(24, 0); titleRT.offsetMax = new Vector2(0, -8);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = title;   titleTMP.fontSize  = 38;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color     = UIStyleKit.TextOnLight;
        titleTMP.alignment = TextAlignmentOptions.Left;
        titleTMP.overflowMode = TextOverflowModes.Ellipsis;
        titleTMP.enableWordWrapping = false;

        // Description: bottom half, left 65%
        var descGO = MakeGO(footerGO.transform, "CardDesc");
        var descRT = descGO.RT();
        descRT.anchorMin = new Vector2(0, 0); descRT.anchorMax = new Vector2(0.65f, 0.5f);
        descRT.offsetMin = new Vector2(24, 6); descRT.offsetMax = Vector2.zero;
        var descTMP = descGO.AddComponent<TextMeshProUGUI>();
        descTMP.text      = desc;  descTMP.fontSize  = 24;
        descTMP.fontStyle = FontStyles.Normal;
        descTMP.color     = UIStyleKit.TextSecLight;
        descTMP.alignment = TextAlignmentOptions.Left;
        descTMP.overflowMode       = TextOverflowModes.Ellipsis;
        descTMP.enableWordWrapping = false;

        // Play button: right 35%, vertically centred — large enough for rounded sprite to look good
        var playGO = MakeGO(footerGO.transform, "PlayBtn");
        var playRT = playGO.RT();
        playRT.anchorMin = new Vector2(0.65f, 0.12f);
        playRT.anchorMax = new Vector2(1f,    0.88f);
        playRT.offsetMin = new Vector2(8,  0);
        playRT.offsetMax = new Vector2(-20, 0);
        var playImg = playGO.AddComponent<Image>();
        playImg.color = UIStyleKit.Accent;

        var playLbl = MakeTMP(playGO.transform, "PlayLabel", "Jugar",
            30, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        Stretch(playLbl.GetComponent<RectTransform>());

        var cardUI = cardGO.AddComponent<GameCardUI>();
        var so = new SerializedObject(cardUI);
        so.Update();
        so.FindProperty("sceneName").stringValue = scene;
        SetRef(so, "cardBackground", cardBg);
        SetRef(so, "thumbnail", thumbImg);
        SetRef(so, "playBtnBg", playImg);
        so.ApplyModifiedProperties();
    }

    // ── Minor builders ────────────────────────────────────────────────────────

    // PlaceLeft: anchor to top-left corner, explicit position & size.
    // x/y are offset from top-left of parent (y negative = downward).
    static void PlaceLeft(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin        = new Vector2(0, 1);
        rt.anchorMax        = new Vector2(0, 1);
        rt.pivot            = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta        = new Vector2(w, h);
    }

    // BuildChip: inline chip for hero panel (no LayoutElement needed since parent has explicit pos)
    static TextMeshProUGUI BuildChip(GameObject chipGO, string iconPath, string label)
    {
        var chipImg = chipGO.AddComponent<Image>();
        chipImg.color = new Color(1f, 1f, 1f, 0.15f);
        var hlg = chipGO.AddComponent<HorizontalLayoutGroup>();
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth  = false;
        hlg.padding   = new RectOffset(10, 12, 0, 0);
        hlg.spacing   = 6;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        var icon = MakeImage(chipGO.transform, "Icon", 24, 24);
        icon.sprite = LoadSingle(iconPath);
        icon.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;
        return MakeTMP(chipGO.transform, label + "Label", label,
            26, FontStyles.Normal, UIStyleKit.TextOnDark, TextAlignmentOptions.Left);
    }

    static void BuildLegendChip(Transform parent, Color color, string label)
    {
        var go = MakeGO(parent, "Chip_" + label);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 40; le.preferredWidth = 120;
        var layout = go.AddComponent<HorizontalLayoutGroup>();
        layout.childForceExpandHeight = true;
        layout.padding  = new RectOffset(8, 10, 4, 4);
        layout.spacing  = 6;
        layout.childAlignment = TextAnchor.MiddleLeft;

        var dot = MakeGO(go.transform, "Dot");
        var dotLE = dot.AddComponent<LayoutElement>();
        dotLE.preferredWidth = 16; dotLE.preferredHeight = 16;
        var dotImg = dot.AddComponent<Image>();
        dotImg.color = color;

        MakeTMP(go.transform, "Label", label,
            22, FontStyles.Normal, UIStyleKit.TextSecLight, TextAlignmentOptions.Left);
    }

    static (GameObject go, TextMeshProUGUI label, Image indicator) BuildNavButton(
        Transform parent, string name, string iconChar, string labelText, bool active)
    {
        var go = MakeGO(parent, name);
        go.AddComponent<LayoutElement>().flexibleWidth = 1;
        var bg  = go.AddComponent<Image>();
        bg.color = Color.clear;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;

        var layout = go.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 2;
        layout.padding = new RectOffset(0, 0, 8, 0);

        var color = active ? UIStyleKit.NavActive : UIStyleKit.NavInactive;

        // Icon (emoji via TMP)
        var iconGO = MakeGO(go.transform, "Icon");
        iconGO.AddComponent<LayoutElement>().preferredHeight = 46;
        var iconTMP = iconGO.AddComponent<TextMeshProUGUI>();
        iconTMP.text      = iconChar;
        iconTMP.fontSize  = 38;
        iconTMP.alignment = TextAlignmentOptions.Center;
        iconTMP.color     = color;

        // Label
        var lbl = MakeTMP(go.transform, "Label", labelText,
            26, FontStyles.Bold, color, TextAlignmentOptions.Center);
        lbl.gameObject.AddComponent<LayoutElement>().preferredHeight = 34;

        // Active indicator bar (thin, below label)
        var indGO  = MakeGO(go.transform, "Indicator");
        indGO.AddComponent<LayoutElement>().preferredHeight = 4;
        var indImg = indGO.AddComponent<Image>();
        indImg.color = active ? UIStyleKit.NavActive : new Color(0, 0, 0, 0);

        return (go, lbl, indImg);
    }

    static Button MakeNavArrow(Transform parent, string name, string symbol)
    {
        var go = MakeGO(parent, name);
        var le  = go.AddComponent<LayoutElement>();
        le.preferredWidth = 88; le.preferredHeight = 88;
        var bg  = go.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.10f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;

        var lbl = MakeTMP(go.transform, "Lbl", symbol,
            60, FontStyles.Bold, UIStyleKit.TextOnDark, TextAlignmentOptions.Center);
        var lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        return btn;
    }

    static Transform MakeHorizRow(Transform parent, string name, int spacing)
    {
        var go = MakeGO(parent, name);
        go.AddComponent<LayoutElement>().preferredHeight = 64;
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth  = false;
        hlg.spacing = spacing;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        return go.transform;
    }

    static void WireMascotFrames(MascotController ctrl)
    {
        var so = new SerializedObject(ctrl);
        so.Update();
        WireArray(so, "neutralFrames",  LoadSheetFrames(IMG + "nutral.png"));
        WireArray(so, "happyFrames",    LoadSheetFrames(IMG + "saludating.png"));
        WireArray(so, "cheeringFrames", LoadSheetFrames(IMG + "cheering.png"));
        WireArray(so, "sadFrames",      LoadSheetFrames(IMG + "sad.png"));
        so.ApplyModifiedProperties();
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    static GameObject MakeGO(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    // RT() just retrieves — MakeGO always guarantees it exists
    static RectTransform RT(this GameObject go) =>
        go.GetComponent<RectTransform>();

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static Image MakeImage(Transform parent, string name, float w, float h)
    {
        var go = MakeGO(parent, name);
        go.RT().sizeDelta = new Vector2(w, h);
        var img = go.AddComponent<Image>();
        img.preserveAspect = true;
        return img;
    }

    static TextMeshProUGUI MakeTMP(Transform parent, string name, string text,
        float size, FontStyles style, Color color, TextAlignmentOptions align)
    {
        var go = MakeGO(parent, name);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.fontStyle = style; tmp.color = color; tmp.alignment = align;
        return tmp;
    }

    static Sprite[] LoadSheetFrames(string path)
    {
        var all  = AssetDatabase.LoadAllAssetsAtPath(path);
        var list = new List<Sprite>();
        foreach (var a in all) if (a is Sprite s) list.Add(s);
        list.Sort((a, b) => FrameIdx(a.name).CompareTo(FrameIdx(b.name)));
        return list.ToArray();
    }

    static int FrameIdx(string name)
    {
        int i = name.LastIndexOf('_');
        return i >= 0 && int.TryParse(name.Substring(i + 1), out int n) ? n : 0;
    }

    static Sprite LoadSingle(string path) =>
        AssetDatabase.LoadAssetAtPath<Sprite>(path);

    static Sprite LoadFrame(string path, int idx)
    {
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
            if (a is Sprite s && s.name.EndsWith("_" + idx)) return s;
        foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
            if (a is Sprite sp) return sp;
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static TextMeshProUGUI FindDeepTMP(Transform root, string name)
    {
        if (root.name == name) return root.GetComponent<TextMeshProUGUI>();
        foreach (Transform c in root) { var r = FindDeepTMP(c, name); if (r != null) return r; }
        return null;
    }

    static void SetRef(SerializedObject so, string prop, Object obj)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = obj;
        else Debug.LogWarning("[MenuSetup] prop not found: " + prop);
    }

    static void WireArray(SerializedObject so, string prop, Sprite[] sprites)
    {
        var arr = so.FindProperty(prop);
        if (arr == null) { Debug.LogWarning("[MenuSetup] array not found: " + prop); return; }
        arr.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    }
}

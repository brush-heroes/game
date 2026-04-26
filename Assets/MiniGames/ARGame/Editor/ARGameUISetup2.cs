using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

/// Full UI rebuild for BrushHeroes AR game.
/// Run via: Tools ▶ AR Game ▶ Setup Full UI
/// Creates StartScreen, GameHUD (TopBar with score + zone dots + timer), EndScreen.
/// Also resizes the GuidePanel to 1.8×, hides zone spheres, and offsets BrushVisual.
public class ARGameUISetup2
{
    const string GUIDE_BASE = "Assets/MiniGames/ARGame/Assets/mouth_guide/";

    // ── Entry point ──────────────────────────────────────────────────────────

    [MenuItem("Tools/AR Game/Setup Full UI")]
    public static void Execute()
    {
        var canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[ARGameUISetup2] No Canvas found in scene."); return; }

        if (canvas.transform.Find("StartScreen") != null)
        {
            Debug.Log("[ARGameUISetup2] StartScreen already exists. Delete it to re-run.");
            return;
        }

        var sm  = UnityEngine.Object.FindObjectOfType<SessionManager>();
        var scm = UnityEngine.Object.FindObjectOfType<ScoreManager>();
        var mzm = UnityEngine.Object.FindObjectOfType<MouthZoneManager>();

        // 1. Resize GuidePanel to 1.8× (270 → 486)
        ResizeGuidePanel(canvas.transform);

        // 2. Remove old StartButton / BackButton from GuidePanel
        RemoveOldButtons(canvas.transform);

        // 3. Disable legacy score/instruction text (replaced by new HUD)
        DisableLegacyElements(canvas.transform);

        // 4. Create UIManager with GameScreenManager
        var uiManagerGO = NewGO("UIManager", canvas.transform);
        var gsm = uiManagerGO.AddComponent<GameScreenManager>();
        WireRef(gsm, "sessionManager", sm);
        WireRef(gsm, "scoreManager",   scm);
        WireRef(gsm, "mouthZoneManager", mzm);
        AssignFrameArray(gsm, "completionFrames", GUIDE_BASE + "completion.png", 4);
        AssignFrameArray(gsm, "failedFrames",     GUIDE_BASE + "failed.png",     4);

        // 5. Create screens
        var startScreenGO = CreateStartScreen(canvas.transform);
        var gameHUDGO     = CreateGameHUD(canvas.transform);
        var endScreenGO   = CreateEndScreen(canvas.transform);

        // 6. Wire GameScreenManager screen references
        var gsmSO = new SerializedObject(gsm);
        SetRef(gsmSO, "startScreen", startScreenGO.GetComponent<CanvasGroup>());
        SetRef(gsmSO, "gameHUD",     gameHUDGO.GetComponent<CanvasGroup>());
        SetRef(gsmSO, "endScreen",   endScreenGO.GetComponent<CanvasGroup>());

        var resultAnimGO = endScreenGO.transform.Find("ResultAnimation");
        if (resultAnimGO != null) SetRef(gsmSO, "resultImage", resultAnimGO.GetComponent<Image>());

        var titleGO = endScreenGO.transform.Find("ResultTitle");
        if (titleGO != null) SetRef(gsmSO, "resultTitle", titleGO.GetComponent<TMP_Text>());

        var finalScoreGO = endScreenGO.transform.Find("FinalScoreValue");
        if (finalScoreGO != null) SetRef(gsmSO, "finalScoreText", finalScoreGO.GetComponent<TMP_Text>());

        var msgGO = endScreenGO.transform.Find("ResultMessage");
        if (msgGO != null) SetRef(gsmSO, "resultMessage", msgGO.GetComponent<TMP_Text>());

        var timerValueGO = gameHUDGO.transform.Find("TopBar/TimerPanel/TimerValue");
        if (timerValueGO != null) SetRef(gsmSO, "timerText", timerValueGO.GetComponent<TMP_Text>());

        gsmSO.ApplyModifiedProperties();

        // 7. Wire button persistent listeners
        WireButton(startScreenGO, "StartButton", gsm.OnStartButtonPressed);
        WireButton(endScreenGO,   "MenuButton",  gsm.OnMenuButtonPressed);
        WireButton(endScreenGO,   "RetryButton", gsm.OnRetryButtonPressed);

        // 8. Wire ZoneProgressUI
        var zpUI = gameHUDGO.GetComponentInChildren<ZoneProgressUI>();
        if (zpUI != null)
        {
            var zpSO = new SerializedObject(zpUI);
            SetRef(zpSO, "sessionManager", sm);

            var dotsPanel = gameHUDGO.transform.Find("TopBar/ZoneDotsPanel");
            if (dotsPanel != null)
            {
                var dotsArr = zpSO.FindProperty("dots");
                dotsArr.arraySize = 6;
                for (int i = 0; i < 6; i++)
                {
                    var dot = dotsPanel.Find("Dot" + i);
                    if (dot != null)
                        dotsArr.GetArrayElementAtIndex(i).objectReferenceValue = dot.GetComponent<Image>();
                }
            }
            zpSO.ApplyModifiedProperties();
        }

        // 9. Wire ScoreAnimUI
        var scoreAnim = gameHUDGO.GetComponentInChildren<ScoreAnimUI>();
        if (scoreAnim != null)
        {
            var saSO = new SerializedObject(scoreAnim);
            SetRef(saSO, "scoreManager", scm);
            saSO.ApplyModifiedProperties();
        }

        // 10. Offset BrushVisual so handle sits at finger
        var brushCtrl = UnityEngine.Object.FindObjectOfType<VirtualBrushController>();
        if (brushCtrl != null)
        {
            Transform vis = brushCtrl.transform.Find("BrushVisual");
            if (vis != null)
            {
                vis.localPosition = new Vector3(0f, 0.04f, 0f);
                Debug.Log("[ARGameUISetup2] BrushVisual localPosition.y = 0.04");
            }
        }

        // 11. Disable zone sphere renderers
        if (mzm != null)
        {
            foreach (MouthZone zone in (MouthZone[])Enum.GetValues(typeof(MouthZone)))
            {
                Transform t = mzm.GetZoneTransform(zone);
                if (t == null) continue;
                foreach (Renderer r in t.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;
            }
            Debug.Log("[ARGameUISetup2] Zone sphere renderers hidden.");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[ARGameUISetup2] Full UI setup complete! Save scene (Ctrl+S).");
    }

    // ── Resize existing GuidePanel ───────────────────────────────────────────

    static void ResizeGuidePanel(Transform canvasRoot)
    {
        var guidePanel = canvasRoot.Find("GuidePanel");
        if (guidePanel == null) { Debug.LogWarning("[ARGameUISetup2] GuidePanel not found — run ARGameSetup first."); return; }
        var rt = guidePanel.GetComponent<RectTransform>();
        rt.offsetMax = new Vector2(0f, 486f);
        Debug.Log("[ARGameUISetup2] GuidePanel height set to 486.");
    }

    static void RemoveOldButtons(Transform canvasRoot)
    {
        var guidePanel = canvasRoot.Find("GuidePanel");
        if (guidePanel == null) return;
        foreach (string name in new[] { "StartButton", "BackButton" })
        {
            var child = guidePanel.Find(name);
            if (child != null) { UnityEngine.Object.DestroyImmediate(child.gameObject); Debug.Log("[ARGameUISetup2] Removed " + name + " from GuidePanel."); }
        }
    }

    static void DisableLegacyElements(Transform canvasRoot)
    {
        foreach (string name in new[] { "ScoreText", "InstructionText" })
        {
            var t = canvasRoot.Find(name);
            if (t != null) { t.gameObject.SetActive(false); Debug.Log("[ARGameUISetup2] Deactivated legacy: " + name); }
        }
    }

    // ── StartScreen ──────────────────────────────────────────────────────────

    static GameObject CreateStartScreen(Transform canvasRoot)
    {
        var go = NewGO("StartScreen", canvasRoot);
        go.AddComponent<CanvasGroup>(); // alpha controlled by GameScreenManager
        SetFullScreen(go.GetComponent<RectTransform>());

        // Dark full-screen background
        var bg = NewGO("Background", go.transform);
        bg.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.10f, 0.88f);
        SetFullScreen(bg.GetComponent<RectTransform>());

        // Title
        var title = NewGO("TitleText", go.transform);
        var titleTMP = title.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "BrushHeroes";
        titleTMP.fontSize  = 62f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color     = Color.white;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.raycastTarget = false;
        SetArea(title.GetComponent<RectTransform>(), 0.05f, 0.84f, 0.95f, 0.96f);

        // Mouth preview (default.png centered)
        var preview = NewGO("MouthPreview", go.transform);
        var previewImg = preview.AddComponent<Image>();
        previewImg.preserveAspect = true;
        previewImg.sprite = LoadSprite(GUIDE_BASE + "default.png");
        SetArea(preview.GetComponent<RectTransform>(), 0.18f, 0.46f, 0.82f, 0.84f);

        // Subtitle
        var sub = NewGO("SubtitleText", go.transform);
        var subTMP = sub.AddComponent<TextMeshProUGUI>();
        subTMP.text      = "¡Mantén tus dientes brillantes!";
        subTMP.fontSize  = 30f;
        subTMP.fontStyle = FontStyles.Bold;
        subTMP.color     = new Color(0.85f, 0.95f, 1f);
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.raycastTarget = false;
        SetArea(sub.GetComponent<RectTransform>(), 0.05f, 0.37f, 0.95f, 0.46f);

        // Instructions
        var instr = NewGO("InstructionsText", go.transform);
        var instrTMP = instr.AddComponent<TextMeshProUGUI>();
        instrTMP.text      = "Usa tu dedo para cepillar las bacterias malvadas";
        instrTMP.fontSize  = 22f;
        instrTMP.color     = new Color(0.75f, 0.85f, 0.9f);
        instrTMP.alignment = TextAlignmentOptions.Center;
        instrTMP.raycastTarget = false;
        SetArea(instr.GetComponent<RectTransform>(), 0.08f, 0.29f, 0.92f, 0.37f);

        // Start button (large, green)
        var startBtn = CreateButton("StartButton", go.transform, "¡EMPEZAR!", new Color(0.12f, 0.72f, 0.22f));
        SetArea(startBtn.GetComponent<RectTransform>(), 0.18f, 0.14f, 0.82f, 0.27f);

        return go;
    }

    // ── GameHUD ──────────────────────────────────────────────────────────────

    static GameObject CreateGameHUD(Transform canvasRoot)
    {
        var go = NewGO("GameHUD", canvasRoot);
        var cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false; // HUD never blocks AR touch input
        SetFullScreen(go.GetComponent<RectTransform>());

        // TopBar pinned to top
        var topBar = NewGO("TopBar", go.transform);
        var topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(0.08f, 0.08f, 0.15f, 0.82f);
        var topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0f, 1f);
        topBarRT.anchorMax = new Vector2(1f, 1f);
        topBarRT.pivot     = new Vector2(0.5f, 1f);
        topBarRT.offsetMin = new Vector2(0f, -90f);
        topBarRT.offsetMax = Vector2.zero;

        // Score panel (left third of TopBar)
        var scorePanelGO = NewGO("ScorePanel", topBar.transform);
        SetArea(scorePanelGO.GetComponent<RectTransform>(), 0f, 0f, 0.30f, 1f);

        var scoreLbl = NewGO("ScoreLabel", scorePanelGO.transform);
        var scoreLblTMP = scoreLbl.AddComponent<TextMeshProUGUI>();
        scoreLblTMP.text      = "PUNTOS";
        scoreLblTMP.fontSize  = 16f;
        scoreLblTMP.color     = new Color(0.75f, 0.85f, 1f);
        scoreLblTMP.alignment = TextAlignmentOptions.Center;
        scoreLblTMP.raycastTarget = false;
        SetArea(scoreLbl.GetComponent<RectTransform>(), 0.05f, 0.52f, 0.95f, 0.95f);

        var scoreVal = NewGO("ScoreValue", scorePanelGO.transform);
        var scoreValTMP = scoreVal.AddComponent<TextMeshProUGUI>();
        scoreValTMP.text      = "0";
        scoreValTMP.fontSize  = 32f;
        scoreValTMP.fontStyle = FontStyles.Bold;
        scoreValTMP.color     = new Color(1f, 0.95f, 0.2f);
        scoreValTMP.alignment = TextAlignmentOptions.Center;
        scoreValTMP.raycastTarget = false;
        SetArea(scoreVal.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.95f, 0.52f);
        scoreVal.AddComponent<ScoreAnimUI>(); // wired later

        // Zone dots panel (center third of TopBar)
        var dotsPanel = NewGO("ZoneDotsPanel", topBar.transform);
        SetArea(dotsPanel.GetComponent<RectTransform>(), 0.30f, 0f, 0.70f, 1f);
        var zpUI = dotsPanel.AddComponent<ZoneProgressUI>(); // wired later

        var hLayout = dotsPanel.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing              = 6f;
        hLayout.childAlignment       = TextAnchor.MiddleCenter;
        hLayout.childForceExpandWidth  = false;
        hLayout.childForceExpandHeight = false;
        hLayout.childControlWidth      = false;
        hLayout.childControlHeight     = false;

        for (int i = 0; i < 6; i++)
        {
            var dot = NewGO("Dot" + i, dotsPanel.transform);
            var dotImg = dot.AddComponent<Image>();
            dotImg.color = new Color(0.35f, 0.35f, 0.35f, 0.55f);
            var dotRT = dot.GetComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(18f, 18f);
        }

        // Timer panel (right third of TopBar)
        var timerPanelGO = NewGO("TimerPanel", topBar.transform);
        SetArea(timerPanelGO.GetComponent<RectTransform>(), 0.70f, 0f, 1.0f, 1f);

        var timerLbl = NewGO("TimerLabel", timerPanelGO.transform);
        var timerLblTMP = timerLbl.AddComponent<TextMeshProUGUI>();
        timerLblTMP.text      = "TIEMPO";
        timerLblTMP.fontSize  = 16f;
        timerLblTMP.color     = new Color(0.75f, 0.85f, 1f);
        timerLblTMP.alignment = TextAlignmentOptions.Center;
        timerLblTMP.raycastTarget = false;
        SetArea(timerLbl.GetComponent<RectTransform>(), 0.05f, 0.52f, 0.95f, 0.95f);

        var timerVal = NewGO("TimerValue", timerPanelGO.transform);
        var timerValTMP = timerVal.AddComponent<TextMeshProUGUI>();
        timerValTMP.text      = "180";
        timerValTMP.fontSize  = 32f;
        timerValTMP.fontStyle = FontStyles.Bold;
        timerValTMP.color     = new Color(1f, 0.75f, 0.2f);
        timerValTMP.alignment = TextAlignmentOptions.Center;
        timerValTMP.raycastTarget = false;
        SetArea(timerVal.GetComponent<RectTransform>(), 0.05f, 0.05f, 0.95f, 0.52f);

        return go;
    }

    // ── EndScreen ────────────────────────────────────────────────────────────

    static GameObject CreateEndScreen(Transform canvasRoot)
    {
        var go = NewGO("EndScreen", canvasRoot);
        var cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        SetFullScreen(go.GetComponent<RectTransform>());

        // Dark full-screen background
        var bg = NewGO("Background", go.transform);
        bg.AddComponent<Image>().color = new Color(0.04f, 0.04f, 0.10f, 0.92f);
        SetFullScreen(bg.GetComponent<RectTransform>());

        // Result animation (completion or failed sprites)
        var resultAnimGO = NewGO("ResultAnimation", go.transform);
        var resultImg = resultAnimGO.AddComponent<Image>();
        resultImg.preserveAspect = true;
        resultAnimGO.AddComponent<SpriteAnimator>();
        SetArea(resultAnimGO.GetComponent<RectTransform>(), 0.18f, 0.54f, 0.82f, 0.90f);

        // Result title ("¡Excelente!" / "¡Tiempo agotado!")
        var titleGO = NewGO("ResultTitle", go.transform);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "¡Excelente!";
        titleTMP.fontSize  = 52f;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.color     = Color.white;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.raycastTarget = false;
        SetArea(titleGO.GetComponent<RectTransform>(), 0.05f, 0.44f, 0.95f, 0.54f);

        // Score label
        var scoreLblGO = NewGO("ScoreLabel", go.transform);
        var scoreLblTMP = scoreLblGO.AddComponent<TextMeshProUGUI>();
        scoreLblTMP.text      = "PUNTOS FINALES";
        scoreLblTMP.fontSize  = 22f;
        scoreLblTMP.color     = new Color(0.75f, 0.85f, 1f);
        scoreLblTMP.alignment = TextAlignmentOptions.Center;
        scoreLblTMP.raycastTarget = false;
        SetArea(scoreLblGO.GetComponent<RectTransform>(), 0.15f, 0.35f, 0.85f, 0.44f);

        // Final score value (large yellow)
        var finalScoreGO = NewGO("FinalScoreValue", go.transform);
        var finalScoreTMP = finalScoreGO.AddComponent<TextMeshProUGUI>();
        finalScoreTMP.text      = "0";
        finalScoreTMP.fontSize  = 76f;
        finalScoreTMP.fontStyle = FontStyles.Bold;
        finalScoreTMP.color     = new Color(1f, 0.92f, 0.1f);
        finalScoreTMP.alignment = TextAlignmentOptions.Center;
        finalScoreTMP.raycastTarget = false;
        SetArea(finalScoreGO.GetComponent<RectTransform>(), 0.20f, 0.22f, 0.80f, 0.35f);

        // Result message
        var msgGO = NewGO("ResultMessage", go.transform);
        var msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
        msgTMP.text      = "¡Limpiaste todos tus dientes!";
        msgTMP.fontSize  = 26f;
        msgTMP.color     = new Color(0.88f, 0.92f, 0.95f);
        msgTMP.alignment = TextAlignmentOptions.Center;
        msgTMP.raycastTarget = false;
        SetArea(msgGO.GetComponent<RectTransform>(), 0.06f, 0.14f, 0.94f, 0.22f);

        // Menu button (left)
        var menuBtn = CreateButton("MenuButton", go.transform, "MENÚ", new Color(0.22f, 0.32f, 0.80f));
        SetArea(menuBtn.GetComponent<RectTransform>(), 0.05f, 0.03f, 0.46f, 0.13f);

        // Retry button (right)
        var retryBtn = CreateButton("RetryButton", go.transform, "¡OTRA VEZ!", new Color(0.12f, 0.72f, 0.22f));
        SetArea(retryBtn.GetComponent<RectTransform>(), 0.54f, 0.03f, 0.95f, 0.13f);

        return go;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static GameObject NewGO(string name, Transform parent)
    {
        // Use typeof(RectTransform) in constructor — creates GO with RectTransform directly
        // so empty container nodes (no Image/Text) still have a proper UI transform.
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

    static GameObject CreateButton(string name, Transform parent, string label, Color bg)
    {
        var go = NewGO(name, parent);
        go.AddComponent<Image>().color = bg;
        go.AddComponent<Button>();

        var lblGO = NewGO("Label", go.transform);
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 36f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        SetArea(lblGO.GetComponent<RectTransform>(), 0.04f, 0.08f, 0.96f, 0.92f);

        return go;
    }

    static void WireButton(GameObject screen, string buttonName, UnityEngine.Events.UnityAction callback)
    {
        var btnT = screen.transform.Find(buttonName);
        if (btnT == null) { Debug.LogWarning("[ARGameUISetup2] Button not found: " + buttonName); return; }
        var btn = btnT.GetComponent<Button>();
        if (btn == null) return;
        UnityEventTools.AddPersistentListener(btn.onClick, callback);
    }

    static void SetRef(SerializedObject so, string prop, UnityEngine.Object value)
        => so.FindProperty(prop).objectReferenceValue = value;

    static void WireRef(UnityEngine.Object target, string prop, UnityEngine.Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(prop).objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    static void AssignFrameArray(UnityEngine.Object target, string prop, string texPath, int count)
    {
        var so  = new SerializedObject(target);
        var arr = so.FindProperty(prop);
        arr.arraySize = count;
        for (int i = 0; i < count; i++)
            arr.GetArrayElementAtIndex(i).objectReferenceValue = LoadFrameSprite(texPath, i);
        so.ApplyModifiedProperties();
    }

    static Sprite LoadSprite(string path)
        => AssetDatabase.LoadAssetAtPath<Sprite>(path);

    static Sprite LoadFrameSprite(string texPath, int index)
    {
        string name = Path.GetFileNameWithoutExtension(texPath) + "_" + index;
        foreach (UnityEngine.Object a in AssetDatabase.LoadAllAssetsAtPath(texPath))
            if (a is Sprite s && s.name == name) return s;
        return null;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// Setup step 4: guide panel resize + background removal, top-bar enlarge, guide visibility wiring.
/// Run via: Tools ▶ AR Game ▶ Resize Panels + Wire Guide Visibility
public class ARGameUISetup4
{
    const string SCENE_PATH = "Assets/MiniGames/ARGame/Scenes/ARGame.unity";

    [MenuItem("Tools/AR Game/Resize Panels + Wire Guide Visibility")]
    public static void Execute()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[ARGameUISetup4] No Canvas found."); return; }

        // 1. Guide panel: 1.4× taller (486 → 680) and transparent background
        ResizeGuidePanel(canvas.transform);

        // 2. Top bar: taller (90 → 120 px)
        ResizeTopBar(canvas.transform);

        // 3. Wire GameScreenManager.guidePanel reference
        WireGuidePanel(canvas.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_PATH);
        AssetDatabase.SaveAssets();

        Debug.Log("[ARGameUISetup4] Done — scene saved.");
    }

    // ── 1. Guide panel ───────────────────────────────────────────────────────

    static void ResizeGuidePanel(Transform canvasRoot)
    {
        var guidePanel = canvasRoot.Find("GuidePanel");
        if (guidePanel == null) { Debug.LogWarning("[ARGameUISetup4] GuidePanel not found."); return; }

        // Height 486 × 1.4 = 680
        var rt = guidePanel.GetComponent<RectTransform>();
        rt.offsetMax = new Vector2(0f, 680f);
        Debug.Log("[ARGameUISetup4] GuidePanel height → 680.");

        // Remove dark background (set Image alpha to 0)
        var img = guidePanel.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(0f, 0f, 0f, 0f);
            Debug.Log("[ARGameUISetup4] GuidePanel background set to transparent.");
        }
    }

    // ── 2. Top bar ───────────────────────────────────────────────────────────

    static void ResizeTopBar(Transform canvasRoot)
    {
        var gameHUD = canvasRoot.Find("GameHUD");
        if (gameHUD == null) { Debug.LogWarning("[ARGameUISetup4] GameHUD not found."); return; }

        var topBar = gameHUD.Find("TopBar");
        if (topBar == null) { Debug.LogWarning("[ARGameUISetup4] TopBar not found."); return; }

        var rt = topBar.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(0f, -120f);
        Debug.Log("[ARGameUISetup4] TopBar height → 120 px.");
    }

    // ── 3. Wire guidePanel on GameScreenManager ──────────────────────────────

    static void WireGuidePanel(Transform canvasRoot)
    {
        var gsm = Object.FindObjectOfType<GameScreenManager>();
        if (gsm == null) { Debug.LogWarning("[ARGameUISetup4] GameScreenManager not found."); return; }

        var guidePanel = canvasRoot.Find("GuidePanel");
        if (guidePanel == null) { Debug.LogWarning("[ARGameUISetup4] GuidePanel not found."); return; }

        var so = new SerializedObject(gsm);
        var prop = so.FindProperty("guidePanel");
        if (prop != null)
        {
            prop.objectReferenceValue = guidePanel.gameObject;
            so.ApplyModifiedProperties();
            Debug.Log("[ARGameUISetup4] GameScreenManager.guidePanel wired.");
        }
        else
        {
            Debug.LogWarning("[ARGameUISetup4] guidePanel property not found on GameScreenManager.");
        }
    }
}

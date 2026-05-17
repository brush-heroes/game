using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Re-adds the end screens lost in the recent merge:
///   1. BrushingGame: creates an EndScreenPanel with a "Volver al menú" button
///      that fires after the tongue mini-game (level 2) wins. Wires BrushEndScreen
///      to a GameObject in the scene.
///   2. DentalFlossGame (both Game.unity and DentalFlossGame.unity): adds a
///      "Volver al menú" button to the existing finalPanel of GameManager.
///
/// Usage: in Unity, Tools → BrushHeroes → Add End Screens (Brush + Floss).
/// Or via Coplay MCP: execute this with method "Run".
/// </summary>
public static class AddEndScreens
{
    public static void Run()
    {
        AddBrushEndScreen();
        AddFlossMenuButton("Assets/MiniGames/DentalFlossGame/Scenes/Game.unity");
        AddFlossMenuButton("Assets/MiniGames/DentalFlossGame/Scenes/DentalFlossGame.unity");
    }

    [MenuItem("Tools/BrushHeroes/Add End Screens (Brush + Floss)")]
    public static void RunFromMenu() => Run();

    // ───────────────────────────────────────────────────────────────────────
    // Brushing Game
    // ───────────────────────────────────────────────────────────────────────

    static void AddBrushEndScreen()
    {
        const string scenePath = "Assets/MiniGames/Brushing Games/Scenes/BrushingGame.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        // Find Canvas (any UI canvas in the scene).
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError($"[AddEndScreens] No Canvas found in {scenePath}.");
            return;
        }

        // Skip if already wired.
        if (canvas.transform.Find("EndScreenPanel") != null)
        {
            Debug.Log("[AddEndScreens] EndScreenPanel already exists in BrushingGame — skipping.");
            return;
        }

        // ── EndScreenPanel ──
        var panel = new GameObject("EndScreenPanel",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);

        var panelRT = (RectTransform)panel.transform;
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        var panelImg = panel.GetComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.78f);
        panelImg.raycastTarget = true;

        // ── Title text ──
        var titleGO = new GameObject("Title",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(panel.transform, false);
        var titleRT = (RectTransform)titleGO.transform;
        titleRT.anchorMin = new Vector2(0.5f, 0.62f);
        titleRT.anchorMax = new Vector2(0.5f, 0.62f);
        titleRT.pivot = new Vector2(0.5f, 0.5f);
        titleRT.anchoredPosition = Vector2.zero;
        titleRT.sizeDelta = new Vector2(900f, 200f);

        var title = titleGO.GetComponent<TextMeshProUGUI>();
        title.text = "¡Buen trabajo!";
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 96f;
        title.fontStyle = FontStyles.Bold;
        title.color = Color.white;
        title.raycastTarget = false;
        var tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (tmpFont != null) title.font = tmpFont;

        // ── Menu button ──
        var btnGO = new GameObject("MenuButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(panel.transform, false);
        var btnRT = (RectTransform)btnGO.transform;
        btnRT.anchorMin = new Vector2(0.5f, 0.38f);
        btnRT.anchorMax = new Vector2(0.5f, 0.38f);
        btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = Vector2.zero;
        btnRT.sizeDelta = new Vector2(560f, 140f);

        var btnImg = btnGO.GetComponent<Image>();
        btnImg.color = new Color(1f, 1f, 1f, 0.92f);

        var btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = btnImg;

        var btnLabelGO = new GameObject("Label",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        btnLabelGO.transform.SetParent(btnGO.transform, false);
        var btnLabelRT = (RectTransform)btnLabelGO.transform;
        btnLabelRT.anchorMin = Vector2.zero;
        btnLabelRT.anchorMax = Vector2.one;
        btnLabelRT.offsetMin = Vector2.zero;
        btnLabelRT.offsetMax = Vector2.zero;

        var btnLabel = btnLabelGO.GetComponent<TextMeshProUGUI>();
        btnLabel.text = "Volver al menú";
        btnLabel.alignment = TextAlignmentOptions.Center;
        btnLabel.fontSize = 56f;
        btnLabel.fontStyle = FontStyles.Bold;
        btnLabel.color = Color.black;
        btnLabel.raycastTarget = false;
        if (tmpFont != null) btnLabel.font = tmpFont;

        // ── BrushEndScreen controller ──
        // Put it on a stable GameObject. Use the panel itself; it survives even when hidden.
        var controllerGO = new GameObject("BrushEndScreenController");
        controllerGO.transform.SetParent(canvas.transform, false);
        var brushEnd = controllerGO.AddComponent<BrushEndScreen>();

        // Set private serialized field "endScreenPanel" via SerializedObject.
        var so = new SerializedObject(brushEnd);
        var prop = so.FindProperty("endScreenPanel");
        if (prop != null)
        {
            prop.objectReferenceValue = panel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Wire Button.onClick → BrushEndScreen.GoToMenu (persistent listener so it survives saves).
        UnityEventTools.AddPersistentListener(btn.onClick, brushEnd.GoToMenu);

        // Hide panel initially; BrushEndScreen.Start() also hides it at runtime, but this
        // keeps the scene view tidy.
        panel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[AddEndScreens] BrushingGame: EndScreenPanel + Menu button created and wired.");
    }

    // ───────────────────────────────────────────────────────────────────────
    // Dental Floss Game — add menu button to existing finalPanel
    // ───────────────────────────────────────────────────────────────────────

    static void AddFlossMenuButton(string scenePath)
    {
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.Log($"[AddEndScreens] Scene not found, skipping: {scenePath}");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath);

        var gm = Object.FindObjectOfType<GameManager>(true);
        if (gm == null)
        {
            Debug.Log($"[AddEndScreens] No GameManager in {scenePath} — skipping.");
            return;
        }

        if (gm.finalPanel == null)
        {
            Debug.LogWarning($"[AddEndScreens] GameManager.finalPanel is null in {scenePath} — cannot attach button.");
            return;
        }

        // Skip if button already exists.
        if (gm.finalPanel.transform.Find("MenuButton") != null)
        {
            Debug.Log($"[AddEndScreens] MenuButton already exists in {scenePath} — skipping.");
            return;
        }

        var tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // ── Menu button ──
        var btnGO = new GameObject("MenuButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(gm.finalPanel.transform, false);
        var btnRT = (RectTransform)btnGO.transform;
        // Bottom-center of finalPanel.
        btnRT.anchorMin = new Vector2(0.5f, 0.12f);
        btnRT.anchorMax = new Vector2(0.5f, 0.12f);
        btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.anchoredPosition = Vector2.zero;
        btnRT.sizeDelta = new Vector2(520f, 130f);

        var btnImg = btnGO.GetComponent<Image>();
        btnImg.color = new Color(1f, 1f, 1f, 0.92f);

        var btn = btnGO.GetComponent<Button>();
        btn.targetGraphic = btnImg;

        var btnLabelGO = new GameObject("Label",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        btnLabelGO.transform.SetParent(btnGO.transform, false);
        var btnLabelRT = (RectTransform)btnLabelGO.transform;
        btnLabelRT.anchorMin = Vector2.zero;
        btnLabelRT.anchorMax = Vector2.one;
        btnLabelRT.offsetMin = Vector2.zero;
        btnLabelRT.offsetMax = Vector2.zero;

        var btnLabel = btnLabelGO.GetComponent<TextMeshProUGUI>();
        btnLabel.text = "Volver al menú";
        btnLabel.alignment = TextAlignmentOptions.Center;
        btnLabel.fontSize = 52f;
        btnLabel.fontStyle = FontStyles.Bold;
        btnLabel.color = Color.black;
        btnLabel.raycastTarget = false;
        if (tmpFont != null) btnLabel.font = tmpFont;

        // Wire onClick → GameManager.GoToMenu.
        UnityEventTools.AddPersistentListener(btn.onClick, gm.GoToMenu);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[AddEndScreens] {scenePath}: MenuButton added to finalPanel and wired to GameManager.GoToMenu.");
    }
}

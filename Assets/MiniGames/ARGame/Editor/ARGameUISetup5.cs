using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

/// Setup step 5: doubles the TopBar height and scales its text up proportionally.
/// Run via: Tools ▶ AR Game ▶ Double TopBar Size
public class ARGameUISetup5
{
    const string SCENE_PATH = "Assets/MiniGames/ARGame/Scenes/ARGame.unity";

    [MenuItem("Tools/AR Game/Double TopBar Size")]
    public static void Execute()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("[ARGameUISetup5] No Canvas found."); return; }

        var gameHUD = canvas.transform.Find("GameHUD");
        if (gameHUD == null) { Debug.LogError("[ARGameUISetup5] GameHUD not found."); return; }

        var topBar = gameHUD.Find("TopBar");
        if (topBar == null) { Debug.LogError("[ARGameUISetup5] TopBar not found."); return; }

        // Height: 120 → 240 px
        var rt = topBar.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(0f, -240f);
        Debug.Log("[ARGameUISetup5] TopBar height → 240 px.");

        // Scale all TMP_Text sizes ×2
        foreach (TMP_Text t in topBar.GetComponentsInChildren<TMP_Text>(true))
            t.fontSize *= 2f;

        Debug.Log("[ARGameUISetup5] TopBar font sizes doubled.");

        // Bigger zone dots
        var dotsPanel = topBar.Find("ZoneDotsPanel");
        if (dotsPanel != null)
        {
            for (int i = 0; i < 6; i++)
            {
                var dot = dotsPanel.Find("Dot" + i);
                if (dot != null)
                {
                    var dotRT = dot.GetComponent<RectTransform>();
                    dotRT.sizeDelta = new Vector2(36f, 36f); // was 18
                }
            }
            // Increase dot spacing
            var hLayout = dotsPanel.GetComponent<HorizontalLayoutGroup>();
            if (hLayout != null) hLayout.spacing = 10f;
            Debug.Log("[ARGameUISetup5] Zone dots enlarged to 36 px.");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), SCENE_PATH);
        AssetDatabase.SaveAssets();

        Debug.Log("[ARGameUISetup5] Done — scene saved.");
    }
}

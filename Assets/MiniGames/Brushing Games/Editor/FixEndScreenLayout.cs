using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FixEndScreenLayout
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();

        // Delete restart button — only "Volver al menú" remains
        var restart = GameObject.Find("Canvas/EndScreenPanel/RestartButton");
        if (restart != null) Object.DestroyImmediate(restart);

        // Center the single menu button vertically in the panel
        FixRT("Canvas/EndScreenPanel/MenuButton",
            new Vector2(0.2f, 0.38f), new Vector2(0.8f, 0.52f),
            Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        // Fix EndTitle position
        FixRT("Canvas/EndScreenPanel/EndTitle",
            new Vector2(0.1f, 0.62f), new Vector2(0.9f, 0.78f),
            Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        // Fix font sizes so text fits inside buttons
        FixText("Canvas/EndScreenPanel/MenuButton/Text", "Volver al menú", 22);
        FixText("Canvas/EndScreenPanel/EndTitle",       "¡Cepillado completo!", 26);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FixEndScreenLayout] Done.");
    }

    static void FixRT(string path, Vector2 anchorMin, Vector2 anchorMax,
                      Vector2 pos, Vector2 size, Vector2 pivot)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta        = size;
        rt.pivot            = pivot;
    }

    static void FixText(string path, string text, int fontSize)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        var t = go.GetComponent<UnityEngine.UI.Text>();
        if (t == null) { Debug.LogWarning("No Text on: " + path); return; }
        t.text     = text;
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color    = Color.white;
    }
}

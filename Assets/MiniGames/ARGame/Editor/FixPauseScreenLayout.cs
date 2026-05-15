using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FixPauseScreenLayout
{
    public static void Execute()
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

        Fix("Canvas/PauseScreen/PauseTitle",
            new Vector2(0.2f, 0.6f), new Vector2(0.8f, 0.72f),
            Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        Fix("Canvas/PauseScreen/ResumeButton",
            new Vector2(0.2f, 0.46f), new Vector2(0.8f, 0.58f),
            Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        Fix("Canvas/PauseScreen/MenuButton",
            new Vector2(0.2f, 0.32f), new Vector2(0.8f, 0.44f),
            Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        // PauseButton: pinned to right edge of TopBar, 160x160, 20px from edge
        Fix("Canvas/GameHUD/TopBar/PauseButton",
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(-20f, 0f), new Vector2(160f, 160f), new Vector2(1f, 0.5f));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[FixPauseScreenLayout] Done and saved.");
    }

    static void Fix(string path, Vector2 anchorMin, Vector2 anchorMax,
                    Vector2 anchoredPos, Vector2 sizeDelta, Vector2 pivot)
    {
        var go = GameObject.Find(path);
        if (go == null) { Debug.LogWarning("Not found: " + path); return; }
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin       = anchorMin;
        rt.anchorMax       = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta       = sizeDelta;
        rt.pivot           = pivot;
        Debug.Log("Fixed: " + path);
    }
}

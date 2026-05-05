using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static class ForceLayoutRebuild
{
    [MenuItem("Tools/Menu/Force Layout Rebuild")]
    public static void Execute()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("No Canvas found"); return; }

        // Rebuild from leaves up: content first, then scroll, then page, then canvas
        var content = GameObject.Find("Canvas/PageContainer/MinijuegosPage/ScrollSection/ScrollRect/Viewport/Content");
        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
            Debug.Log($"[ForceLayout] Content height after rebuild: {content.GetComponent<RectTransform>().rect.height}");
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(canvas.GetComponent<RectTransform>());

        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[ForceLayout] Done.");
    }
}

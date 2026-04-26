using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FixScrollLayout
{
    // Card constants matching BuildGameCard in MenuSetup
    const float CARD_HEIGHT     = 380f;
    const float SPACING         = 24f;
    const float PAD_TOP         = 24f;
    const float PAD_BOTTOM      = 60f;
    const float SECTION_TITLE_H = 60f;
    const int   CARD_COUNT      = 3;

    [MenuItem("Tools/Menu/Fix Scroll Layout")]
    public static void Execute()
    {
        var contentGO = GameObject.Find(
            "Canvas/PageContainer/MinijuegosPage/ScrollSection/ScrollRect/Viewport/Content");
        if (contentGO == null) { Debug.LogError("[FixScroll] Content not found"); return; }

        // Calculate total content height manually
        float total = PAD_TOP + SECTION_TITLE_H
                    + CARD_COUNT * CARD_HEIGHT
                    + (CARD_COUNT) * SPACING   // spacing after title + after each card (except last)
                    + PAD_BOTTOM;

        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(0, total);
        Debug.Log($"[FixScroll] Content height set to {total}");

        // Set each card_wrap to the right size using the VLG preferred heights
        var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
        if (vlg != null)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRT);
            Debug.Log($"[FixScroll] After rebuild: content height = {contentRT.rect.height}");
        }

        // Force each wrap to correct height as fallback
        float yPos = -(PAD_TOP + SECTION_TITLE_H + SPACING);
        var wraps = new[] { "ARCard_Wrap", "BrushCard_Wrap", "FlossCard_Wrap" };
        foreach (var wrapName in wraps)
        {
            var wrap = contentGO.transform.Find(wrapName);
            if (wrap == null) continue;
            var rt = wrap.GetComponent<RectTransform>();
            if (rt.sizeDelta.y < 200f)
            {
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, CARD_HEIGHT);
                Debug.Log($"[FixScroll] Fixed {wrapName} height to {CARD_HEIGHT}");
            }
        }

        EditorUtility.SetDirty(contentGO);
        EditorSceneManager.MarkSceneDirty(contentGO.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[FixScroll] Done.");
    }
}

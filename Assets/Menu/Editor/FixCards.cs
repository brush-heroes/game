using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class FixCards
{
    const float CARD_H     = 380f;
    const float CONTENT_W  = 1080f;  // canvas reference width
    const float PAD_H      = 56f;    // content padding left(28) + right(28)

    [MenuItem("Tools/Menu/Fix Cards")]
    public static void Execute()
    {
        var contentGO = GameObject.Find(
            "Canvas/PageContainer/MinijuegosPage/ScrollSection/ScrollRect/Viewport/Content");
        if (contentGO == null) { Debug.LogError("[FixCards] Content not found"); return; }

        float cardW = CONTENT_W - PAD_H;  // 1024

        // Fix each card wrap + its children
        string[] wraps = { "ARCard_Wrap", "BrushCard_Wrap", "FlossCard_Wrap" };
        string[] cards = { "ARCard",      "BrushCard",      "FlossCard" };

        for (int i = 0; i < wraps.Length; i++)
        {
            // --- Wrap: stretch full width, fixed height ---
            var wrapT = contentGO.transform.Find(wraps[i]);
            if (wrapT == null) continue;
            var wrapRT = wrapT.GetComponent<RectTransform>();
            wrapRT.anchorMin = new Vector2(0, 1);
            wrapRT.anchorMax = new Vector2(1, 1);
            wrapRT.pivot     = new Vector2(0.5f, 1);
            wrapRT.sizeDelta        = new Vector2(0, CARD_H);
            wrapRT.anchoredPosition = new Vector2(0, -(84f + i * (CARD_H + 24f)));  // pad_top+titleH + index offset
            EditorUtility.SetDirty(wrapT.gameObject);

            // --- Shadow: fills wrap with offset ---
            var shadowT = wrapT.Find("Shadow");
            if (shadowT != null)
            {
                var sRT = shadowT.GetComponent<RectTransform>();
                sRT.anchorMin = Vector2.zero; sRT.anchorMax = Vector2.one;
                sRT.offsetMin = new Vector2(4, -10); sRT.offsetMax = new Vector2(-4, 4);
                EditorUtility.SetDirty(shadowT.gameObject);
            }

            // --- Card: fills wrap ---
            var cardT = wrapT.Find(cards[i]);
            if (cardT == null) continue;
            var cardRT = cardT.GetComponent<RectTransform>();
            cardRT.anchorMin = Vector2.zero; cardRT.anchorMax = Vector2.one;
            cardRT.offsetMin = cardRT.offsetMax = Vector2.zero;

            // Fix card background color to visible light-blue (not pure white)
            var cardImg = cardT.GetComponent<Image>();
            if (cardImg != null)
                cardImg.color = new Color(0.95f, 0.96f, 1.00f, 1.00f);
            EditorUtility.SetDirty(cardT.gameObject);

            // --- Thumbnail: fix height ---
            var thumbT = cardT.Find("Thumbnail");
            if (thumbT != null)
            {
                var tRT = thumbT.GetComponent<RectTransform>();
                tRT.anchorMin = new Vector2(0, 1);
                tRT.anchorMax = new Vector2(1, 1);
                tRT.pivot     = new Vector2(0.5f, 1);
                tRT.sizeDelta        = new Vector2(0, 280);
                tRT.anchoredPosition = Vector2.zero;
                EditorUtility.SetDirty(thumbT.gameObject);
            }

            // --- Footer: fix height below thumbnail ---
            var footerT = cardT.Find("Footer");
            if (footerT != null)
            {
                var fRT = footerT.GetComponent<RectTransform>();
                fRT.anchorMin = new Vector2(0, 0);
                fRT.anchorMax = new Vector2(1, 0);
                fRT.pivot     = new Vector2(0.5f, 0);
                fRT.sizeDelta        = new Vector2(0, 100);
                fRT.anchoredPosition = Vector2.zero;
                EditorUtility.SetDirty(footerT.gameObject);
            }
        }

        // Fix content height
        float totalH = 24f + 60f + 3 * (CARD_H + 24f) + 60f;  // padTop+sectionTitle+(card+spacing)x3+padBottom
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.sizeDelta = new Vector2(0, totalH);
        EditorUtility.SetDirty(contentGO);

        // Add scroll section background if missing
        var scrollSect = GameObject.Find(
            "Canvas/PageContainer/MinijuegosPage/ScrollSection/ScrollRect");
        if (scrollSect != null && scrollSect.GetComponent<Image>() == null)
        {
            var img = scrollSect.AddComponent<Image>();
            img.color = new Color(0.90f, 0.92f, 0.96f, 0.85f);
            EditorUtility.SetDirty(scrollSect);
        }

        EditorSceneManager.MarkSceneDirty(contentGO.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[FixCards] Done. Content height={totalH}, cardW={cardW}");
    }
}

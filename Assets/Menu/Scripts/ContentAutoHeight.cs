using UnityEngine;

/// Runs once at Start and sets the Content RectTransform height to fit all
/// top-anchored children (anchorMin.y = 1).  Works with any card height because
/// it reads rect.height (= sizeDelta.y for top-anchored elements) at runtime.
[RequireComponent(typeof(RectTransform))]
public class ContentAutoHeight : MonoBehaviour
{
    [SerializeField] float bottomPadding = 60f;

    void Start() => Recalculate();

    public void Recalculate()
    {
        var rt = (RectTransform)transform;
        float maxExtent = 0f;
        for (int i = 0; i < rt.childCount; i++)
        {
            var child = (RectTransform)rt.GetChild(i);
            float extent = Mathf.Abs(child.anchoredPosition.y) + child.rect.height;
            if (extent > maxExtent) maxExtent = extent;
        }
        var sd = rt.sizeDelta;
        sd.y = maxExtent + bottomPadding;
        rt.sizeDelta = sd;
    }
}

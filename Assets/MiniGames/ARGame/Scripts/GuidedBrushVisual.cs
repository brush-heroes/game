using UnityEngine;

// Lightweight visual for the guided-mode toothbrush.
// Parented inside FacePrefab so it naturally follows AR face tracking.
// GuidedModeController finds this at runtime via FindObjectOfType and drives its world position.
public class GuidedBrushVisual : MonoBehaviour
{
    [SerializeField] Sprite[] brushingFrames;
    [SerializeField] float fps = 8f;

    SpriteAnimator _anim;

    void Awake()
    {
        _anim = GetComponent<SpriteAnimator>();
        if (_anim != null) _anim.Fps = fps;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (_anim != null && brushingFrames != null && brushingFrames.Length > 0)
            _anim.PlayLoop(brushingFrames);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}

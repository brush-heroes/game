using System;
using UnityEngine;

// Born → Living(loop) → [Brushed(loop) ↔ Living] → Dying
[RequireComponent(typeof(SpriteAnimator))]
public class BacteriaAnimator : MonoBehaviour
{
    [Header("Sprite Frames")]
    [SerializeField] Sprite[] bornFrames;
    [SerializeField] Sprite[] livingFrames;
    [SerializeField] Sprite[] brushedFrames;
    [SerializeField] Sprite[] dyingFrames;

    [SerializeField] float fps = 8f;

    SpriteAnimator _anim;
    bool _dying;
    bool _brushing;

    void Awake()
    {
        _anim = GetComponent<SpriteAnimator>();
        _anim.Fps = fps;
    }

    void Start()
    {
        if (bornFrames != null && bornFrames.Length > 0)
            _anim.PlayOnce(bornFrames, () => _anim.PlayLoop(livingFrames));
        else
            _anim.PlayLoop(livingFrames);
    }

    // Called when the brush starts touching this bacteria.
    public void TriggerBrush()
    {
        if (_dying) return;
        _brushing = true;
        if (brushedFrames != null && brushedFrames.Length > 0)
            _anim.PlayLoop(brushedFrames);
    }

    // Called when the brush stops touching this bacteria.
    public void StopBrush()
    {
        if (_dying || !_brushing) return;
        _brushing = false;
        _anim.PlayLoop(livingFrames);
    }

    public void TriggerDeath(Action onComplete)
    {
        if (_dying) return;
        _dying = true;
        _brushing = false;
        if (dyingFrames != null && dyingFrames.Length > 0)
            _anim.PlayOnce(dyingFrames, onComplete);
        else
            onComplete?.Invoke();
    }
}

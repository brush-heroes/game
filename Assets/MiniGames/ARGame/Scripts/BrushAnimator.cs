using UnityEngine;

// Switches the toothbrush between static sprite (idle) and brushing animation (cleaning).
// Place on the same GameObject as SpriteAnimator and SpriteRenderer.
[RequireComponent(typeof(SpriteAnimator))]
public class BrushAnimator : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] Sprite staticSprite;
    [SerializeField] Sprite[] brushingFrames;

    [SerializeField] float fps = 8f;

    SpriteAnimator _anim;
    VirtualBrushController _controller;
    bool _wasCleaning;

    void Awake()
    {
        _anim = GetComponent<SpriteAnimator>();
        _anim.Fps = fps;
        _controller = GetComponentInParent<VirtualBrushController>();
    }

    void Start()
    {
        _anim.SetStatic(staticSprite);
    }

    void Update()
    {
        if (_controller == null) return;

        // In guided mode the circular orbit is below the movement threshold, so force animate.
        bool moving = _controller.GuidedMode || _controller.IsMoving;
        if (moving == _wasCleaning) return;
        _wasCleaning = moving;

        if (moving)
            _anim.PlayLoop(brushingFrames);
        else
            _anim.SetStatic(staticSprite);
    }
}

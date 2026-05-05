using UnityEngine;

public class MascotController : MonoBehaviour
{
    public enum MascotState { Neutral, Happy, Cheering, Sad, Saluting }

    [SerializeField] Sprite[] neutralFrames;
    [SerializeField] Sprite[] happyFrames;       // saludating
    [SerializeField] Sprite[] cheeringFrames;
    [SerializeField] Sprite[] sadFrames;
    [SerializeField] float fps = 6f;

    SpriteAnimator _anim;
    MascotState _state = (MascotState)(-1);      // force first refresh

    void Awake() => _anim = GetComponent<SpriteAnimator>();

    public void RefreshState(int streak)
    {
        MascotState next = DetermineState(streak);
        if (next == _state) return;
        SetState(next);
    }

    public void SetState(MascotState state)
    {
        _state = state;
        if (_anim == null) return;
        _anim.Fps = fps;
        switch (state)
        {
            case MascotState.Cheering: _anim.PlayLoop(cheeringFrames); break;
            case MascotState.Happy:    _anim.PlayLoop(happyFrames);    break;
            case MascotState.Sad:      _anim.PlayLoop(sadFrames);      break;
            default:                   _anim.PlayLoop(neutralFrames);  break;
        }
    }

    static MascotState DetermineState(int streak)
    {
        if (streak >= 7) return MascotState.Cheering;
        if (streak >= 3) return MascotState.Happy;
        if (streak == 0) return MascotState.Sad;
        return MascotState.Neutral;
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Generic sprite frame animator — works with SpriteRenderer or UI Image on the same GameObject.
public class SpriteAnimator : MonoBehaviour
{
    [SerializeField] float fps = 8f;

    SpriteRenderer _sr;
    Image _img;
    Coroutine _routine;

    public float Fps { get => fps; set => fps = value; }

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _img = GetComponent<Image>();
    }

    public void PlayOnce(Sprite[] frames, Action onDone = null) => Restart(frames, false, onDone);
    public void PlayLoop(Sprite[] frames) => Restart(frames, true, null);

    public void Stop()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }

    public void SetStatic(Sprite sprite)
    {
        Stop();
        Apply(sprite);
    }

    void Restart(Sprite[] frames, bool loop, Action onDone)
    {
        Stop();
        if (frames == null || frames.Length == 0) return;
        _routine = StartCoroutine(Animate(frames, loop, onDone));
    }

    IEnumerator Animate(Sprite[] frames, bool loop, Action onDone)
    {
        var delay = new WaitForSeconds(fps > 0 ? 1f / fps : 0.125f);
        do
        {
            for (int i = 0; i < frames.Length; i++)
            {
                Apply(frames[i]);
                yield return delay;
            }
        } while (loop);

        onDone?.Invoke();
    }

    void Apply(Sprite s)
    {
        if (_sr != null) _sr.sprite = s;
        if (_img != null) _img.sprite = s;
    }
}

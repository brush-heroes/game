using System.Collections;
using TMPro;
using UnityEngine;

// Attaches to a TextMeshProUGUI that displays the live score.
// Subscribes to ScoreManager.OnScoreChanged to update text and play a
// brief scale-pulse animation each time points are awarded.
[RequireComponent(typeof(TextMeshProUGUI))]
public class ScoreAnimUI : MonoBehaviour
{
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] float pulseDuration = 0.25f;
    [SerializeField] float pulseScale    = 1.45f;

    TextMeshProUGUI _text;
    Coroutine _pulse;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (scoreManager == null) scoreManager = ScoreManager.instance;
        if (scoreManager != null) scoreManager.OnScoreChanged += HandleScoreChanged;
    }

    void OnDisable()
    {
        if (scoreManager != null) scoreManager.OnScoreChanged -= HandleScoreChanged;
    }

    void HandleScoreChanged(int newScore)
    {
        if (_text != null) _text.text = newScore.ToString();
        if (_pulse != null) StopCoroutine(_pulse);
        _pulse = StartCoroutine(PulseRoutine());
    }

    IEnumerator PulseRoutine()
    {
        float half = pulseDuration * 0.5f;
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(1f, pulseScale, t / half);
            yield return null;
        }
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(pulseScale, 1f, t / half);
            yield return null;
        }
        transform.localScale = Vector3.one;
        _pulse = null;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct ZoneGuideData
{
    public MouthZone zone;
    [Tooltip("Single-frame indicator image shown briefly when the zone starts")]
    public Sprite zoneIndicator;
    [Tooltip("Looping brushing animation frames for this zone (4 frames)")]
    public Sprite[] brushingFrames;
}

// Manages the bottom guide panel: default mouth image before game, per-zone
// brushing animations during play, and zone-timeout alerts.
// StartScreen / EndScreen / GameHUD are handled by GameScreenManager.
public class BrushingGuideUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SessionManager sessionManager;

    [Header("Guide Display")]
    [SerializeField] Image guideImage;

    [Header("Alert")]
    [SerializeField] GameObject alertPanel;
    [SerializeField] TMP_Text alertText;

    [Header("Pre-game Default")]
    [SerializeField] Sprite defaultSprite;

    [Header("Zone Guide Data (one entry per zone)")]
    [SerializeField] ZoneGuideData[] zoneGuideData;

    [Header("Settings")]
    [SerializeField] float zoneIndicatorDisplayTime = 1.5f;
    [Tooltip("Seconds in a zone before an alert encourages the player")]
    [SerializeField] float alertThresholdSeconds = 28f;
    [SerializeField] float animationFps = 8f;

    [SerializeField] string[] alertMessages = new[]
    {
        "¡Sigue cepillando!",
        "¡No pares, casi terminas!",
        "¡Fuerza, ya casi acabas esa zona!"
    };

    SpriteAnimator _anim;
    float _zoneStartTime;
    bool _alertShown;
    bool _sessionRunning;
    Coroutine _zoneTransitionRoutine;

    void Awake()
    {
        if (guideImage == null) return;
        _anim = guideImage.GetComponent<SpriteAnimator>();
        if (_anim == null) _anim = guideImage.gameObject.AddComponent<SpriteAnimator>();
        _anim.Fps = animationFps;
    }

    void OnEnable()
    {
        if (sessionManager == null) return;
        sessionManager.OnSessionStarted += HandleSessionStarted;
        sessionManager.OnStepChanged   += HandleStepChanged;
        sessionManager.OnSessionEnded  += HandleSessionEnded;
        sessionManager.OnSessionFailed += HandleSessionFailed;
    }

    void OnDisable()
    {
        if (sessionManager == null) return;
        sessionManager.OnSessionStarted -= HandleSessionStarted;
        sessionManager.OnStepChanged   -= HandleStepChanged;
        sessionManager.OnSessionEnded  -= HandleSessionEnded;
        sessionManager.OnSessionFailed -= HandleSessionFailed;
    }

    void Start()
    {
        // Default sprite is no longer used — guide is hidden until session starts.
        if (alertPanel != null) alertPanel.SetActive(false);
    }

    void Update()
    {
        if (!_sessionRunning || _alertShown) return;
        if (Time.time - _zoneStartTime >= alertThresholdSeconds)
            ShowAlert();
    }

    // ── Session event handlers ───────────────────────────────────────────────

    void HandleSessionStarted()
    {
        _sessionRunning = true;
        if (alertPanel != null) alertPanel.SetActive(false);
        // Start zone-1 animation immediately (HandleStepChanged also fires right after,
        // but this handles the edge case where Start() hasn't run yet this frame).
        if (sessionManager != null)
        {
            SessionStep step = sessionManager.GetCurrentStep();
            if (_zoneTransitionRoutine != null) StopCoroutine(_zoneTransitionRoutine);
            _zoneTransitionRoutine = StartCoroutine(ZoneTransitionRoutine(step.zone));
        }
    }

    void HandleStepChanged(SessionStep step, int stepIndex)
    {
        _zoneStartTime = Time.time;
        _alertShown = false;
        if (alertPanel != null) alertPanel.SetActive(false);

        if (_zoneTransitionRoutine != null) StopCoroutine(_zoneTransitionRoutine);
        _zoneTransitionRoutine = StartCoroutine(ZoneTransitionRoutine(step.zone));
    }

    void HandleSessionEnded()
    {
        StopPlay();
    }

    void HandleSessionFailed()
    {
        StopPlay();
    }

    void StopPlay()
    {
        _sessionRunning = false;
        if (_zoneTransitionRoutine != null) { StopCoroutine(_zoneTransitionRoutine); _zoneTransitionRoutine = null; }
        if (alertPanel != null) alertPanel.SetActive(false);
    }

    // ── Zone transition ──────────────────────────────────────────────────────

    IEnumerator ZoneTransitionRoutine(MouthZone zone)
    {
        // Zone indicator is now shown as a full-screen overlay by ZoneNotificationUI.
        // Guide panel goes straight to the brushing animation for the new zone.
        ZoneGuideData data = FindZoneData(zone);
        if (data.brushingFrames != null && data.brushingFrames.Length > 0)
            _anim?.PlayLoop(data.brushingFrames);
        yield return null;
    }

    // ── Alert ────────────────────────────────────────────────────────────────

    void ShowAlert()
    {
        _alertShown = true;
        if (alertPanel != null) alertPanel.SetActive(true);
        if (alertText != null && alertMessages.Length > 0)
            alertText.text = alertMessages[Random.Range(0, alertMessages.Length)];
        StartCoroutine(HideAlertAfter(3f));
    }

    IEnumerator HideAlertAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (alertPanel != null) alertPanel.SetActive(false);
        _alertShown = false;
        _zoneStartTime = Time.time;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    ZoneGuideData FindZoneData(MouthZone zone)
    {
        if (zoneGuideData == null) return default;
        foreach (var d in zoneGuideData)
            if (d.zone == zone) return d;
        return default;
    }
}

using System.Collections;
using UnityEngine;

// Autonomous guided-mode controller.
// Moves the GuidedBrushVisual (a child of FacePrefab) in a sine-wave pattern over the current zone
// and kills bacteria sequentially over secondsPerZone seconds.
// No touch input, no scoring, no time limit.
public class GuidedModeController : MonoBehaviour
{
    [SerializeField] SessionManager sessionManager;
    [SerializeField] BacteriaSpawner bacteriaSpawner;
    [SerializeField] MouthZoneManager mouthZoneManager;
    [SerializeField] VirtualBrushController brushController;

    [Header("Brush Motion")]
    [Tooltip("Half-width of the brushing stroke along the zone's horizontal axis (metres)")]
    [SerializeField] float horizontalAmplitude = 0.006f;
    [Tooltip("Half-height drift along the zone's vertical axis (metres)")]
    [SerializeField] float verticalAmplitude   = 0.004f;
    [Tooltip("Horizontal oscillation speed (radians/second — ~12 ≈ 2 strokes/sec)")]
    [SerializeField] float horizontalFrequency = 12f;
    [Tooltip("Vertical drift speed (radians/second)")]
    [SerializeField] float verticalFrequency   = 2.5f;

    [Header("Brush Anchor Offset")]
    [Tooltip("Shifts the brush DOWN along the zone's up-axis so the brush HEAD (top of sprite) " +
             "lands on the zone instead of the brush center. Increase if you see the head landing " +
             "above the teeth (on the cheek). Tune in the Inspector.")]
    [SerializeField] float headOffsetY = 0.04f;

    [Header("Timing")]
    [SerializeField] float secondsPerZone = 20f;

    bool _active;
    Coroutine _killRoutine;
    GuidedBrushVisual _guidedBrush;   // found at runtime from FacePrefab instance

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

    public void Activate()
    {
        _active = true;
        if (brushController != null) brushController.GuidedMode = true;
        if (_guidedBrush == null) _guidedBrush = Object.FindObjectOfType<GuidedBrushVisual>(true);
        _guidedBrush?.Show();
    }

    public void Deactivate()
    {
        _active = false;
        if (brushController != null) brushController.GuidedMode = false;
        _guidedBrush?.Hide();
        if (_killRoutine != null) { StopCoroutine(_killRoutine); _killRoutine = null; }
    }

    void HandleSessionStarted() { }

    void HandleStepChanged(SessionStep step, int stepIndex)
    {
        if (!_active) return;
        if (_killRoutine != null) StopCoroutine(_killRoutine);
        _killRoutine = StartCoroutine(KillZoneRoutine(step.zone));
    }

    void HandleSessionEnded()  => Deactivate();
    void HandleSessionFailed() => Deactivate();

    // ── Brush sine-wave motion ────────────────────────────────────────────────

    void Update()
    {
        if (!_active || sessionManager == null || !sessionManager.IsSessionRunning) return;
        if (sessionManager.IsInTransition || sessionManager.IsPaused) return;

        // MouthZoneManager and GuidedBrushVisual live in the dynamically-instantiated FacePrefab.
        // Re-find them lazily: they may appear after Activate() (face detected late) or
        // reappear after the FacePrefab is recreated (face tracking loss/re-detection).
        if (mouthZoneManager == null)
            mouthZoneManager = Object.FindObjectOfType<MouthZoneManager>();
        if (_guidedBrush == null)
            _guidedBrush = Object.FindObjectOfType<GuidedBrushVisual>(true);

        if (mouthZoneManager == null || _guidedBrush == null) return;

        // Show the guided brush if it was found after Activate() (face detected late or reappeared).
        if (!_guidedBrush.gameObject.activeSelf)
            _guidedBrush.Show();

        float xOff = Mathf.Sin(Time.time * horizontalFrequency) * horizontalAmplitude;
        float yOff = Mathf.Sin(Time.time * verticalFrequency + 1.0f) * verticalAmplitude;

        Transform zoneT = mouthZoneManager.GetZoneTransform(sessionManager.CurrentZone);
        if (zoneT != null)
            _guidedBrush.transform.position =
                zoneT.position + zoneT.right * xOff + zoneT.up * (yOff - headOffsetY);
    }

    // ── Sequential bacteria death ─────────────────────────────────────────────

    IEnumerator KillZoneRoutine(MouthZone zone)
    {
        int totalToKill = bacteriaSpawner != null ? bacteriaSpawner.GetMaxCount(zone) : 10;
        if (totalToKill <= 0) totalToKill = 10;

        float interval = secondsPerZone / totalToKill;
        int killed = 0;

        while (killed < totalToKill)
        {
            yield return new WaitForSeconds(interval);

            if (!_active || sessionManager == null || !sessionManager.IsSessionRunning)
                yield break;

            while (sessionManager.IsInTransition || sessionManager.IsPaused) yield return null;

            GameObject[] all = GameObject.FindGameObjectsWithTag("Bacteria");
            bool found = false;
            foreach (var go in all)
            {
                if (go == null) continue;
                var b = go.GetComponentInChildren<Bacteria>(true);
                if (b != null && b.Zone == zone && !b.IsDying)
                {
                    b.ForceKill();
                    killed++;
                    found = true;
                    break;
                }
            }

            if (!found)
                yield return new WaitForSeconds(0.4f);
        }
    }
}

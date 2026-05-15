using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MouthZone
{
    UpperLeft,
    UpperRight,
    FrontUpper,
    LowerLeft,
    LowerRight,
    FrontLower
}

[Serializable]
public struct SessionStep
{
    public MouthZone zone;
    public float duration;
    public string instructionText;
}

[Serializable]
public class SessionStepChangedEvent : UnityEvent<SessionStep, int> { }

public class SessionManager : MonoBehaviour
{
    [SerializeField] List<SessionStep> sessionSteps = new List<SessionStep>();
    [SerializeField] bool autoStart = false;
    [SerializeField] BacteriaSpawner bacteriaSpawner;

    [Header("Time Limit")]
    [Tooltip("Total seconds before the session fails. 0 = no limit (used in Guided mode).")]
    [SerializeField] float totalTimeLimit = 180f;

    [Header("Zone Transition")]
    [Tooltip("Seconds to pause between zones (brush freezes, notification shown).")]
    [SerializeField] float zoneTransitionDuration = 2.5f;

    [Header("Unity Events (optional)")]
    [SerializeField] SessionStepChangedEvent onStepChangedUnity;
    [SerializeField] UnityEvent onSessionStartedUnity;
    [SerializeField] UnityEvent onSessionEndedUnity;
    [SerializeField] UnityEvent onSessionFailedUnity;

    int currentStepIndex;
    float stepTimer;
    float totalSessionTime;
    bool completedAllSteps;
    bool _transitionPending;

    [Header("Debug")]
    [SerializeField] int debugCurrentStepIndex;
    [SerializeField] float debugTotalSessionTime;
    [SerializeField] float debugRemainingTime;

    public bool IsSessionRunning { get; private set; }
    public bool IsInTransition   { get; private set; }
    public bool IsPaused         { get; private set; }
    public int  CurrentStepIndex => currentStepIndex;
    public MouthZone CurrentActiveZone => GetCurrentStep().zone;
    public MouthZone CurrentZone       => CurrentActiveZone;
    public string    CurrentInstructionText => GetCurrentStep().instructionText;
    public float     RemainingTime => totalTimeLimit > 0 ? Mathf.Max(0f, totalTimeLimit - totalSessionTime) : 0f;
    public bool      IsSessionComplete { get; private set; }
    public float     TotalSessionTime  => totalSessionTime;

    // Settable at runtime so GuidedModeController can disable the timer.
    public float TotalTimeLimit { get => totalTimeLimit; set => totalTimeLimit = value; }

    public event Action<SessionStep, int> OnStepChanged;
    public event Action OnSessionStarted;
    public event Action OnSessionEnded;
    public event Action OnSessionFailed;
    // Fires with the NEXT step right before the zone advances (only for intermediate zones).
    public event Action<SessionStep> OnZoneComplete;

    public UnityEvent<string> OnInstructionChanged = new UnityEvent<string>();

    void Awake()   => EnsureBacteriaSpawnerReference();
    void Start()
    {
        EnsureBacteriaSpawnerReference();
        PopulateDefaultStepsIfEmpty();
        if (autoStart) StartSession();
    }

    void EnsureBacteriaSpawnerReference()
    {
        if (bacteriaSpawner != null) return;
        bacteriaSpawner = FindObjectOfType<BacteriaSpawner>();
        if (bacteriaSpawner == null)
            Debug.LogError("[SessionManager] No BacteriaSpawner found.");
    }

    void Update()
    {
        debugCurrentStepIndex = currentStepIndex;
        debugTotalSessionTime = totalSessionTime;
        debugRemainingTime    = RemainingTime;
    }

    void LateUpdate()
    {
        if (!IsSessionRunning) return;
        if (IsPaused) return;

        totalSessionTime += Time.deltaTime;
        stepTimer        += Time.deltaTime;

        if (totalTimeLimit > 0 && totalSessionTime >= totalTimeLimit)
        {
            FailSession();
            return;
        }

        if (bacteriaSpawner == null || IsInTransition || _transitionPending) return;

        if (bacteriaSpawner.IsZoneComplete(CurrentZone))
        {
            _transitionPending = true;
            StartCoroutine(ZoneTransitionCoroutine());
        }
    }

    IEnumerator ZoneTransitionCoroutine()
    {
        IsInTransition = true;

        int nextIndex = currentStepIndex + 1;
        if (nextIndex < sessionSteps.Count)
        {
            // Notify listeners about the upcoming zone (for the notification overlay).
            OnZoneComplete?.Invoke(sessionSteps[nextIndex]);
            yield return new WaitForSeconds(zoneTransitionDuration);
        }

        IsInTransition    = false;
        _transitionPending = false;
        AdvanceToNextStep();
    }

    // ── Session lifecycle ────────────────────────────────────────────────────

    public void StartSession()
    {
        PopulateDefaultStepsIfEmpty();
        if (sessionSteps == null || sessionSteps.Count == 0)
        {
            Debug.LogWarning("[SessionManager] Cannot start: no steps defined.");
            return;
        }
        if (IsSessionRunning) return;

        ScoreManager.instance?.ResetSession();
        IsSessionRunning  = true;
        IsSessionComplete = false;
        completedAllSteps = false;
        IsInTransition    = false;
        IsPaused          = false;
        _transitionPending = false;
        currentStepIndex  = 0;
        totalSessionTime  = 0f;
        stepTimer         = 0f;

        Debug.Log("[SessionManager] Session started.");
        OnSessionStarted?.Invoke();
        onSessionStartedUnity?.Invoke();
        NotifyCurrentStepChanged();
    }

    public void AdvanceToNextStep()
    {
        if (!IsSessionRunning) return;

        currentStepIndex++;
        if (currentStepIndex >= sessionSteps.Count)
        {
            currentStepIndex  = sessionSteps.Count - 1;
            completedAllSteps = true;
            EndSession();
            return;
        }
        NotifyCurrentStepChanged();
    }

    public void Pause()
    {
        if (!IsSessionRunning || IsPaused) return;
        IsPaused = true;
        Debug.Log("[SessionManager] Session paused.");
    }

    public void Resume()
    {
        if (!IsSessionRunning || !IsPaused) return;
        IsPaused = false;
        Debug.Log("[SessionManager] Session resumed.");
    }

    public void EndSession()
    {
        if (!IsSessionRunning) return;
        IsSessionRunning  = false;
        IsSessionComplete = completedAllSteps;
        stepTimer         = 0f;
        if (!IsSessionComplete) currentStepIndex = -1;
        Debug.Log($"[SessionManager] Session ended. Complete: {IsSessionComplete}");
        OnSessionEnded?.Invoke();
        onSessionEndedUnity?.Invoke();
    }

    public void FailSession()
    {
        if (!IsSessionRunning) return;
        IsSessionRunning  = false;
        IsSessionComplete = false;
        completedAllSteps = false;
        stepTimer         = 0f;
        Debug.Log("[SessionManager] Session FAILED — time limit exceeded.");
        OnSessionFailed?.Invoke();
        onSessionFailedUnity?.Invoke();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    void NotifyCurrentStepChanged()
    {
        SessionStep step = GetCurrentStep();
        stepTimer = 0f;
        Debug.Log($"[SessionManager] Step {currentStepIndex}: {step.zone} — \"{step.instructionText}\"");
        OnStepChanged?.Invoke(step, currentStepIndex);
        onStepChangedUnity?.Invoke(step, currentStepIndex);
        string instruction = EnsureNonEmptyInstruction(step.instructionText, step.zone);
        OnInstructionChanged?.Invoke(instruction);
    }

    public SessionStep GetCurrentStep()
    {
        if (sessionSteps == null || sessionSteps.Count == 0 ||
            currentStepIndex < 0 || currentStepIndex >= sessionSteps.Count)
            return default;
        return sessionSteps[currentStepIndex];
    }

    static string EnsureNonEmptyInstruction(string text, MouthZone zone)
        => string.IsNullOrEmpty(text) ? $"¡Sigue cepillando — {zone}!" : text;

    void PopulateDefaultStepsIfEmpty()
    {
        if (sessionSteps != null && sessionSteps.Count > 0) return;
        sessionSteps = new List<SessionStep>
        {
            NewStep(MouthZone.UpperLeft,  "¡Cepilla los dientes superiores izquierdos!"),
            NewStep(MouthZone.UpperRight, "¡Cepilla los dientes superiores derechos!"),
            NewStep(MouthZone.FrontUpper, "¡Cepilla la parte delantera superior!"),
            NewStep(MouthZone.LowerLeft,  "¡Cepilla los dientes inferiores izquierdos!"),
            NewStep(MouthZone.LowerRight, "¡Cepilla los dientes inferiores derechos!"),
            NewStep(MouthZone.FrontLower, "¡Cepilla la parte delantera inferior!")
        };
    }

    static SessionStep NewStep(MouthZone zone, string instruction)
        => new SessionStep { zone = zone, duration = 20f, instructionText = instruction };
}

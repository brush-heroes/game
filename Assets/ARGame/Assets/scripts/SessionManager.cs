using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum BrushZone
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
    public BrushZone zone;
    public float duration;
    public string instructionText;
}

[Serializable]
public class SessionStepChangedEvent : UnityEvent<SessionStep, int> { }

public class SessionManager : MonoBehaviour
{
    [SerializeField] List<SessionStep> sessionSteps = new List<SessionStep>();
    [SerializeField] bool autoStart = true;
    [SerializeField] private BacteriaSpawner bacteriaSpawner;

    [Header("Unity Events (optional)")]
    [SerializeField] SessionStepChangedEvent onStepChangedUnity;
    [SerializeField] UnityEvent onSessionStartedUnity;
    [SerializeField] UnityEvent onSessionEndedUnity;

    int currentStepIndex;
    float stepTimer;
    bool completedAllSteps;

    [Header("Debug (Inspector only — not used by logic)")]
    [SerializeField] int debugCurrentStepIndex;
    [SerializeField] float debugRemainingTime;

    public bool IsSessionRunning { get; private set; }

    public int CurrentStepIndex => currentStepIndex;
    public BrushZone CurrentActiveZone => GetCurrentStep().zone;
    public BrushZone CurrentZone => CurrentActiveZone;
    public string CurrentInstructionText => GetCurrentStep().instructionText;
    public float RemainingTime => Mathf.Max(0f, stepTimer);
    public bool IsSessionComplete { get; private set; }

    public event Action<SessionStep, int> OnStepChanged;
    public event Action OnSessionStarted;
    public event Action OnSessionEnded;

    public UnityEvent<string> OnInstructionChanged = new UnityEvent<string>();

    void Start()
    {
        PopulateDefaultStepsIfEmpty();
        if (autoStart)
            StartSession();
    }

    void Update()
    {
        if (!IsSessionRunning)
        {
            debugCurrentStepIndex = currentStepIndex;
            debugRemainingTime = stepTimer;
            return;
        }

        if (bacteriaSpawner != null)
        {
            BrushZone currentZone = CurrentZone;
            Debug.Log("[SessionCheck] " + currentZone +
                      " | remaining: " + bacteriaSpawner.GetRemainingCount(currentZone));
            Debug.Log("[SessionManager] Checking zone: " + currentZone);

            bool isFull = bacteriaSpawner.IsZoneFullySpawned(currentZone);
            int remaining = bacteriaSpawner.GetRemainingCount(currentZone);
            int max = bacteriaSpawner.GetMaxCount(currentZone);

            Debug.Log("[SessionManager] Zone: " + currentZone +
                      " | Full: " + isFull +
                      " | Remaining: " + remaining +
                      " | Max: " + max);

            bool zoneComplete = bacteriaSpawner.IsZoneComplete(currentZone);
            Debug.Log("[SessionManager] Zone " + currentZone + " complete? " + zoneComplete);
            if (zoneComplete)
                AdvanceToNextStep();
        }

        debugCurrentStepIndex = currentStepIndex;
        debugRemainingTime = stepTimer;
    }

    public void StartSession()
    {
        PopulateDefaultStepsIfEmpty();
        if (sessionSteps == null || sessionSteps.Count == 0)
        {
            Debug.LogWarning("[SessionManager] Cannot start session: no steps defined.");
            return;
        }

        if (IsSessionRunning)
            return;

        ScoreManager.instance?.ResetSession();

        IsSessionRunning = true;
        IsSessionComplete = false;
        completedAllSteps = false;
        currentStepIndex = 0;

        Debug.Log("[SessionManager] Session started.");

        OnSessionStarted?.Invoke();
        onSessionStartedUnity?.Invoke();

        NotifyCurrentStepChanged();
    }

    public void AdvanceToNextStep()
    {
        if (!IsSessionRunning)
            return;

        currentStepIndex++;
        if (currentStepIndex >= sessionSteps.Count)
        {
            currentStepIndex = sessionSteps.Count - 1;
            completedAllSteps = true;
            EndSession();
            return;
        }

        NotifyCurrentStepChanged();
    }

    public void EndSession()
    {
        if (!IsSessionRunning)
            return;

        IsSessionRunning = false;
        IsSessionComplete = completedAllSteps;
        stepTimer = 0f;

        if (!IsSessionComplete)
            currentStepIndex = -1;

        Debug.Log($"[SessionManager] Session ended. Complete: {IsSessionComplete}");

        OnSessionEnded?.Invoke();
        onSessionEndedUnity?.Invoke();
    }

    void NotifyCurrentStepChanged()
    {
        SessionStep step = GetCurrentStep();
        stepTimer = 0f;

        Debug.Log($"[SessionManager] Step {currentStepIndex}: {step.zone} — \"{step.instructionText}\" (bacteria-based)");

        OnStepChanged?.Invoke(step, currentStepIndex);
        onStepChangedUnity?.Invoke(step, currentStepIndex);

        string instruction = EnsureNonEmptyInstruction(step.instructionText, step.zone);
        Debug.Log("[SessionManager] Sending instruction: " + instruction);
        OnInstructionChanged?.Invoke(instruction);
    }

    public SessionStep GetCurrentStep()
    {
        if (sessionSteps == null || sessionSteps.Count == 0 || currentStepIndex < 0 ||
            currentStepIndex >= sessionSteps.Count)
            return default;

        return sessionSteps[currentStepIndex];
    }

    static string EnsureNonEmptyInstruction(string text, BrushZone zone)
    {
        if (!string.IsNullOrEmpty(text))
            return text;
        return $"Keep brushing — {zone}!";
    }

    void PopulateDefaultStepsIfEmpty()
    {
        if (sessionSteps != null && sessionSteps.Count > 0)
            return;

        sessionSteps = new List<SessionStep>
        {
            NewDefaultStep(BrushZone.UpperLeft, "Brush your upper left teeth!"),
            NewDefaultStep(BrushZone.UpperRight, "Brush your upper right teeth!"),
            NewDefaultStep(BrushZone.FrontUpper, "Brush the front of your upper teeth!"),
            NewDefaultStep(BrushZone.LowerLeft, "Brush your lower left teeth!"),
            NewDefaultStep(BrushZone.LowerRight, "Brush your lower right teeth!"),
            NewDefaultStep(BrushZone.FrontLower, "Brush the front of your lower teeth!")
        };

        Debug.Log("[SessionManager] Populated default session steps (duration unused; steps advance per active zone completion).");
    }

    static SessionStep NewDefaultStep(BrushZone zone, string instruction)
    {
        return new SessionStep
        {
            zone = zone,
            duration = 10f,
            instructionText = instruction
        };
    }
}

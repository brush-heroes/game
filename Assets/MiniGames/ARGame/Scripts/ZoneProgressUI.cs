using UnityEngine;
using UnityEngine.UI;

// Shows 6 dot indicators in the HUD representing the 6 mouth zones.
// Green = completed, yellow = active, gray = pending.
public class ZoneProgressUI : MonoBehaviour
{
    [SerializeField] SessionManager sessionManager;
    [Tooltip("6 Image dots in zone order (UpperLeft, UpperRight, FrontUpper, LowerLeft, LowerRight, FrontLower)")]
    [SerializeField] Image[] dots;

    [SerializeField] Color completedColor = new Color(0.20f, 0.85f, 0.20f, 1f);
    [SerializeField] Color activeColor    = new Color(1.00f, 0.85f, 0.10f, 1f);
    [SerializeField] Color pendingColor   = new Color(0.35f, 0.35f, 0.35f, 0.55f);

    int _currentStep = -1;
    readonly bool[] _completed = new bool[6];

    void OnEnable()
    {
        if (sessionManager == null) return;
        sessionManager.OnSessionStarted += HandleStarted;
        sessionManager.OnStepChanged   += HandleStepChanged;
        sessionManager.OnSessionEnded  += HandleEnded;
        sessionManager.OnSessionFailed += HandleFailed;
    }

    void OnDisable()
    {
        if (sessionManager == null) return;
        sessionManager.OnSessionStarted -= HandleStarted;
        sessionManager.OnStepChanged   -= HandleStepChanged;
        sessionManager.OnSessionEnded  -= HandleEnded;
        sessionManager.OnSessionFailed -= HandleFailed;
    }

    void HandleStarted()
    {
        _currentStep = 0;
        System.Array.Clear(_completed, 0, _completed.Length);
        Refresh();
    }

    void HandleStepChanged(SessionStep step, int stepIndex)
    {
        if (_currentStep >= 0 && _currentStep < _completed.Length)
            _completed[_currentStep] = true;
        _currentStep = stepIndex;
        Refresh();
    }

    void HandleEnded()
    {
        if (_currentStep >= 0 && _currentStep < _completed.Length)
            _completed[_currentStep] = true;
        _currentStep = -1;
        Refresh();
    }

    void HandleFailed()
    {
        _currentStep = -1;
        Refresh();
    }

    void Refresh()
    {
        if (dots == null) return;
        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;
            if (i < _completed.Length && _completed[i])
                dots[i].color = completedColor;
            else if (i == _currentStep)
                dots[i].color = activeColor;
            else
                dots[i].color = pendingColor;
        }
    }
}

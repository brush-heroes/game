using UnityEngine;

// Bridges the AR session lifecycle to PlayerDataManager.
// Listens to SessionManager.OnSessionEnded / OnSessionFailed and reports a session record.
// Adding a similar recorder for another minigame is the only step needed to plug it
// into the global progress system — no changes to PlayerDataManager are required.
public class ARGameSessionRecorder : MonoBehaviour
{
    [SerializeField] SessionManager sessionManager;
    [SerializeField] ScoreManager scoreManager;

    void Awake()
    {
        if (sessionManager == null) sessionManager = FindObjectOfType<SessionManager>();
        if (scoreManager   == null) scoreManager   = ScoreManager.instance;
    }

    void OnEnable()
    {
        if (sessionManager == null) return;
        sessionManager.OnSessionEnded  += HandleEnded;
        sessionManager.OnSessionFailed += HandleFailed;
    }

    void OnDisable()
    {
        if (sessionManager == null) return;
        sessionManager.OnSessionEnded  -= HandleEnded;
        sessionManager.OnSessionFailed -= HandleFailed;
    }

    void HandleEnded()  => Report(true);
    void HandleFailed() => Report(false);

    void Report(bool completed)
    {
        int score    = scoreManager   != null ? scoreManager.Score                            : 0;
        int duration = sessionManager != null ? Mathf.RoundToInt(sessionManager.TotalSessionTime) : 0;
        PlayerDataManager.Instance.RecordSession(MinigameTypes.AR, score, duration, completed);
    }
}

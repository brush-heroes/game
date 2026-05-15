using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameMode { Dynamic, Guided }

// Manages StartScreen → GameHUD → EndScreen transitions and game-mode selection.
public class GameScreenManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SessionManager sessionManager;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] MouthZoneManager mouthZoneManager;
    [SerializeField] GuidedModeController guidedController;

    [Header("Screens")]
    [SerializeField] CanvasGroup startScreen;
    [SerializeField] CanvasGroup gameHUD;
    [SerializeField] CanvasGroup endScreen;
    [SerializeField] CanvasGroup pauseScreen;

    [Header("Face Tracking")]
    [Tooltip("Assign the FaceTrackingManager component. Start buttons are blocked until a face is detected.")]
    [SerializeField] FaceTrackingManager faceTracker;
    [Tooltip("Overlay shown on the start screen while searching for a face (e.g. 'Buscando rostro...').")]
    [SerializeField] CanvasGroup faceSearchOverlay;
    [Tooltip("Overlay shown mid-session when the face is lost — pauses time until face is detected again.")]
    [SerializeField] CanvasGroup faceLostOverlay;

    bool _pausedDueToFaceLoss;

    [Header("End Screen Content")]
    [SerializeField] Image resultImage;
    [SerializeField] TMP_Text resultTitle;
    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] TMP_Text resultMessage;
    [SerializeField] Sprite[] completionFrames;
    [SerializeField] Sprite[] failedFrames;

    [Header("HUD Content")]
    [SerializeField] TMP_Text timerText;
    [SerializeField] GameObject timerPanel;   // optional — hidden in Guided mode
    [SerializeField] GameObject scorePanel;   // optional — hidden in Guided mode
    [SerializeField] GameObject guidePanel;   // hidden on start/end screens

    [Header("Navigation")]
    [SerializeField] string menuSceneName = "MenuScene";
    [SerializeField] float fadeDuration = 0.4f;

    public GameMode CurrentMode { get; private set; }

    void Awake()
    {
        if (scoreManager == null) scoreManager = ScoreManager.instance;
    }

    void Start()
    {
        if (scoreManager == null) scoreManager = ScoreManager.instance;
        HideZoneSpheres();
        SetScreen(startScreen, true);
        SetScreen(gameHUD, false);
        SetScreen(endScreen, false);
        SetScreen(pauseScreen, false);
        SetScreen(faceLostOverlay, false);
        if (guidePanel != null) guidePanel.SetActive(false);

        // Show face-search overlay until a face is detected.
        // If faceTracker is not assigned the overlay stays hidden and behavior is unchanged.
        bool waitingForFace = faceTracker != null && !faceTracker.IsFaceTracked;
        SetScreen(faceSearchOverlay, waitingForFace);
    }

    void OnEnable()
    {
        if (sessionManager != null)
        {
            sessionManager.OnSessionStarted += HandleSessionStarted;
            sessionManager.OnSessionEnded  += HandleSessionEnded;
            sessionManager.OnSessionFailed += HandleSessionFailed;
        }
        if (faceTracker != null)
        {
            faceTracker.OnFaceDetected += HandleFaceDetected;
            faceTracker.OnFaceLost     += HandleFaceLost;
        }
    }

    void OnDisable()
    {
        if (sessionManager != null)
        {
            sessionManager.OnSessionStarted -= HandleSessionStarted;
            sessionManager.OnSessionEnded  -= HandleSessionEnded;
            sessionManager.OnSessionFailed -= HandleSessionFailed;
        }
        if (faceTracker != null)
        {
            faceTracker.OnFaceDetected -= HandleFaceDetected;
            faceTracker.OnFaceLost     -= HandleFaceLost;
        }
    }

    void Update()
    {
        if (timerText != null && sessionManager != null && sessionManager.IsSessionRunning
            && CurrentMode == GameMode.Dynamic)
            timerText.text = Mathf.CeilToInt(sessionManager.RemainingTime).ToString();
    }

    // ── Button callbacks ─────────────────────────────────────────────────────

    // Legacy single-button start (acts as Dynamic)
    public void OnStartButtonPressed() => OnDynamicButtonPressed();

    public void OnDynamicButtonPressed()
    {
        if (faceTracker != null && !faceTracker.IsFaceTracked) return;

        CurrentMode = GameMode.Dynamic;
        // Show guide panel BEFORE StartSession so BrushingGuideUI is subscribed for zone-1 events.
        if (guidePanel != null) guidePanel.SetActive(true);
        if (sessionManager != null) sessionManager.TotalTimeLimit = 180f;
        if (guidedController != null) guidedController.Deactivate();
        SetModeHUD(GameMode.Dynamic);
        sessionManager?.StartSession();
    }

    public void OnGuidedButtonPressed()
    {
        if (faceTracker != null && !faceTracker.IsFaceTracked) return;

        CurrentMode = GameMode.Guided;
        if (guidePanel != null) guidePanel.SetActive(true);
        if (sessionManager != null) sessionManager.TotalTimeLimit = 0f; // no time limit
        if (guidedController != null) guidedController.Activate();
        SetModeHUD(GameMode.Guided);
        sessionManager?.StartSession();
    }

    public void OnMenuButtonPressed()  => SceneManager.LoadScene(menuSceneName);
    public void OnRetryButtonPressed() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void OnPauseButtonPressed()
    {
        if (!sessionManager.IsSessionRunning) return;
        Time.timeScale = 0f;
        SetScreen(pauseScreen, true);
    }

    public void OnResumeButtonPressed()
    {
        Time.timeScale = 1f;
        SetScreen(pauseScreen, false);
    }

    public void OnPauseMenuButtonPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // ── Face tracking events ──────────────────────────────────────────────────

    void HandleFaceDetected()
    {
        // Start screen: hide the "searching" overlay so the player can press Start.
        if (startScreen != null && startScreen.alpha > 0f)
        {
            SetScreen(faceSearchOverlay, false);
            return;
        }

        // Mid-session: resume if we paused due to face loss.
        if (_pausedDueToFaceLoss)
        {
            _pausedDueToFaceLoss = false;
            SetScreen(faceLostOverlay, false);
            sessionManager?.Resume();
        }
    }

    void HandleFaceLost()
    {
        // Start screen: re-show the search overlay so buttons remain blocked.
        if (startScreen != null && startScreen.alpha > 0f)
        {
            SetScreen(faceSearchOverlay, true);
            return;
        }

        // Mid-session: pause the session and show the "put face back" overlay.
        // Skip if a manual pause is already active so we don't fight that flow.
        bool sessionRunning = sessionManager != null && sessionManager.IsSessionRunning;
        bool manuallyPaused = pauseScreen != null && pauseScreen.alpha > 0f;
        if (sessionRunning && !manuallyPaused && !_pausedDueToFaceLoss)
        {
            _pausedDueToFaceLoss = true;
            sessionManager.Pause();
            SetScreen(faceLostOverlay, true);
        }
    }

    // ── Session events ────────────────────────────────────────────────────────

    void HandleSessionStarted()
    {
        // guidePanel was already shown in the button handler before StartSession()
        StartCoroutine(FadeOut(startScreen));
        // blockRaycasts must be true so PauseButton (and any future HUD button) can be clicked.
        // Brushing still works in the rest of the screen because the HUD only covers the TopBar area.
        StartCoroutine(FadeIn(gameHUD, blockRaycasts: true));
    }

    void HandleSessionEnded()  => ShowEndScreen(success: true);
    void HandleSessionFailed() => ShowEndScreen(success: false);

    void ShowEndScreen(bool success)
    {
        if (guidePanel != null) guidePanel.SetActive(false);
        if (scoreManager != null && finalScoreText != null)
            finalScoreText.text = scoreManager.Score.ToString();

        if (resultTitle != null)
            resultTitle.text = success ? "¡Excelente!" : "¡Tiempo agotado!";

        if (resultMessage != null)
            resultMessage.text = success
                ? "¡Limpiaste todos tus dientes!"
                : "¡Inténtalo de nuevo, tú puedes!";

        if (resultImage != null)
        {
            var anim = resultImage.GetComponent<SpriteAnimator>();
            Sprite[] frames = success ? completionFrames : failedFrames;
            if (anim != null && frames != null && frames.Length > 0)
                anim.PlayLoop(frames);
        }

        StartCoroutine(FadeOut(gameHUD));
        StartCoroutine(FadeIn(endScreen));
    }

    void SetModeHUD(GameMode mode)
    {
        bool isDynamic = mode == GameMode.Dynamic;
        if (timerPanel != null) timerPanel.SetActive(isDynamic);
        if (scorePanel  != null) scorePanel.SetActive(isDynamic);
        if (timerText   != null && isDynamic) timerText.text = "180";
    }

    // ── Transitions ───────────────────────────────────────────────────────────

    static void SetScreen(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    IEnumerator FadeIn(CanvasGroup cg, bool blockRaycasts = true)
    {
        if (cg == null) yield break;
        cg.interactable = blockRaycasts;
        cg.blocksRaycasts = blockRaycasts;
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    IEnumerator FadeOut(CanvasGroup cg)
    {
        if (cg == null) yield break;
        for (float t = fadeDuration; t > 0f; t -= Time.deltaTime)
        {
            cg.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    // ── Zone sphere hiding ────────────────────────────────────────────────────

    void HideZoneSpheres()
    {
        if (mouthZoneManager == null) return;
        foreach (MouthZone zone in (MouthZone[])Enum.GetValues(typeof(MouthZone)))
        {
            Transform t = mouthZoneManager.GetZoneTransform(zone);
            if (t == null) continue;
            foreach (Renderer r in t.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
        }
    }
}

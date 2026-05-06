using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrushingScoreManager : MonoBehaviour
{
    public static BrushingScoreManager Instance { get; private set; }

    public event Action<int> ScoreChanged;

    [Header("Score Values")]
    [SerializeField] private int initialMouthDirtPoints = 10;
    [SerializeField] private int outsideMouthDirtPoints = 10;
    [SerializeField] private int tongueStage1DirtPoints = 20;
    [SerializeField] private int tongueStage2BadBacteriaPoints = 15;
    [SerializeField] private int tongueStage2GoodPenaltyPoints = 10;

    [Header("Optional Feedback")]
    [SerializeField] private TMP_Text feedbackTextTMP;
    [SerializeField] private Text feedbackTextLegacy;
    [SerializeField] private float feedbackDuration = 0.7f;
    [SerializeField] private Color gainColor = new Color(0.1f, 0.9f, 0.2f, 1f);
    [SerializeField] private Color lossColor = new Color(0.95f, 0.25f, 0.25f, 1f);
    [SerializeField] private Vector2 feedbackPositionJitter = new Vector2(60f, 35f);

    private StageTimer cachedStageTimer;
    private Coroutine feedbackRoutine;
    private int currentScore;
    private Vector2 feedbackTMPBaseAnchoredPos;
    private Vector2 feedbackLegacyBaseAnchoredPos;
    private bool feedbackPositionsCached;

    public int CurrentScore => currentScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CacheFeedbackBasePositions();
    }

    public void AddPoints(int amount, bool showFeedback = true)
    {
        if (amount <= 0)
            return;

        currentScore += amount;
        ScoreChanged?.Invoke(currentScore);

        if (showFeedback)
            ShowFeedback($"+{amount}", gainColor);
    }

    public void RemovePoints(int amount, bool showFeedback = true)
    {
        if (amount <= 0)
            return;

        currentScore = Mathf.Max(0, currentScore - amount);
        ScoreChanged?.Invoke(currentScore);

        if (showFeedback)
            ShowFeedback($"-{amount}", lossColor);
    }

    public void ResetScore(int newScore = 0)
    {
        currentScore = Mathf.Max(0, newScore);
        ScoreChanged?.Invoke(currentScore);
    }

    public void AddPointsForMouthDirt()
    {
        if (IsOutsideTimerRunning())
            AddPoints(outsideMouthDirtPoints);
        else
            AddPoints(initialMouthDirtPoints);
    }

    public void AddPointsForTongueStage1()
    {
        AddPoints(tongueStage1DirtPoints);
    }

    public void AddPointsForTongueStage2BadBacteria()
    {
        AddPoints(tongueStage2BadBacteriaPoints);
    }

    public void ApplyPenaltyForTongueStage2GoodItem()
    {
        RemovePoints(tongueStage2GoodPenaltyPoints);
    }

    private bool IsOutsideTimerRunning()
    {
        if (cachedStageTimer == null)
            cachedStageTimer = FindObjectOfType<StageTimer>();

        return cachedStageTimer != null && cachedStageTimer.isRunning;
    }

    private void ShowFeedback(string message, Color color)
    {
        if (feedbackTextTMP == null && feedbackTextLegacy == null)
            return;

        if (!feedbackPositionsCached)
            CacheFeedbackBasePositions();

        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(CoShowFeedback(message, color));
    }

    private IEnumerator CoShowFeedback(string message, Color color)
    {
        ApplyRandomFeedbackOffset();

        if (feedbackTextTMP != null)
        {
            feedbackTextTMP.gameObject.SetActive(true);
            feedbackTextTMP.text = message;
            feedbackTextTMP.color = color;
        }

        if (feedbackTextLegacy != null)
        {
            feedbackTextLegacy.gameObject.SetActive(true);
            feedbackTextLegacy.text = message;
            feedbackTextLegacy.color = color;
        }

        yield return new WaitForSeconds(feedbackDuration);

        if (feedbackTextTMP != null)
            feedbackTextTMP.gameObject.SetActive(false);

        if (feedbackTextLegacy != null)
            feedbackTextLegacy.gameObject.SetActive(false);

        feedbackRoutine = null;
    }

    private void CacheFeedbackBasePositions()
    {
        if (feedbackTextTMP != null)
            feedbackTMPBaseAnchoredPos = feedbackTextTMP.rectTransform.anchoredPosition;

        if (feedbackTextLegacy != null)
            feedbackLegacyBaseAnchoredPos = feedbackTextLegacy.rectTransform.anchoredPosition;

        feedbackPositionsCached = true;
    }

    private void ApplyRandomFeedbackOffset()
    {
        float randomX = UnityEngine.Random.Range(-feedbackPositionJitter.x, feedbackPositionJitter.x);
        float randomY = UnityEngine.Random.Range(-feedbackPositionJitter.y, feedbackPositionJitter.y);
        Vector2 offset = new Vector2(randomX, randomY);

        if (feedbackTextTMP != null)
            feedbackTextTMP.rectTransform.anchoredPosition = feedbackTMPBaseAnchoredPos + offset;

        if (feedbackTextLegacy != null)
            feedbackTextLegacy.rectTransform.anchoredPosition = feedbackLegacyBaseAnchoredPos + offset;
    }
}


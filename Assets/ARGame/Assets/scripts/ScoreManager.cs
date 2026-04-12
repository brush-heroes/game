using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [SerializeField] TextMeshProUGUI scoreText;

    [Header("Zone-aware cleaning")]
    [SerializeField] int pointsCorrectTargetZone = 2;
    [SerializeField] int pointsWrongZoneWhileTargetHasWork = -1;

    int score;
    private int totalSpawned;
    private int totalCleaned;
    private int cleansInTargetZone;
    private int cleansOutsideTargetZone;

    public int CleansInTargetZone => cleansInTargetZone;
    public int CleansOutsideTargetZone => cleansOutsideTargetZone;

    void Awake()
    {
        instance = this;
        score = 0;
        UpdateScoreText();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    public void RegisterSpawn()
    {
        totalSpawned++;
    }

    /// <summary>
    /// Wrong-zone cleans are only penalized while the current step's zone still has bacteria to finish
    /// (not fully spawned yet, or still alive). Cleaning another zone after the target is done is neutral.
    /// </summary>
    public void RegisterCleanResult(BrushZone cleanedZone, BrushZone currentTargetZone, bool targetZoneStillHasWork)
    {
        totalCleaned++;

        if (cleanedZone == currentTargetZone)
        {
            cleansInTargetZone++;
            AddScore(pointsCorrectTargetZone);
            return;
        }

        cleansOutsideTargetZone++;
        if (targetZoneStillHasWork)
            AddScore(pointsWrongZoneWhileTargetHasWork);
    }

    public float GetAccuracy()
    {
        if (totalSpawned == 0)
            return 0;

        return (float)totalCleaned / totalSpawned;
    }

    public float GetTargetZoneFocusRatio()
    {
        int zoneCleans = cleansInTargetZone + cleansOutsideTargetZone;
        if (zoneCleans == 0)
            return 0f;
        return (float)cleansInTargetZone / zoneCleans;
    }

    public void ResetSession()
    {
        score = 0;
        totalSpawned = 0;
        totalCleaned = 0;
        cleansInTargetZone = 0;
        cleansOutsideTargetZone = 0;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}

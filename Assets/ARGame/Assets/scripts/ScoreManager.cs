using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [SerializeField] TextMeshProUGUI scoreText;

    int score;
    private int totalSpawned;
    private int totalCleaned;

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
        Debug.Log("[Score] Spawned: " + totalSpawned);
    }

    public void RegisterClean()
    {
        totalCleaned++;
        Debug.Log("[Score] Cleaned: " + totalCleaned);
    }

    public float GetAccuracy()
    {
        if (totalSpawned == 0)
            return 0;

        return (float)totalCleaned / totalSpawned;
    }

    public void ResetSession()
    {
        score = 0;
        totalSpawned = 0;
        totalCleaned = 0;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
}

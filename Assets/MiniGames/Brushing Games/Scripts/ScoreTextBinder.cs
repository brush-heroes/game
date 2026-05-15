using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreTextBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreTextTMP;
    [SerializeField] private Text scoreTextLegacy;
    [SerializeField] private string prefix = "Score: ";

    private void Awake()
    {
        if (scoreTextTMP == null)
            scoreTextTMP = GetComponent<TMP_Text>();

        if (scoreTextLegacy == null)
            scoreTextLegacy = GetComponent<Text>();
    }

    private void OnEnable()
    {
        if (BrushingScoreManager.Instance != null)
        {
            BrushingScoreManager.Instance.ScoreChanged += HandleScoreChanged;
            HandleScoreChanged(BrushingScoreManager.Instance.CurrentScore);
        }
    }

    private void OnDisable()
    {
        if (BrushingScoreManager.Instance != null)
            BrushingScoreManager.Instance.ScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int score)
    {
        string value = prefix + score;

        if (scoreTextTMP != null)
            scoreTextTMP.text = value;

        if (scoreTextLegacy != null)
            scoreTextLegacy.text = value;
    }
}


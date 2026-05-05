using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Settings")]
    public float remainingTime = 15f;
    public int totalScore = 0;
    public bool isGameActive = true;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject finalPanel;
    public TextMeshProUGUI finalScoreText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1;
        if (finalPanel != null) finalPanel.SetActive(false);
        Debug.Log("Juego de Dental Floss iniciado");
    }

    void Update()
    {
        if (isGameActive)
        {
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                UpdateInterface();
            }
            else
            {
                EndGame();
            }
        }
    }

    void UpdateInterface()
    {
        if (timerText != null) timerText.text = "Tiempo: " + Mathf.Ceil(remainingTime).ToString();
        if (scoreText != null) scoreText.text = "Puntos: " + totalScore;
    }

    public void AddScore(int amount)
    {
        if (isGameActive)
        {
            ScoreSystem.Instance.AddScore(amount);
            totalScore += amount;
        }
    }

    void EndGame()
    {
        isGameActive = false;
        remainingTime = 0;

        if (finalPanel != null) finalPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Puntaje Final: " + totalScore;

        Time.timeScale = 0;
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MenuScene");
    }
}
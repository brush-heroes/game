using UnityEngine;
using TMPro; 

public class StageTimer : MonoBehaviour
{
    public float timeLeft = 10f;
    public bool isRunning = false;

    public TextMeshProUGUI timerText;
    public GameObject timerUI;

    public BushingDirtSpawner dirtSpawner;

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
        {
            timeLeft = 0;
            isRunning = false;
            UpdateUI();
            OnTimeEnd();
            return;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(timeLeft).ToString();
        }
    }

    void OnTimeEnd()
    {
        Debug.Log("Tiempo terminado");

        StopTimer();

        if (dirtSpawner != null)
        {
            dirtSpawner.StopSpawning();
        }
    }

    public void StartTimer(float duration)
    {
        timeLeft = duration;
        isRunning = true;

        if (timerUI != null)
            timerUI.SetActive(true);

        UpdateUI();
    }

    public void StopTimer()
    {
        isRunning = false;

        if (timerUI != null)
            timerUI.SetActive(false);
    }
}
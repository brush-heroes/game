using UnityEngine;
using TMPro; // si usas TextMeshPro

public class StageTimer : MonoBehaviour
{
    public float timeLeft = 10f;
    public bool isRunning = false;

    public TextMeshProUGUI timerText;

    public GameObject timerUI;

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0)
        {
            timeLeft = 0;
            isRunning = false;
            OnTimeEnd();
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

        StopTimer(); // 🔥 esto lo apaga automáticamente
    }

    public void StartTimer(float duration)
    {
        timeLeft = duration;
        isRunning = true;

        if (timerUI != null)
            timerUI.SetActive(true);
    }

    public void StopTimer()
    {
        isRunning = false;

        if (timerUI != null)
            timerUI.SetActive(false);
    }

}
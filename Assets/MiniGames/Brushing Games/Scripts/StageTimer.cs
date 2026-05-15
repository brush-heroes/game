using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class StageTimer : MonoBehaviour
{
    public event Action RightSequenceCompleted;
    public event Action LeftSequenceCompleted;

    public float timeLeft = 10f;
    public bool isRunning = false;

    public TextMeshProUGUI timerText;
    public GameObject timerUI;

    public enum BushingSide
    {
        Right,
        Left
    }

    [Header("Current Flow")]
    public BushingSide currentSide;
    public bool isOutsidePhase = true;
    public float transitionDelay = 2f;
    public float stageDuration = 15f;

    [Header("Right Stages")]
    public GameObject outsideStageRight;
    public GameObject insideStageRight;
    public BushingDirtSpawner outsideSpawnerRight;
    public BushingDirtSpawner insideSpawnerRight;

    [Header("Left Stages")]
    public GameObject outsideStageLeft;
    public GameObject insideStageLeft;
    public BushingDirtSpawner outsideSpawnerLeft;
    public BushingDirtSpawner insideSpawnerLeft;

    [Header("Teeth visuals (sucio / limpio por fase)")]
    public LateralZonesVisualManager lateralZonesVisual;

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
        StopTimer();
        StartCoroutine(HandleStageTransition());
    }

    private IEnumerator HandleStageTransition()
    {
        yield return new WaitForSeconds(transitionDelay);

        if (currentSide == BushingSide.Right)
        {
            HandleRightFlow();
        }
        else
        {
            HandleLeftFlow();
        }
    }

    void HandleRightFlow()
    {
        if (isOutsidePhase)
        {
            lateralZonesVisual?.EndPhase(LateralVisualPhase.OutsideRight);

            StopAndClearSpawner(outsideSpawnerRight);
            SetStageActive(outsideStageRight, false);
            SetStageActive(insideStageRight, true);
            StartSpawner(insideSpawnerRight);

            isOutsidePhase = false;
            lateralZonesVisual?.BeginPhase(LateralVisualPhase.InsideRight);
            StartTimer(stageDuration);
        }
        else
        {
            lateralZonesVisual?.EndPhase(LateralVisualPhase.InsideRight);

            StopAndClearSpawner(insideSpawnerRight);
            SetStageActive(insideStageRight, false);

            Debug.Log("Secuencia derecha terminada");
            RightSequenceCompleted?.Invoke();
        }
    }

    void HandleLeftFlow()
    {
        if (isOutsidePhase)
        {
            lateralZonesVisual?.EndPhase(LateralVisualPhase.OutsideLeft);

            StopAndClearSpawner(outsideSpawnerLeft);
            SetStageActive(outsideStageLeft, false);
            SetStageActive(insideStageLeft, true);
            StartSpawner(insideSpawnerLeft);

            isOutsidePhase = false;
            lateralZonesVisual?.BeginPhase(LateralVisualPhase.InsideLeft);
            StartTimer(stageDuration);
        }
        else
        {
            lateralZonesVisual?.EndPhase(LateralVisualPhase.InsideLeft);

            StopAndClearSpawner(insideSpawnerLeft);
            SetStageActive(insideStageLeft, false);

            Debug.Log("Secuencia izquierda terminada");
            LeftSequenceCompleted?.Invoke();
        }
    }

    void SetStageActive(GameObject stage, bool value)
    {
        if (stage != null)
        {
            stage.SetActive(value);
        }
    }

    void StartSpawner(BushingDirtSpawner spawner)
    {
        if (spawner != null)
        {
            spawner.StartSpawning();
        }
    }

    void StopAndClearSpawner(BushingDirtSpawner spawner)
    {
        if (spawner != null)
        {
            spawner.StopSpawning();
            spawner.ClearAll();
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

    public void StartRightSequence()
    {
        currentSide = BushingSide.Right;
        isOutsidePhase = true;

        SetStageActive(outsideStageRight, true);
        SetStageActive(insideStageRight, false);
        SetStageActive(outsideStageLeft, false);
        SetStageActive(insideStageLeft, false);

        lateralZonesVisual?.BeginPhase(LateralVisualPhase.OutsideRight);
        StartSpawner(outsideSpawnerRight);
        StartTimer(stageDuration);
    }

    public void StartLeftSequence()
    {
        currentSide = BushingSide.Left;
        isOutsidePhase = true;

        SetStageActive(outsideStageRight, false);
        SetStageActive(insideStageRight, false);
        SetStageActive(outsideStageLeft, true);
        SetStageActive(insideStageLeft, false);

        lateralZonesVisual?.BeginPhase(LateralVisualPhase.OutsideLeft);
        StartSpawner(outsideSpawnerLeft);
        StartTimer(stageDuration);
    }
}
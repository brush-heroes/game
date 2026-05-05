using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraTestInput : MonoBehaviour
{
    public CameraTransitionController cameraController;

    public StageTimer stageTimer;

    public BrushController brush;
    public TongueSwipeCleaningManager tongueSwipeCleaningManager;
    public TongueGameManager tongueGameManager;
    public Button startButton;
    public GameObject brushOff;
    public GameObject mouth;
    public GameObject ninoSucio;
    private bool tongueSequenceStarted;
    private bool gameStarted;

    private void Start()
    {
        ResolveStartButtonIfNeeded();

        if (BrushGameManager.Instance != null)
        {
            BrushGameManager.Instance.OutsideRightCompleted += HandleOutsideRightCompleted;
            BrushGameManager.Instance.OutsideLeftCompleted += HandleOutsideLeftCompleted;
            BrushGameManager.Instance.RightSideCompleted += HandleRightSideCompleted;
            BrushGameManager.Instance.LeftSideCompleted += HandleLeftSideCompleted;
        }

        if (stageTimer != null)
        {
            stageTimer.RightSequenceCompleted += HandleRightSequenceCompleted;
            stageTimer.LeftSequenceCompleted += HandleLeftSequenceCompleted;
        }

        if (tongueSwipeCleaningManager != null)
            tongueSwipeCleaningManager.CleaningCompleted += HandleTongueSwipeCompleted;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonPressed);
    }

    private void OnDestroy()
    {
        if (BrushGameManager.Instance != null)
        {
            BrushGameManager.Instance.OutsideRightCompleted -= HandleOutsideRightCompleted;
            BrushGameManager.Instance.OutsideLeftCompleted -= HandleOutsideLeftCompleted;
            BrushGameManager.Instance.RightSideCompleted -= HandleRightSideCompleted;
            BrushGameManager.Instance.LeftSideCompleted -= HandleLeftSideCompleted;
        }

        if (stageTimer != null)
        {
            stageTimer.RightSequenceCompleted -= HandleRightSequenceCompleted;
            stageTimer.LeftSequenceCompleted -= HandleLeftSequenceCompleted;
        }

        if (tongueSwipeCleaningManager != null)
            tongueSwipeCleaningManager.CleaningCompleted -= HandleTongueSwipeCompleted;

        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonPressed);
    }

    public void OnStartButtonPressed()
    {
        if (gameStarted)
            return;

        gameStarted = true;
        cameraController.GoToNormalView();

        if (brush != null)
        {
            brush.SetStartPose();
            brush.SetZoomMode(false);
        }

        if (ninoSucio != null)
            ninoSucio.SetActive(false);

        if (startButton != null)
            startButton.gameObject.SetActive(false);
    }

    private void ResolveStartButtonIfNeeded()
    {
        if (startButton != null)
            return;

        GameObject buttonObject = GameObject.Find("StartButton");
        if (buttonObject == null)
            buttonObject = GameObject.Find("startButton");

        if (buttonObject != null)
            startButton = buttonObject.GetComponent<Button>();

        if (startButton == null)
        {
            Debug.LogWarning("CameraTestInput: No se encontro 'StartButton'. Asignalo en el inspector.");
        }
    }

    private void HandleRightSideCompleted()
    {
        StartCoroutine(StartRightWithDelay());
    }

    private void HandleOutsideRightCompleted()
    {
        if (brush != null)
            brush.MirrorDirection();
    }

    private void HandleLeftSideCompleted()
    {
        StartCoroutine(StartLeftWithDelay());
    }

    private void HandleOutsideLeftCompleted()
    {
        if (brush != null)
            brush.MirrorDirection();
    }

    private void HandleRightSequenceCompleted()
    {
        ReturnToNormalView();
        if (brush != null)
            brush.RestoreSavedPose();
    }

    private void HandleLeftSequenceCompleted()
    {
        if (tongueSequenceStarted)
            return;

        tongueSequenceStarted = true;
        StartCoroutine(ReturnThenStartTongue());
    }

    private void ReturnToNormalView()
    {
        cameraController.GoToNormalView();
        brush.SetZoomMode(false);
    }

    private IEnumerator ReturnThenStartTongue()
    {
        ReturnToNormalView();
        yield return new WaitForSeconds(1.1f);
        yield return StartCoroutine(StartTongueWithDelay());
    }

    private IEnumerator StartRightWithDelay()
    {
        if (brush != null)
            brush.SetOutsideRightMinigamePose();

        cameraController.GoToOutsideZoomView();

        yield return new WaitForSeconds(1.1f);

        brush.SetZoomMode(true);

        if (stageTimer != null)
        {
            stageTimer.StartRightSequence();
        }
    }

    private IEnumerator StartLeftWithDelay()
    {
        if (brush != null)
            brush.SetOutsideLeftMinigamePose();

        cameraController.GoToLeftOutsideZoomView();

        yield return new WaitForSeconds(1.1f);

        brush.SetZoomMode(true);

        if (stageTimer != null)
        {
            stageTimer.StartLeftSequence();
        }
    }

    private IEnumerator StartTongueWithDelay()
    {
        cameraController.GoToTongueZoomView();

        yield return new WaitForSeconds(1.1f);

        if (tongueSwipeCleaningManager != null)
        {
            tongueSwipeCleaningManager.StartCleaningMechanic();
            yield break;
        }

        if (brushOff != null)
            brushOff.SetActive(false);

        if (tongueGameManager != null)
            tongueGameManager.StartTongueGame();
    }

    private void HandleTongueSwipeCompleted()
    {
        if (tongueSwipeCleaningManager != null)
            tongueSwipeCleaningManager.ClearSpawnedDirt();

        if (brushOff != null)
            brushOff.SetActive(false);

        if (tongueGameManager != null)
            tongueGameManager.StartTongueGame();
    }

}
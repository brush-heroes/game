using UnityEngine;
using System.Collections;

public class CameraTestInput : MonoBehaviour
{
    public CameraTransitionController cameraController;

    public StageTimer stageTimer;

    public BrushController brush;
    public TongueGameManager tongueGameManager;
    public GameObject brushOff;
    public GameObject mouth;
    public GameObject ninoSucio;
    private bool tongueSequenceStarted;

    private void Start()
    {
        if (BrushGameManager.Instance != null)
        {
            BrushGameManager.Instance.RightSideCompleted += HandleRightSideCompleted;
            BrushGameManager.Instance.LeftSideCompleted += HandleLeftSideCompleted;
        }

        if (stageTimer != null)
        {
            stageTimer.RightSequenceCompleted += HandleRightSequenceCompleted;
            stageTimer.LeftSequenceCompleted += HandleLeftSequenceCompleted;
        }
    }

    private void OnDestroy()
    {
        if (BrushGameManager.Instance != null)
        {
            BrushGameManager.Instance.RightSideCompleted -= HandleRightSideCompleted;
            BrushGameManager.Instance.LeftSideCompleted -= HandleLeftSideCompleted;
        }

        if (stageTimer != null)
        {
            stageTimer.RightSequenceCompleted -= HandleRightSequenceCompleted;
            stageTimer.LeftSequenceCompleted -= HandleLeftSequenceCompleted;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            cameraController.GoToNormalView();

            brush.SetZoomMode(false);

            if (ninoSucio != null)
                ninoSucio.SetActive(false);
        
        }
    }

    private void HandleRightSideCompleted()
    {
        StartCoroutine(StartRightWithDelay());
    }

    private void HandleLeftSideCompleted()
    {
        StartCoroutine(StartLeftWithDelay());
    }

    private void HandleRightSequenceCompleted()
    {
        ReturnToNormalView();
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
        if (brushOff != null)
            brushOff.SetActive(false);

        cameraController.GoToTongueZoomView();

        yield return new WaitForSeconds(1.1f);

        tongueGameManager.StartTongueGame();
    }

}
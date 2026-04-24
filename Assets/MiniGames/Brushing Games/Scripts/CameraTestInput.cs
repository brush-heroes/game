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
        if (brushOff != null)
            brushOff.SetActive(false);

        cameraController.GoToTongueZoomView();

        yield return new WaitForSeconds(1.1f);

        tongueGameManager.StartTongueGame();
    }

}
using UnityEngine;
using System.Collections;

public class CameraTestInput : MonoBehaviour
{
    public CameraTransitionController cameraController;

    public StageTimer stageTimer;

    public BrushController brush;
    public TongueGameManager tongueGameManager;
    public GameObject brushOff;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            cameraController.GoToNormalView();

            brush.SetZoomMode(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(StartRightWithDelay());
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartCoroutine(StartLeftWithDelay());
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartCoroutine(StartTongueWithDelay());
            
        }

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
using UnityEngine;
using System.Collections;

public class CameraTestInput : MonoBehaviour
{
    public CameraTransitionController cameraController;

    public StageTimer stageTimer;

    public BrushController brush;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            cameraController.GoToNormalView();

            brush.SetZoomMode(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(StartOutsideWithDelay());
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            cameraController.GoToInsideZoomView();
        }
    }

    private IEnumerator StartOutsideWithDelay()
    {
        cameraController.GoToOutsideZoomView();

        brush.SetZoomMode(true);

        yield return new WaitForSeconds(1.1f);

        stageTimer.StartTimer(10f);

    }
}
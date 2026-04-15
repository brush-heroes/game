using System.Collections;
using UnityEngine;

public class CameraTransitionController : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Targets")]
    public Transform normalView;
    public Transform outsideZoomView;
    public Transform OutsideLeftZoomView;

    [Header("Zoom Sizes")]
    public float normalSize = 5f;
    public float outsideZoomSize = 3f;
    public float outsideZoomSizeLeft = 3f;

    [Header("Speed")]
    public float moveDuration = 1f;

    private Coroutine currentTransition;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void GoToNormalView()
    {
        StartTransition(normalView, normalSize);
    }

    public void GoToOutsideZoomView()
    {
        StartTransition(outsideZoomView, outsideZoomSize);
    }

    public void GoToLeftOutsideZoomView()
    {
        StartTransition(OutsideLeftZoomView, outsideZoomSizeLeft);
    }

    private void StartTransition(Transform target, float targetSize)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(TransitionRoutine(target, targetSize));
    }

    private IEnumerator TransitionRoutine(Transform target, float targetSize)
    {
        Vector3 startPos = mainCamera.transform.position;
        float startSize = mainCamera.orthographicSize;

        Vector3 endPos = new Vector3(
            target.position.x,
            target.position.y,
            mainCamera.transform.position.z
        );

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = time / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        mainCamera.transform.position = endPos;
        mainCamera.orthographicSize = targetSize;
        currentTransition = null;
    }
}
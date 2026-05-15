using System;
using System.Collections;
using UnityEngine;

public class CameraTransitionController : MonoBehaviour
{
    public Camera mainCamera;

    [Header("Targets")]
    public Transform normalView;
    public Transform outsideZoomView;
    public Transform OutsideLeftZoomView;
    public Transform TongueZoomView;


    [Header("Zoom Sizes")]
    public float normalSize = 5f;
    public float initialStartSize = 10f;
    public float outsideZoomSize = 3f;
    public float outsideZoomSizeLeft = 3f;
    public float tongueZoomSize = 3f;

    [Header("Speed")]
    public float moveDuration = 1f;

    public float MoveDuration => moveDuration;

    private Coroutine currentTransition;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Start()
    {
        if (mainCamera == null)
            return;

        if (normalView != null)
        {
            Vector3 startPos = new Vector3(
                normalView.position.x,
                normalView.position.y,
                mainCamera.transform.position.z
            );
            mainCamera.transform.position = startPos;
        }

        mainCamera.orthographicSize = initialStartSize;
    }

    public void GoToNormalView()
    {
        StartTransition(normalView, normalSize);
    }

    /// <summary>Vista amplia inicial (misma que al cargar la escena).</summary>
    public void GoToInitialStartView()
    {
        StartTransition(normalView, initialStartSize);
    }

    /// <summary>Zoom out a la vista inicial; onProgress recibe t de 0 a 1 cada frame.</summary>
    public IEnumerator CoGoToInitialStartView(Action<float> onProgress = null)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        yield return CoTransitionTo(normalView, initialStartSize, onProgress);
        currentTransition = null;
    }

    public void GoToOutsideZoomView()
    {
        StartTransition(outsideZoomView, outsideZoomSize);
    }

    public void GoToLeftOutsideZoomView()
    {
        StartTransition(OutsideLeftZoomView, outsideZoomSizeLeft);
    }
    public void GoToTongueZoomView()
    {
        StartTransition(TongueZoomView, tongueZoomSize);
    }

    private void StartTransition(Transform target, float targetSize)
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);

        currentTransition = StartCoroutine(CoTransitionTo(target, targetSize));
    }

    private IEnumerator CoTransitionTo(Transform target, float targetSize, Action<float> onProgress = null)
    {
        if (mainCamera == null || target == null)
            yield break;

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
            float t = Mathf.Clamp01(time / moveDuration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, eased);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, eased);
            onProgress?.Invoke(eased);

            yield return null;
        }

        mainCamera.transform.position = endPos;
        mainCamera.orthographicSize = targetSize;
        onProgress?.Invoke(1f);
    }
}
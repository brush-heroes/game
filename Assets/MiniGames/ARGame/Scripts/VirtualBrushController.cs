using UnityEngine;

public class VirtualBrushController : MonoBehaviour
{
    [SerializeField] SessionManager sessionManager;
    [SerializeField] MouthZoneManager mouthZoneManager;
    [SerializeField] BacteriaSpawner bacteriaSpawner;
    [Tooltip("Optional: object to SetActive when session runs. If unset, Renderers on this object are toggled instead (keeps this script running).")]
    [SerializeField] GameObject brushRoot;

    [SerializeField] float distanceFromCamera = 0.3f;
    [SerializeField] float positionSmoothSpeed = 15f;
    [SerializeField] float cleaningRadius = 0.02f;

    bool brushShown = true;

    void Awake()
    {
        if (bacteriaSpawner == null)
            bacteriaSpawner = FindObjectOfType<BacteriaSpawner>();
    }

    void Update()
    {
        if (sessionManager == null)
        {
            SetBrushVisible(false);
            return;
        }

        if (!sessionManager.IsSessionRunning)
        {
            SetBrushVisible(false);
            return;
        }

        SetBrushVisible(true);

        if (Camera.main == null)
        {
            Debug.LogWarning("[VirtualBrushController] Camera.main is null; cannot place brush.");
            return;
        }

        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 screenPos = touch.position;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, distanceFromCamera)
            );

            transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * positionSmoothSpeed);
        }

        TryCleanBacteria2D();
    }

    void TryCleanBacteria2D()
    {
        if (mouthZoneManager == null)
            mouthZoneManager = FindObjectOfType<MouthZoneManager>();

        Transform zone = mouthZoneManager != null
            ? mouthZoneManager.GetZoneTransform(sessionManager.CurrentZone)
            : null;

        if (zone == null)
            return;

        Vector3 brushLocal = zone.InverseTransformPoint(transform.position);
        Vector2 brushPos2D = new Vector2(brushLocal.x, brushLocal.y);

        BrushZone targetZone = sessionManager.CurrentZone;
        bool targetStillHasWork = bacteriaSpawner != null &&
                                  (bacteriaSpawner.GetRemainingCount(targetZone) > 0 ||
                                   !bacteriaSpawner.IsZoneFullySpawned(targetZone));

        GameObject[] bacteriaObjects = GameObject.FindGameObjectsWithTag("Bacteria");
        for (int i = 0; i < bacteriaObjects.Length; i++)
        {
            GameObject bacteria = bacteriaObjects[i];
            if (bacteria == null)
                continue;

            Vector3 bacteriaLocal = zone.InverseTransformPoint(bacteria.transform.position);
            Vector2 bacteriaPos2D = new Vector2(bacteriaLocal.x, bacteriaLocal.y);
            float distance = Vector2.Distance(brushPos2D, bacteriaPos2D);

            if (distance < cleaningRadius)
            {
                Bacteria cleanable = bacteria.GetComponentInChildren<Bacteria>(true);
                if (cleanable != null)
                    cleanable.Clean(Time.deltaTime, targetZone, targetStillHasWork);
            }
        }
    }

    void SetBrushVisible(bool visible)
    {
        if (visible == brushShown)
            return;

        brushShown = visible;

        if (brushRoot != null)
        {
            brushRoot.SetActive(visible);
            return;
        }

        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            r.enabled = visible;
    }
}

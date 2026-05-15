using UnityEngine;

public class VirtualBrushController : MonoBehaviour
{
    [SerializeField] SessionManager sessionManager;
    [SerializeField] MouthZoneManager mouthZoneManager;
    [SerializeField] BacteriaSpawner bacteriaSpawner;
    [Tooltip("Optional: object to SetActive when session runs. If unset, all child Renderers are toggled.")]
    [SerializeField] GameObject brushRoot;

    [SerializeField] float distanceFromCamera = 0.3f;
    [SerializeField] float positionSmoothSpeed = 15f;
    [SerializeField] float cleaningRadius = 0.02f;

    [Header("Brush Offset")]
    [Tooltip("Lifts BrushVisual above the touch point so the handle sits at the finger")]
    [SerializeField] float brushVisualOffsetY = 0.04f;
    [Tooltip("Y offset from the brush root toward the bristle tip — used for cleaning collision")]
    [SerializeField] float bristlesOffsetY = 0.07f;

    [Header("Movement Detection")]
    [Tooltip("Minimum world-space distance per frame to count as moving")]
    [SerializeField] float movementThreshold = 0.003f;

    // Set to true by GuidedModeController — disables touch input while keeping movement detection.
    [HideInInspector] public bool GuidedMode = false;

    bool brushShown = true;
    Vector3 _prevPosition;

    public bool IsCleaning { get; private set; }
    public bool IsMoving   { get; private set; }

    void Awake()
    {
        if (bacteriaSpawner == null)
            bacteriaSpawner = FindObjectOfType<BacteriaSpawner>();

        Transform vis = transform.Find("BrushVisual");
        if (vis != null)
            vis.localPosition = new Vector3(0f, brushVisualOffsetY, 0f);

        _prevPosition = transform.position;
    }

    void Update()
    {
        if (sessionManager == null)
        {
            SetBrushVisible(false);
            IsCleaning = false;
            IsMoving   = false;
            return;
        }

        if (!sessionManager.IsSessionRunning)
        {
            SetBrushVisible(false);
            IsCleaning = false;
            IsMoving   = false;
            return;
        }

        if (sessionManager.IsPaused)
        {
            // Freeze brush input + cleaning while paused; keep visual on so the player sees state preserved.
            IsCleaning = false;
            IsMoving   = false;
            return;
        }

        SetBrushVisible(!GuidedMode);

        if (GuidedMode)
        {
            IsCleaning = false;
            IsMoving   = false;
            return;
        }

        if (Camera.main == null) return;

        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        // Touch input — only in dynamic mode and not during zone transitions
        if (!GuidedMode && !sessionManager.IsInTransition && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(touch.position.x, touch.position.y, distanceFromCamera));
            transform.position = Vector3.Lerp(transform.position, worldPos,
                                              Time.deltaTime * positionSmoothSpeed);
        }

        // Movement detection — works for both manual and guided motion
        IsMoving   = Vector3.Distance(transform.position, _prevPosition) > movementThreshold;
        _prevPosition = transform.position;

        // Cleaning — only in dynamic mode, only while moving, only outside transitions
        IsCleaning = false;
        if (!GuidedMode && IsMoving && !sessionManager.IsInTransition)
            TryCleanBacteria2D();
    }

    void TryCleanBacteria2D()
    {
        if (mouthZoneManager == null)
            mouthZoneManager = FindObjectOfType<MouthZoneManager>();

        Transform zone = mouthZoneManager != null
            ? mouthZoneManager.GetZoneTransform(sessionManager.CurrentZone)
            : null;

        if (zone == null) return;

        Vector3 bristlesWorld = transform.position + transform.up * bristlesOffsetY;
        Vector3 brushLocal  = zone.InverseTransformPoint(bristlesWorld);
        Vector2 brushPos2D  = new Vector2(brushLocal.x, brushLocal.y);

        MouthZone targetZone = sessionManager.CurrentZone;
        bool targetStillHasWork = bacteriaSpawner != null &&
                                  (bacteriaSpawner.GetRemainingCount(targetZone) > 0 ||
                                   !bacteriaSpawner.IsZoneFullySpawned(targetZone));

        GameObject[] bacteriaObjects = GameObject.FindGameObjectsWithTag("Bacteria");
        for (int i = 0; i < bacteriaObjects.Length; i++)
        {
            GameObject bacteria = bacteriaObjects[i];
            if (bacteria == null) continue;

            Vector3 bacteriaLocal = zone.InverseTransformPoint(bacteria.transform.position);
            Vector2 bacteriaPos2D = new Vector2(bacteriaLocal.x, bacteriaLocal.y);

            if (Vector2.Distance(brushPos2D, bacteriaPos2D) < cleaningRadius)
            {
                IsCleaning = true;
                Bacteria cleanable = bacteria.GetComponentInChildren<Bacteria>(true);
                if (cleanable != null)
                    cleanable.Clean(Time.deltaTime, targetZone, targetStillHasWork);
            }
        }
    }

    void SetBrushVisible(bool visible)
    {
        if (visible == brushShown) return;
        brushShown = visible;

        // Only toggle SpriteRenderers — 3D meshes (Capsule debug shape) stay hidden always.
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            r.enabled = (r is SpriteRenderer) && visible;
    }
}

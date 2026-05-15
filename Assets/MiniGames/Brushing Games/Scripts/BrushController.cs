using UnityEngine;


public class BrushController : MonoBehaviour
{
    private enum OrientationMode
    {
        Chewing,
        OutsideRight,
        OutsideLeft
    }

    public Vector3 normalScale = new Vector3(0.9f, 0.9f, 0.9f);
    public Vector3 zoomScale = new Vector3(0.01f, 0.01f, 0.01f);

    [Header("Rotación — zona inicial (masticación)")]
    public Vector3 bristlesUpRotationEuler = Vector3.zero;
    public Vector3 bristlesDownRotationEuler = new Vector3(0f, 0f, 180f);

    [Header("Rotación — laterales (base; espejo horizontal en X al aplicar)")]
    public Vector3 outsideRightMinigameRotationEuler = new Vector3(0f, 0f, 90f);
    public Vector3 outsideLeftMinigameRotationEuler = new Vector3(0f, 0f, -90f);

    private OrientationMode orientationMode = OrientationMode.Chewing;
    private int directionSign = 1;
    private bool isZoomMode;
    private Quaternion savedRotation;
    private Vector3 savedLocalScale;
    private int savedDirectionSign = 1;
    private OrientationMode savedOrientationMode;
    private bool hasSavedPose;
    private bool followEnabled = true;

    public void SetZoomMode(bool isZoom)
    {
        isZoomMode = isZoom;
        ApplyScaleWithDirection(isZoom ? zoomScale : normalScale);
    }

    public void MirrorDirection()
    {
        directionSign *= -1;
        ApplyScaleWithDirection(isZoomMode ? zoomScale : normalScale);
    }

    public void SetStartPose()
    {
        orientationMode = OrientationMode.Chewing;
        directionSign = 1;
        transform.rotation = Quaternion.Euler(bristlesDownRotationEuler);
        ApplyScaleWithDirection(isZoomMode ? zoomScale : normalScale);
    }

    public void SetOutsideRightMinigamePose()
    {
        SavePoseIfNeeded();
        ApplyOutsideRightLateralPose();
    }

    public void SetOutsideLeftMinigamePose()
    {
        SavePoseIfNeeded();
        ApplyOutsideLeftLateralPose();
    }

    /// <summary>Vuelve a aplicar rotación y espejo lateral tras SetZoomMode.</summary>
    public void RefreshLateralPoseAfterZoom()
    {
        if (orientationMode == OrientationMode.OutsideRight)
            ApplyOutsideRightLateralPose();
        else if (orientationMode == OrientationMode.OutsideLeft)
            ApplyOutsideLeftLateralPose();
    }

    public void RestoreSavedPose()
    {
        if (!hasSavedPose)
            return;

        transform.rotation = savedRotation;
        transform.localScale = savedLocalScale;
        directionSign = savedDirectionSign;
        orientationMode = savedOrientationMode;
        hasSavedPose = false;
    }

    public void SetFollowEnabled(bool enabled)
    {
        followEnabled = enabled;
    }

    private void SavePoseIfNeeded()
    {
        if (hasSavedPose)
            return;

        savedRotation = transform.rotation;
        savedLocalScale = transform.localScale;
        savedDirectionSign = directionSign;
        savedOrientationMode = orientationMode;
        hasSavedPose = true;
    }

    private void ApplyOutsideRightLateralPose()
    {
        orientationMode = OrientationMode.OutsideRight;
        transform.rotation = Quaternion.Euler(MirrorEulerHorizontal(outsideRightMinigameRotationEuler));
        directionSign = -1;
        ApplyScaleWithDirection(isZoomMode ? zoomScale : normalScale);
    }

    private void ApplyOutsideLeftLateralPose()
    {
        orientationMode = OrientationMode.OutsideLeft;
        transform.rotation = Quaternion.Euler(MirrorEulerHorizontal(outsideLeftMinigameRotationEuler));
        directionSign = 1;
        ApplyScaleWithDirection(isZoomMode ? zoomScale : normalScale);
    }

    private static Vector3 MirrorEulerHorizontal(Vector3 euler)
    {
        return new Vector3(euler.x, euler.y, -euler.z);
    }

    private void ApplyScaleWithDirection(Vector3 baseScale)
    {
        transform.localScale = new Vector3(
            Mathf.Abs(baseScale.x) * directionSign,
            baseScale.y,
            baseScale.z);
    }

    void Update()
    {
        if (!followEnabled)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        if (!TryGetPointerScreenPosition(out Vector2 pointerScreenPosition))
            return;

        Ray ray = cam.ScreenPointToRay(pointerScreenPosition);

        float distance = -cam.transform.position.z;

        Vector3 worldPos = ray.origin + ray.direction * distance;

        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
    }

    private bool TryGetPointerScreenPosition(out Vector2 position)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            position = touch.position;
            return true;
        }

        if (Input.mousePresent)
        {
            position = Input.mousePosition;
            return true;
        }

        position = default;
        return false;
    }
}

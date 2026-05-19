using UnityEngine;

public class BrushController : MonoBehaviour
{
    private enum OrientationMode
    {
        Default,
        DirectionPose,
        OutsideRightLateral,
        OutsideLeftLateral
    }

    public Vector3 normalScale = new Vector3(0.9f, 0.9f, 0.9f);
    public Vector3 zoomScale = new Vector3(0.01f, 0.01f, 0.01f);

    [Header("Chewing / Outside — sub-zonas Right y Left")]
    [Tooltip("Direction_Right (con espejo). Pon aquí la rotación horizontal si la quieres en la zona derecha.")]
    public Vector3 verticalBristlesRightEuler = new Vector3(0f, 0f, 90f);
    [Tooltip("Direction_Left (sin espejo respecto a Right).")]
    public Vector3 verticalBristlesLeftEuler = new Vector3(0f, 0f, 0f);

    [Header("Horizontal — zona Front (Chewing y Outside)")]
    public Vector3 frontHorizontalRotationEuler = new Vector3(0f, 0f, 90f);

    [Header("Inside — frente vertical")]
    public Vector3 insideFrontVerticalEuler = new Vector3(0f, 0f, 0f);

    [Header("Rotación — laterales zoom (círculos)")]
    public Vector3 outsideRightMinigameRotationEuler = new Vector3(0f, 0f, 90f);
    public Vector3 outsideLeftMinigameRotationEuler = new Vector3(0f, 0f, -90f);

    private OrientationMode orientationMode = OrientationMode.Default;
    private ZoneType directionZoneType;
    private BrushDirectionSubZone directionSubZone;
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
        orientationMode = OrientationMode.Default;
        directionSign = 1;
        transform.rotation = Quaternion.Euler(verticalBristlesLeftEuler);
        ApplyScaleWithDirection(isZoomMode ? zoomScale : normalScale);
    }

    public void ApplyDirectionPose(ZoneType zoneType, BrushDirectionSubZone subZone)
    {
        bool reversed;
        Vector3 euler = ResolveDirectionEuler(zoneType, subZone, out reversed);
        int targetSign = reversed ? -1 : 1;
        Quaternion targetRotation = Quaternion.Euler(euler);

        if (IsSameDirectionPose(zoneType, subZone, targetRotation, targetSign))
        {
            ApplyScaleWithDirection(isZoomMode ? zoomScale : normalScale);
            return;
        }

        orientationMode = OrientationMode.DirectionPose;
        directionZoneType = zoneType;
        directionSubZone = subZone;
        directionSign = targetSign;
        transform.rotation = targetRotation;
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

    /// <summary>Solo re-aplica escala tras zoom; la rotación lateral ya está fijada.</summary>
    public void RefreshLateralPoseAfterZoom()
    {
        ApplyScaleWithDirection(isZoomMode ? zoomScale : normalScale);
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

    private bool IsSameDirectionPose(
        ZoneType zoneType,
        BrushDirectionSubZone subZone,
        Quaternion targetRotation,
        int targetSign)
    {
        if (orientationMode != OrientationMode.DirectionPose)
            return false;

        if (directionZoneType != zoneType || directionSubZone != subZone)
            return false;

        if (Quaternion.Angle(transform.rotation, targetRotation) > 0.05f)
            return false;

        return Mathf.Sign(transform.localScale.x) == Mathf.Sign(targetSign);
    }

    private Vector3 ResolveDirectionEuler(
        ZoneType zoneType,
        BrushDirectionSubZone subZone,
        out bool reversed)
    {
        if (zoneType == ZoneType.Inside)
        {
            switch (subZone)
            {
                case BrushDirectionSubZone.Right:
                    reversed = false;
                    return verticalBristlesLeftEuler;
                case BrushDirectionSubZone.Left:
                    reversed = true;
                    return verticalBristlesLeftEuler;
                case BrushDirectionSubZone.Front:
                    reversed = false;
                    return insideFrontVerticalEuler;
                default:
                    reversed = false;
                    return verticalBristlesLeftEuler;
            }
        }

        switch (subZone)
        {
            case BrushDirectionSubZone.Right:
                reversed = true;
                return verticalBristlesRightEuler;
            case BrushDirectionSubZone.Left:
                reversed = false;
                return verticalBristlesLeftEuler;
            case BrushDirectionSubZone.Front:
                reversed = false;
                return frontHorizontalRotationEuler;
            default:
                reversed = true;
                return verticalBristlesRightEuler;
        }
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
        orientationMode = OrientationMode.OutsideRightLateral;
        transform.rotation = Quaternion.Euler(MirrorEulerHorizontal(outsideRightMinigameRotationEuler));
        directionSign = -1;
        ApplyScaleWithDirection(isZoomMode ? zoomScale : normalScale);
    }

    private void ApplyOutsideLeftLateralPose()
    {
        orientationMode = OrientationMode.OutsideLeftLateral;
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

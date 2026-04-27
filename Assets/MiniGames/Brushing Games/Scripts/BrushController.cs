using UnityEngine;


public class BrushController : MonoBehaviour
{

    public Vector3 normalScale = new Vector3(0.9f, 0.9f, 0.9f);
    public Vector3 zoomScale = new Vector3(0.01f, 0.01f, 0.01f);

    [Header("Brush Rotation")]
    public Vector3 bristlesUpRotationEuler = Vector3.zero;
    public Vector3 bristlesDownRotationEuler = new Vector3(0f, 0f, 180f);
    public Vector3 outsideRightMinigameRotationEuler = new Vector3(0f, 0f, 90f);
    public Vector3 outsideLeftMinigameRotationEuler = new Vector3(0f, 0f, -90f);
    private int directionSign = 1;
    private Quaternion savedRotation;
    private int savedDirectionSign = 1;
    private bool hasSavedPose;

    public void SetZoomMode(bool isZoom)
    {
        Vector3 baseScale = isZoom ? zoomScale : normalScale;
        transform.localScale = new Vector3(Mathf.Abs(baseScale.x) * directionSign, baseScale.y, baseScale.z);
    }

    public void MirrorDirection()
    {
        directionSign *= -1;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    public void SetStartPose()
    {
        transform.rotation = Quaternion.Euler(bristlesDownRotationEuler);
    }

    public void SetOutsideRightMinigamePose()
    {
        SavePoseIfNeeded();
        transform.rotation = Quaternion.Euler(outsideRightMinigameRotationEuler);
        directionSign = 1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void SetOutsideLeftMinigamePose()
    {
        SavePoseIfNeeded();

        transform.rotation = Quaternion.Euler(0f, 0f, -90f);
        directionSign = 1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void RestoreSavedPose()
    {
        if (!hasSavedPose)
            return;

        transform.rotation = savedRotation;
        directionSign = savedDirectionSign;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * directionSign, transform.localScale.y, transform.localScale.z);
        hasSavedPose = false;
    }

    private void SavePoseIfNeeded()
    {
        if (hasSavedPose)
            return;

        savedRotation = transform.rotation;
        savedDirectionSign = directionSign;
        hasSavedPose = true;
    }

    void Update()
    {
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
        // Android/iOS: usar touch como fuente principal.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            position = touch.position;
            return true;
        }

        // Fallback para pruebas en Editor/PC.
        if (Input.mousePresent)
        {
            position = Input.mousePosition;
            return true;
        }

        position = default;
        return false;
    }
}
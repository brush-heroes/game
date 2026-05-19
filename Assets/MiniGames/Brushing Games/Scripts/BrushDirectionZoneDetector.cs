using UnityEngine;

[RequireComponent(typeof(BrushController))]
public class BrushDirectionZoneDetector : MonoBehaviour
{
    [SerializeField] private BrushController brush;

    private BrushDirectionZone currentDirectionZone;

    private void Awake()
    {
        if (brush == null)
            brush = GetComponent<BrushController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (BrushGameManager.Instance != null && !BrushGameManager.Instance.AreDirectionZonesActive())
            return;

        BrushDirectionZone zone = GetDirectionZoneFromCollider(other);
        if (zone == null || !IsZoneRelevantForCurrentPhase(zone))
            return;

        if (currentDirectionZone == zone)
            return;

        currentDirectionZone = zone;
        brush.ApplyDirectionPose(zone.zoneType, zone.subZone);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BrushDirectionZone zone = GetDirectionZoneFromCollider(other);
        if (zone == null || currentDirectionZone != zone)
            return;

        currentDirectionZone = null;
    }

    private static BrushDirectionZone GetDirectionZoneFromCollider(Collider2D other)
    {
        BrushDirectionZone zone = other.GetComponent<BrushDirectionZone>();
        if (zone != null)
            return zone;

        return other.GetComponentInParent<BrushDirectionZone>();
    }

    private static bool IsZoneRelevantForCurrentPhase(BrushDirectionZone zone)
    {
        if (BrushGameManager.Instance == null)
            return true;

        return zone.zoneType == BrushGameManager.Instance.currentType;
    }

    public void ClearCurrentZone()
    {
        currentDirectionZone = null;
    }

    public static void ClearAllDetectors()
    {
        BrushDirectionZoneDetector[] detectors = FindObjectsOfType<BrushDirectionZoneDetector>();
        for (int i = 0; i < detectors.Length; i++)
            detectors[i].ClearCurrentZone();
    }
}

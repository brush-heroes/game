using UnityEngine;

public class MouthZoneManager : MonoBehaviour
{
    [SerializeField] Transform upperLeft;
    [SerializeField] Transform upperRight;
    [SerializeField] Transform frontUpper;
    [SerializeField] Transform lowerLeft;
    [SerializeField] Transform lowerRight;
    [SerializeField] Transform frontLower;

    public Transform GetTransformForZone(BrushZone zone)
    {
        return zone switch
        {
            BrushZone.UpperLeft => upperLeft,
            BrushZone.UpperRight => upperRight,
            BrushZone.FrontUpper => frontUpper,
            BrushZone.LowerLeft => lowerLeft,
            BrushZone.LowerRight => lowerRight,
            BrushZone.FrontLower => frontLower,
            _ => null
        };
    }

    public Transform GetZoneTransform(BrushZone zone) => GetTransformForZone(zone);
}

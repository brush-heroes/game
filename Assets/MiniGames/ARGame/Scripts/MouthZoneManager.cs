using UnityEngine;

public class MouthZoneManager : MonoBehaviour
{
    [SerializeField] Transform upperLeft;
    [SerializeField] Transform upperRight;
    [SerializeField] Transform frontUpper;
    [SerializeField] Transform lowerLeft;
    [SerializeField] Transform lowerRight;
    [SerializeField] Transform frontLower;

    public Transform GetTransformForZone(MouthZone zone)
    {
        return zone switch
        {
            MouthZone.UpperLeft => upperLeft,
            MouthZone.UpperRight => upperRight,
            MouthZone.FrontUpper => frontUpper,
            MouthZone.LowerLeft => lowerLeft,
            MouthZone.LowerRight => lowerRight,
            MouthZone.FrontLower => frontLower,
            _ => null
        };
    }

    public Transform GetZoneTransform(MouthZone zone) => GetTransformForZone(zone);
}

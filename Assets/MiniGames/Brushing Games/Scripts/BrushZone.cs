using UnityEngine;

public class BrushZone : MonoBehaviour
{
    public ZoneType zoneType;
    public ZoneSide zoneSide;
}

public enum ZoneType
{
    Chewing,
    Outside,
    Inside
}

public enum ZoneSide
{
    Left,
    Right
    
}
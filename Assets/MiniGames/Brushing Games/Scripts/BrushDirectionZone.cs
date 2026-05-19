using UnityEngine;

/// <summary>Sub-zona anatómica (izq / der / frente) dentro de Chewing, Outside o Inside.</summary>
public enum BrushDirectionSubZone
{
    Left,
    Right,
    Front
}

/// <summary>
/// Collider que define la orientación del cepillo según el área del diente.
/// Debe vivir como hijo del grupo de zonas activo (p. ej. Chewing_Right).
/// </summary>
public class BrushDirectionZone : MonoBehaviour
{
    public ZoneType zoneType;
    public BrushDirectionSubZone subZone;
}

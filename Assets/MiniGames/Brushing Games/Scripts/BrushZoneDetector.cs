using UnityEngine;

public class BrushZoneDetector : MonoBehaviour
{
    public BrushZone currentZone;

    private void OnTriggerEnter2D(Collider2D other)
    {
        BrushZone zone = other.GetComponent<BrushZone>();

        if (zone != null)
        {
            currentZone = zone;
            Debug.Log("Entró en zona: " + zone.zoneType + " - " + zone.zoneSide);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BrushZone zone = other.GetComponent<BrushZone>();

        if (zone != null && currentZone == zone)
        {
            currentZone = null;
            Debug.Log("Salió de la zona");
        }
    }

    public bool IsCorrectZone()
    {
        if (currentZone == null)
        {
            Debug.Log("No está en ninguna zona");
            return false;
        }

        if (BrushGameManager.Instance == null)
        {
            Debug.LogError("No hay BrushGameManager");
            return false;
        }

        bool correct =
            currentZone.zoneType == BrushGameManager.Instance.currentType &&
            currentZone.zoneSide == BrushGameManager.Instance.currentSide;

        Debug.Log("Zona actual: " + currentZone.zoneType + " - " + currentZone.zoneSide +
                  " | Esperado: " + BrushGameManager.Instance.currentType + " - " + BrushGameManager.Instance.currentSide +
                  " | Resultado: " + correct);

        return correct;
    }
}
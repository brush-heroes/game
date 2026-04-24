using UnityEngine;

public class BrushGameManager : MonoBehaviour
{
    public static BrushGameManager Instance;

    public ZoneType currentType;
    public ZoneSide currentSide;

    public GameObject chewingRightGroup;
    public GameObject outsideRightGroup;
    public GameObject insideRightGroup;
    public GameObject chewingLeftGroup;
    public GameObject outsideLeftGroup;
    public GameObject insideLeftGroup;

    public int cleaned = 0;
    public int target = 3;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
        currentType = ZoneType.Chewing;
        currentSide = ZoneSide.Right;
    }

    public void AddClean()
    {
        cleaned++;

        Debug.Log("Progreso: " + cleaned + "/" + target);

        if (cleaned >= target)
        {
            NextStep();
        }
    }

void NextStep()
{
    cleaned = 0;

    Debug.Log("Paso completado");

    // RIGHT SIDE

    if (currentType == ZoneType.Chewing && currentSide == ZoneSide.Right)
    {
        chewingRightGroup.SetActive(false);
        outsideRightGroup.SetActive(true);

        currentType = ZoneType.Outside;
    }

    else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Right)
    {
        outsideRightGroup.SetActive(false);
        insideRightGroup.SetActive(true);

        currentType = ZoneType.Inside;
    }

    else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Right)
    {
        insideRightGroup.SetActive(false);

        Debug.Log("Terminaste lado derecho");

        // PASAR A IZQUIERDA
        chewingLeftGroup.SetActive(true);

        currentType = ZoneType.Chewing;
        currentSide = ZoneSide.Left;

        Debug.Log("Nueva fase: Chewing Left");
    }

    // LEFT SIDE

    else if (currentType == ZoneType.Chewing && currentSide == ZoneSide.Left)
    {
        chewingLeftGroup.SetActive(false);
        outsideLeftGroup.SetActive(true);

        currentType = ZoneType.Outside;

        Debug.Log("Nueva fase: Outside Left");
    }

    else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Left)
    {
        outsideLeftGroup.SetActive(false);
        insideLeftGroup.SetActive(true);

        currentType = ZoneType.Inside;

        Debug.Log("Nueva fase: Inside Left");
    }

    else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Left)
    {
        insideLeftGroup.SetActive(false);

        Debug.Log("🎉 TERMINASTE TODO EL CEPILLADO 🎉");

    }
}
}
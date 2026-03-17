using UnityEngine;

public class BrushGameManager : MonoBehaviour
{
    public static BrushGameManager Instance;

    public ZoneType currentType;
    public ZoneSide currentSide;

    public int cleaned = 0;
    public int target = 3;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Empezamos con Chewing derecho
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

    // Chewing Right → Outside Right
    if (currentType == ZoneType.Chewing && currentSide == ZoneSide.Right)
    {
        currentType = ZoneType.Outside;
        currentSide = ZoneSide.Right;

        Debug.Log("Nueva fase: Outside Right");
    }

    // Outside Right → Inside Right
    else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Right)
    {
        currentType = ZoneType.Inside;
        currentSide = ZoneSide.Right;

        Debug.Log("Nueva fase: Inside Right");
    }

    // Inside Right → (luego izquierda)
    else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Right)
    {
        Debug.Log("Terminaste lado derecho 🔥");

        // luego aquí pasamos al lado izquierdo
    }
}
}
using System;
using UnityEngine;

public class BrushGameManager : MonoBehaviour
{
    public static BrushGameManager Instance;
    public event Action RightSideCompleted;
    public event Action LeftSideCompleted;
    public event Action BrushingCompleted;

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
        StartFromChewingRight();
    }

    public void StartFromChewingRight()
    {
        currentType = ZoneType.Chewing;
        currentSide = ZoneSide.Right;
        cleaned = 0;

        SetGroupActive(chewingRightGroup, true);
        SetGroupActive(outsideRightGroup, false);
        SetGroupActive(insideRightGroup, false);
        SetGroupActive(chewingLeftGroup, false);
        SetGroupActive(outsideLeftGroup, false);
        SetGroupActive(insideLeftGroup, false);
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
        SetGroupActive(chewingRightGroup, false);
        SetGroupActive(outsideRightGroup, true);

        currentType = ZoneType.Outside;
    }

    else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Right)
    {
        SetGroupActive(outsideRightGroup, false);
        SetGroupActive(insideRightGroup, true);

        currentType = ZoneType.Inside;
    }

    else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Right)
    {
        SetGroupActive(insideRightGroup, false);

        Debug.Log("Terminaste lado derecho");

        // PASAR A IZQUIERDA
        SetGroupActive(chewingLeftGroup, true);

        currentType = ZoneType.Chewing;
        currentSide = ZoneSide.Left;
        RightSideCompleted?.Invoke();

        Debug.Log("Nueva fase: Chewing Left");
    }

    // LEFT SIDE

    else if (currentType == ZoneType.Chewing && currentSide == ZoneSide.Left)
    {
        SetGroupActive(chewingLeftGroup, false);
        SetGroupActive(outsideLeftGroup, true);

        currentType = ZoneType.Outside;

        Debug.Log("Nueva fase: Outside Left");
    }

    else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Left)
    {
        SetGroupActive(outsideLeftGroup, false);
        SetGroupActive(insideLeftGroup, true);

        currentType = ZoneType.Inside;

        Debug.Log("Nueva fase: Inside Left");
    }

    else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Left)
    {
        SetGroupActive(insideLeftGroup, false);
        LeftSideCompleted?.Invoke();
        BrushingCompleted?.Invoke();

        Debug.Log("TERMINASTE TODO EL CEPILLADO");

    }
}

    private void SetGroupActive(GameObject group, bool isActive)
    {
        if (group == null)
            return;

        if (isActive)
            EnsureParentHierarchyActive(group.transform);

        group.SetActive(isActive);
    }

    private void EnsureParentHierarchyActive(Transform child)
    {
        Transform parent = child.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
                parent.gameObject.SetActive(true);

            parent = parent.parent;
        }
    }
}
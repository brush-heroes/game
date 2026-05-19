using System;
using UnityEngine;

public class BrushGameManager : MonoBehaviour
{
    public static BrushGameManager Instance;
    public event Action OutsideRightCompleted;
    public event Action OutsideLeftCompleted;
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

    [Header("Grupos de zonas (BrushZone / colliders) — misma secuencia que dirt")]
    [Tooltip("Misma fase que chewingRightGroup: solo uno activo a la vez.")]
    public GameObject chewingRightZonesGroup;
    public GameObject outsideRightZonesGroup;
    public GameObject insideRightZonesGroup;
    public GameObject chewingLeftZonesGroup;
    public GameObject outsideLeftZonesGroup;
    public GameObject insideLeftZonesGroup;

    public int cleaned = 0;
    public int target = 3;

    private bool directionZonesActive;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (autoStart)
            StartFromChewingRight();
    }

    [Tooltip("Si está desactivado, el minijuego no inicia hasta que alguien llame StartFromChewingRight().")]
    public bool autoStart = true;

    public void StartFromChewingRight()
    {
        currentType = ZoneType.Chewing;
        currentSide = ZoneSide.Right;
        cleaned = 0;

        SetDirtGroupActive(chewingRightGroup, true);
        SetDirtGroupActive(outsideRightGroup, false);
        SetDirtGroupActive(insideRightGroup, false);
        SetDirtGroupActive(chewingLeftGroup, false);
        SetDirtGroupActive(outsideLeftGroup, false);
        SetDirtGroupActive(insideLeftGroup, false);
        ActivateZonesForCurrentPhase();
    }

    /// <summary>Apaga todas las sub-zonas de dirección (p. ej. durante zoom o minijuego lateral).</summary>
    public bool AreDirectionZonesActive()
    {
        return directionZonesActive;
    }

    public void DeactivateAllZoneGroups()
    {
        directionZonesActive = false;
        SetGroupActive(chewingRightZonesGroup, false);
        SetGroupActive(outsideRightZonesGroup, false);
        SetGroupActive(insideRightZonesGroup, false);
        SetGroupActive(chewingLeftZonesGroup, false);
        SetGroupActive(outsideLeftZonesGroup, false);
        SetGroupActive(insideLeftZonesGroup, false);
        BrushDirectionZoneDetector.ClearAllDetectors();
    }

    /// <summary>Activa solo el grupo de zonas de la fase actual (Chewing/Outside/Inside + lado).</summary>
    public void ActivateZonesForCurrentPhase()
    {
        DeactivateAllZoneGroups();
        GameObject zoneGroup = GetZoneGroupFor(currentType, currentSide);
        if (zoneGroup != null)
        {
            SetGroupActive(zoneGroup, true);
            directionZonesActive = true;
        }
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

    private void NextStep()
    {
        cleaned = 0;

        Debug.Log("Paso completado");

        // RIGHT SIDE

        if (currentType == ZoneType.Chewing && currentSide == ZoneSide.Right)
        {
            SetDirtGroupActive(chewingRightGroup, false);
            SetDirtGroupActive(outsideRightGroup, true);

            currentType = ZoneType.Outside;
            ActivateZonesForCurrentPhase();
        }

        else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Right)
        {
            SetDirtGroupActive(outsideRightGroup, false);
            SetDirtGroupActive(insideRightGroup, true);

            currentType = ZoneType.Inside;
            ActivateZonesForCurrentPhase();
            OutsideRightCompleted?.Invoke();
        }

        else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Right)
        {
            SetDirtGroupActive(insideRightGroup, false);

            Debug.Log("Terminaste lado derecho");

            SetDirtGroupActive(chewingLeftGroup, true);

            currentType = ZoneType.Chewing;
            currentSide = ZoneSide.Left;
            DeactivateAllZoneGroups();
            RightSideCompleted?.Invoke();

            Debug.Log("Nueva fase: Chewing Left");
        }

        // LEFT SIDE

        else if (currentType == ZoneType.Chewing && currentSide == ZoneSide.Left)
        {
            SetDirtGroupActive(chewingLeftGroup, false);
            SetDirtGroupActive(outsideLeftGroup, true);

            currentType = ZoneType.Outside;
            ActivateZonesForCurrentPhase();

            Debug.Log("Nueva fase: Outside Left");
        }

        else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Left)
        {
            SetDirtGroupActive(outsideLeftGroup, false);
            SetDirtGroupActive(insideLeftGroup, true);

            currentType = ZoneType.Inside;
            ActivateZonesForCurrentPhase();
            OutsideLeftCompleted?.Invoke();

            Debug.Log("Nueva fase: Inside Left");
        }

        else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Left)
        {
            SetDirtGroupActive(insideLeftGroup, false);
            DeactivateAllZoneGroups();
            LeftSideCompleted?.Invoke();
            BrushingCompleted?.Invoke();

            Debug.Log("TERMINASTE TODO EL CEPILLADO");
        }
    }

    private GameObject GetZoneGroupFor(ZoneType type, ZoneSide side)
    {
        if (side == ZoneSide.Right)
        {
            switch (type)
            {
                case ZoneType.Chewing: return chewingRightZonesGroup;
                case ZoneType.Outside: return outsideRightZonesGroup;
                case ZoneType.Inside: return insideRightZonesGroup;
            }
        }
        else
        {
            switch (type)
            {
                case ZoneType.Chewing: return chewingLeftZonesGroup;
                case ZoneType.Outside: return outsideLeftZonesGroup;
                case ZoneType.Inside: return insideLeftZonesGroup;
            }
        }

        return null;
    }

    private void SetDirtGroupActive(GameObject dirtGroup, bool isActive)
    {
        SetGroupActive(dirtGroup, isActive);
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

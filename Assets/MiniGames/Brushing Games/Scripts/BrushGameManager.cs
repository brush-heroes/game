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

        SetDirtAndZoneGroupActive(chewingRightGroup, chewingRightZonesGroup, true);
        SetDirtAndZoneGroupActive(outsideRightGroup, outsideRightZonesGroup, false);
        SetDirtAndZoneGroupActive(insideRightGroup, insideRightZonesGroup, false);
        SetDirtAndZoneGroupActive(chewingLeftGroup, chewingLeftZonesGroup, false);
        SetDirtAndZoneGroupActive(outsideLeftGroup, outsideLeftZonesGroup, false);
        SetDirtAndZoneGroupActive(insideLeftGroup, insideLeftZonesGroup, false);
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
            SetDirtAndZoneGroupActive(chewingRightGroup, chewingRightZonesGroup, false);
            SetDirtAndZoneGroupActive(outsideRightGroup, outsideRightZonesGroup, true);

            currentType = ZoneType.Outside;
        }

        else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Right)
        {
            SetDirtAndZoneGroupActive(outsideRightGroup, outsideRightZonesGroup, false);
            SetDirtAndZoneGroupActive(insideRightGroup, insideRightZonesGroup, true);

            currentType = ZoneType.Inside;
            OutsideRightCompleted?.Invoke();
        }

        else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Right)
        {
            SetDirtAndZoneGroupActive(insideRightGroup, insideRightZonesGroup, false);

            Debug.Log("Terminaste lado derecho");

            // PASAR A IZQUIERDA
            SetDirtAndZoneGroupActive(chewingLeftGroup, chewingLeftZonesGroup, true);

            currentType = ZoneType.Chewing;
            currentSide = ZoneSide.Left;
            RightSideCompleted?.Invoke();

            Debug.Log("Nueva fase: Chewing Left");
        }

        // LEFT SIDE

        else if (currentType == ZoneType.Chewing && currentSide == ZoneSide.Left)
        {
            SetDirtAndZoneGroupActive(chewingLeftGroup, chewingLeftZonesGroup, false);
            SetDirtAndZoneGroupActive(outsideLeftGroup, outsideLeftZonesGroup, true);

            currentType = ZoneType.Outside;

            Debug.Log("Nueva fase: Outside Left");
        }

        else if (currentType == ZoneType.Outside && currentSide == ZoneSide.Left)
        {
            SetDirtAndZoneGroupActive(outsideLeftGroup, outsideLeftZonesGroup, false);
            SetDirtAndZoneGroupActive(insideLeftGroup, insideLeftZonesGroup, true);

            currentType = ZoneType.Inside;
            OutsideLeftCompleted?.Invoke();

            Debug.Log("Nueva fase: Inside Left");
        }

        else if (currentType == ZoneType.Inside && currentSide == ZoneSide.Left)
        {
            SetDirtAndZoneGroupActive(insideLeftGroup, insideLeftZonesGroup, false);
            LeftSideCompleted?.Invoke();
            BrushingCompleted?.Invoke();

            Debug.Log("TERMINASTE TODO EL CEPILLADO");
        }
    }

    /// <summary>
    /// Activa o desactiva el grupo de suciedad y el de zonas de detección de la misma fase (mismo estado).
    /// </summary>
    private void SetDirtAndZoneGroupActive(GameObject dirtGroup, GameObject zoneGroup, bool isActive)
    {
        SetGroupActive(dirtGroup, isActive);
        SetGroupActive(zoneGroup, isActive);
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

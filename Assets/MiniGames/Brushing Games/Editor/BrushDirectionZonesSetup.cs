using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Crea 3 sub-zonas de dirección (Left / Right / Front) bajo cada grupo de zonas del BrushGameManager.
/// </summary>
public static class BrushDirectionZonesSetup
{
    private const string MenuPath = "Tools/Brushing Games/Setup Direction Sub-Zones";

    [MenuItem(MenuPath)]
    public static void SetupAllZoneGroups()
    {
        BrushGameManager manager = Object.FindObjectOfType<BrushGameManager>();
        if (manager == null)
        {
            Debug.LogError("No se encontró BrushGameManager en la escena.");
            return;
        }

        SetupGroup(manager.chewingRightZonesGroup, ZoneType.Chewing);
        SetupGroup(manager.outsideRightZonesGroup, ZoneType.Outside);
        SetupGroup(manager.insideRightZonesGroup, ZoneType.Inside);
        SetupGroup(manager.chewingLeftZonesGroup, ZoneType.Chewing);
        SetupGroup(manager.outsideLeftZonesGroup, ZoneType.Outside);
        SetupGroup(manager.insideLeftZonesGroup, ZoneType.Inside);

        BrushController brush = Object.FindObjectOfType<BrushController>();
        if (brush != null && brush.GetComponent<BrushDirectionZoneDetector>() == null)
            brush.gameObject.AddComponent<BrushDirectionZoneDetector>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Sub-zonas de dirección creadas. Ajusta tamaño/posición de los BoxCollider2D en el Inspector.");
    }

    private static void SetupGroup(GameObject groupRoot, ZoneType zoneType)
    {
        if (groupRoot == null)
            return;

        CreateDirectionChildIfMissing(groupRoot.transform, zoneType, BrushDirectionSubZone.Left, new Vector3(-1.2f, 0f, 0f));
        CreateDirectionChildIfMissing(groupRoot.transform, zoneType, BrushDirectionSubZone.Right, new Vector3(1.2f, 0f, 0f));
        CreateDirectionChildIfMissing(groupRoot.transform, zoneType, BrushDirectionSubZone.Front, new Vector3(0f, 1.2f, 0f));
    }

    private static void CreateDirectionChildIfMissing(
        Transform parent,
        ZoneType zoneType,
        BrushDirectionSubZone subZone,
        Vector3 localPosition)
    {
        string objectName = "Direction_" + subZone;

        Transform existing = parent.Find(objectName);
        if (existing != null)
            return;

        var go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        var zone = go.AddComponent<BrushDirectionZone>();
        zone.zoneType = zoneType;
        zone.subZone = subZone;

        var collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(2.5f, 2.5f);
    }
}

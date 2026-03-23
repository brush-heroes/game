using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacteriaSpawner : MonoBehaviour
{
    class ZoneData
    {
        public BrushZone zone;
        public Transform transform;
        public int currentCount;
        public int maxCount;
        public float spawnTimer;
        public bool isFullySpawned;
        public float targetFillTime;
    }

    public GameObject bacteriaPrefab;

    [SerializeField] SessionManager sessionManager;
    [SerializeField] private int bacteriaPerZone = 10;

    MouthZoneManager mouthZoneManager;
    readonly List<GameObject> spawnedBacteria = new List<GameObject>();
    private Dictionary<BrushZone, ZoneData> zones = new Dictionary<BrushZone, ZoneData>();

    private bool isInitialized;
    private float sessionTimer;

    void Start()
    {
        StartCoroutine(InitializeRoutine());
    }

    void OnEnable()
    {
        if (sessionManager != null)
            sessionManager.OnSessionStarted += OnSessionStarted;
    }

    void OnDisable()
    {
        if (sessionManager != null)
            sessionManager.OnSessionStarted -= OnSessionStarted;
    }

    void OnSessionStarted()
    {
        ClearSpawnedBacteria();
        ResetAllZoneCounters();
    }

    void ResetAllZoneCounters()
    {
        sessionTimer = 0f;

        foreach (ZoneData data in zones.Values)
        {
            data.currentCount = 0;
            data.spawnTimer = 0f;
            data.isFullySpawned = false;
        }
    }

    IEnumerator InitializeRoutine()
    {
        while (mouthZoneManager == null)
        {
            mouthZoneManager = FindObjectOfType<MouthZoneManager>();
            Debug.Log("[BacteriaSpawner] Waiting for MouthZoneManager...");
            yield return null;
        }

        Debug.Log("[BacteriaSpawner] MouthZoneManager FOUND");

        InitializeZones();

        isInitialized = true;
    }

    void InitializeZones()
    {
        zones.Clear();

        int index = 0;

        foreach (BrushZone zone in System.Enum.GetValues(typeof(BrushZone)))
        {
            ZoneData data = new ZoneData();
            data.zone = zone;
            data.transform = mouthZoneManager.GetZoneTransform(zone);
            data.currentCount = 0;
            data.maxCount = bacteriaPerZone;
            data.spawnTimer = 0f;
            data.isFullySpawned = false;
            data.targetFillTime = 20f * (index + 1);

            zones.Add(zone, data);

            Debug.Log("[BacteriaSpawner] Initialized zone: " + zone);

            index++;
        }

        Debug.Log("[BacteriaSpawner] Total zones initialized: " + zones.Count);
    }

    void Update()
    {
        if (!isInitialized)
            return;

        if (sessionManager == null || !sessionManager.IsSessionRunning)
            return;

        if (bacteriaPrefab == null || zones.Count == 0)
            return;

        sessionTimer += Time.deltaTime;

        BrushZone activeZone = sessionManager.CurrentZone;

        foreach (var kvp in zones)
        {
            BrushZone zone = kvp.Key;
            ZoneData data = kvp.Value;

            data.isFullySpawned = data.currentCount >= data.maxCount;

            if (data.currentCount >= data.maxCount)
                continue;

            if (data.transform == null)
                continue;

            // Only the current brushing step receives timeline spawns; otherwise every zone
            // advances with sessionTimer and refills while the player cleans the active zone.
            if (zone != activeZone)
                continue;

            float expectedProgress = Mathf.Clamp01(sessionTimer / data.targetFillTime);

            int expectedCount = Mathf.FloorToInt(expectedProgress * data.maxCount);

            if (data.currentCount < expectedCount)
            {
                SpawnBacteriaInZone(data.transform, zone);
                data.currentCount++;

                Debug.Log("[TimelineSpawn] " + zone +
                          " | expected: " + expectedCount +
                          " | current: " + data.currentCount);
            }
        }
    }

    void SpawnBacteriaInZone(Transform zoneTransform, BrushZone zone)
    {
        if (bacteriaPrefab == null || zoneTransform == null)
            return;

        GameObject bacteria = Instantiate(bacteriaPrefab);

        Bacteria b = bacteria.GetComponent<Bacteria>();
        if (b == null)
        {
            Debug.LogError("Bacteria prefab missing Bacteria script!");
            Destroy(bacteria);
            return;
        }

        b.Initialize(zone);
        Debug.Log("[Spawner] Spawned bacteria in zone: " + zone);

        float radius = 0.02f;
        Vector2 random2D = UnityEngine.Random.insideUnitCircle * radius;

        float surfaceDistance = 0.015f;

        Vector3 worldPosition =
            zoneTransform.position +
            zoneTransform.right * random2D.x +
            zoneTransform.up * random2D.y +
            zoneTransform.forward * surfaceDistance;

        bacteria.transform.position = worldPosition;

        bacteria.transform.SetParent(zoneTransform, true);

        spawnedBacteria.Add(bacteria);
        ScoreManager.instance?.RegisterSpawn();
    }

    public int GetSpawnedCount(BrushZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return 0;

        int n = data.currentCount;
        Debug.Log("[BacteriaSpawner] GetSpawnedCount " + zone + ": " + n);
        return n;
    }

    public int GetRemainingCount(BrushZone zone)
    {
        Bacteria[] all = FindObjectsOfType<Bacteria>();

        int count = 0;

        foreach (var b in all)
        {
            if (b == null)
                continue;

            Debug.Log("[Check] Bacteria zone: " + b.Zone + " | Checking for: " + zone);

            if (b.Zone == zone)
                count++;
        }

        Debug.Log("[RemainingCount] Zone: " + zone + " | Count: " + count);

        return count;
    }

    public int GetMaxCount(BrushZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return 0;

        return data.maxCount;
    }

    public bool IsZoneFullySpawned(BrushZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return false;

        bool full = data.currentCount >= data.maxCount;
        Debug.Log("[BacteriaSpawner] IsZoneFullySpawned " + zone + ": " + full);
        return full;
    }

    public bool IsZoneComplete(BrushZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return false;

        int remaining = GetRemainingCount(zone);
        bool complete = data.currentCount >= data.maxCount && remaining == 0;

        Debug.Log("[ZoneCheck] " + zone +
                  " | spawned: " + data.currentCount +
                  " | max: " + data.maxCount +
                  " | remaining: " + remaining);

        return complete;
    }

    void ClearSpawnedBacteria()
    {
        for (int i = 0; i < spawnedBacteria.Count; i++)
        {
            if (spawnedBacteria[i] != null)
                Destroy(spawnedBacteria[i]);
        }

        spawnedBacteria.Clear();
    }
}

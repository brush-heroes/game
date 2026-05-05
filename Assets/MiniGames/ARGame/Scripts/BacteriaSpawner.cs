using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacteriaSpawner : MonoBehaviour
{
    class ZoneData
    {
        public MouthZone zone;
        public Transform transform;
        public int currentCount;
        public int aliveCount;
        public int maxCount;
        public float spawnTimer;
        public bool isFullySpawned;
        public float targetFillTime;
    }

    public GameObject bacteriaPrefab;

    [SerializeField] SessionManager sessionManager;
    [SerializeField] private int bacteriaPerZone = 10;

    [Header("Spawn Area")]
    [Tooltip("World-space radius around the zone centre where bacteria may appear.")]
    [SerializeField] float spawnRadius = 0.008f;

    [Header("Active zone pacing")]
    [Tooltip("While a zone is the current brushing step, it also fills on this clock so it feels urgent even if the global timeline was only halfway.")]
    [SerializeField] float activeZoneRampDuration = 12f;
    [Tooltip("How many bacteria the current step's zone may spawn per frame when catching up.")]
    [SerializeField] int activeZoneMaxSpawnsPerFrame = 4;

    MouthZoneManager mouthZoneManager;
    readonly List<GameObject> spawnedBacteria = new List<GameObject>();
    private Dictionary<MouthZone, ZoneData> zones = new Dictionary<MouthZone, ZoneData>();

    private bool isInitialized;
    private float sessionTimer;
    private float secondsIntoCurrentStep;
    private bool sessionClearInProgress;
    int _nextSortingOrder;

    void Start()
    {
        StartCoroutine(InitializeRoutine());
    }

    void OnEnable()
    {
        if (sessionManager != null)
        {
            sessionManager.OnSessionStarted += OnSessionStarted;
            sessionManager.OnStepChanged += OnSessionStepChanged;
        }
    }

    void OnDisable()
    {
        if (sessionManager != null)
        {
            sessionManager.OnSessionStarted -= OnSessionStarted;
            sessionManager.OnStepChanged -= OnSessionStepChanged;
        }
    }

    void OnSessionStepChanged(SessionStep step, int stepIndex)
    {
        secondsIntoCurrentStep = 0f;
        RefreshBacteriaTargetHighlights();
    }

    void OnSessionStarted()
    {
        sessionClearInProgress = true;
        ClearSpawnedBacteria();
        ResetAllZoneCounters();
        secondsIntoCurrentStep = 0f;
        sessionClearInProgress = false;
    }

    void ResetAllZoneCounters()
    {
        sessionTimer = 0f;
        _nextSortingOrder = 0;

        foreach (ZoneData data in zones.Values)
        {
            data.currentCount = 0;
            data.aliveCount = 0;
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

        foreach (MouthZone zone in System.Enum.GetValues(typeof(MouthZone)))
        {
            ZoneData data = new ZoneData();
            data.zone = zone;
            data.transform = mouthZoneManager.GetZoneTransform(zone);
            data.currentCount = 0;
            data.aliveCount = 0;
            data.maxCount = bacteriaPerZone;
            data.spawnTimer = 0f;
            data.isFullySpawned = false;
            data.targetFillTime = 60f * (index + 1);

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
        secondsIntoCurrentStep += Time.deltaTime;

        MouthZone activeZone = sessionManager.CurrentZone;

        foreach (var kvp in zones)
        {
            MouthZone zone = kvp.Key;
            ZoneData data = kvp.Value;

            data.isFullySpawned = data.currentCount >= data.maxCount;

            if (data.currentCount >= data.maxCount)
                continue;

            if (data.transform == null)
                continue;

            int expectedCount = GetExpectedSpawnCountForZone(data, zone == activeZone);
            expectedCount = Mathf.Min(expectedCount, data.maxCount);

            bool isActiveZone = zone == activeZone;
            int budget = isActiveZone ? Mathf.Max(1, activeZoneMaxSpawnsPerFrame) : 1;
            int spawnedBurst = 0;
            while (data.currentCount < expectedCount && spawnedBurst < budget)
            {
                SpawnBacteriaInZone(data, zone, activeZone);
                spawnedBurst++;
            }
        }
    }

    static int BackgroundExpectedCount(ZoneData data, float sessionTimer)
    {
        float p = Mathf.Clamp01(sessionTimer / data.targetFillTime);
        if (p >= 1f)
            return data.maxCount;
        return Mathf.FloorToInt(p * data.maxCount);
    }

    int FocusExpectedCountForActiveStep(ZoneData data)
    {
        float dur = Mathf.Max(0.01f, activeZoneRampDuration);
        float p = Mathf.Clamp01(secondsIntoCurrentStep / dur);
        if (p >= 1f)
            return data.maxCount;
        return Mathf.FloorToInt(p * data.maxCount);
    }

    int GetExpectedSpawnCountForZone(ZoneData data, bool isCurrentStepZone)
    {
        int background = BackgroundExpectedCount(data, sessionTimer);
        if (!isCurrentStepZone)
            return background;
        return Mathf.Max(background, FocusExpectedCountForActiveStep(data));
    }

    void RefreshBacteriaTargetHighlights()
    {
        if (sessionManager == null)
            return;

        MouthZone active = sessionManager.CurrentZone;
        for (int i = 0; i < spawnedBacteria.Count; i++)
        {
            GameObject go = spawnedBacteria[i];
            if (go == null)
                continue;
            Bacteria b = go.GetComponentInChildren<Bacteria>(true);
            if (b == null)
                continue;
            b.ApplyTargetHighlight(b.Zone == active);
        }
    }

    void SpawnBacteriaInZone(ZoneData data, MouthZone zone, MouthZone currentHighlightZone)
    {
        Transform zoneTransform = data.transform;
        if (bacteriaPrefab == null || zoneTransform == null)
            return;

        GameObject bacteria = Instantiate(bacteriaPrefab);

        Bacteria b = bacteria.GetComponentInChildren<Bacteria>(true);
        if (b == null)
        {
            Debug.LogError("Bacteria prefab missing Bacteria script (on root or children)!");
            Destroy(bacteria);
            return;
        }

        b.Initialize(this, zone);

        // Front zones span horizontally — use an ellipse wider than tall.
        Vector2 circle = UnityEngine.Random.insideUnitCircle;
        Vector2 random2D = (zone == MouthZone.FrontUpper || zone == MouthZone.FrontLower)
            ? new Vector2(circle.x * spawnRadius * 1.4f, circle.y * spawnRadius * 0.6f)
            : circle * spawnRadius;

        float surfaceDistance = 0.015f;

        Vector3 worldPosition =
            zoneTransform.position +
            zoneTransform.right * random2D.x +
            zoneTransform.up * random2D.y +
            zoneTransform.forward * surfaceDistance;

        bacteria.transform.position = worldPosition;

        // New bacteria appear behind previously spawned ones.
        var sr = bacteria.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null) sr.sortingOrder = _nextSortingOrder--;

        bacteria.transform.SetParent(zoneTransform, true);

        b.ApplyTargetHighlight(zone == currentHighlightZone);

        data.currentCount++;
        data.aliveCount++;
        spawnedBacteria.Add(bacteria);
        ARGameAudioManager.Instance?.PlayBacteriaBorn();
        ScoreManager.instance?.RegisterSpawn();
    }

    internal bool IsSessionClearInProgress => sessionClearInProgress;

    internal void NotifyBacteriaDestroyed(MouthZone zone)
    {
        if (sessionClearInProgress)
            return;

        if (!zones.TryGetValue(zone, out ZoneData data))
            return;

        data.aliveCount = Mathf.Max(0, data.aliveCount - 1);
    }

    public int GetSpawnedCount(MouthZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return 0;

        return data.currentCount;
    }

    public int GetRemainingCount(MouthZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return 0;

        return data.aliveCount;
    }

    public int GetMaxCount(MouthZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return 0;

        return data.maxCount;
    }

    public bool IsZoneFullySpawned(MouthZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return false;

        return data.currentCount >= data.maxCount;
    }

    public bool IsZoneComplete(MouthZone zone)
    {
        if (!zones.TryGetValue(zone, out ZoneData data))
            return false;

        int remaining = data.aliveCount;
        return data.currentCount >= data.maxCount && remaining == 0;
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

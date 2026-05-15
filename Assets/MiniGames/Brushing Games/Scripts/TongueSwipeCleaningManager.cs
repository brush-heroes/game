using System;
using System.Collections.Generic;
using UnityEngine;

public class TongueSwipeCleaningManager : MonoBehaviour
{
    public event Action CleaningCompleted;

    [Header("Tongue Bounds")]
    [SerializeField] private Collider2D tongueBoundsCollider;
    [SerializeField] private SpriteRenderer tongueBoundsSpriteRenderer;

    [Header("References")]
    [SerializeField] private Transform brushTransform;
    [SerializeField] private Transform dirtContainer;

    [Header("Dirt Prefabs")]
    [SerializeField] private GameObject[] dirtPrefabs;
    [SerializeField] private float dirtScaleMultiplier = 0.8f;

    [Header("Stroke Config")]
    [SerializeField] private float minVerticalDistance = 1.2f;
    [SerializeField] private float maxHorizontalTolerance = 0.5f;
    [SerializeField] private float resetDropDistance = 0.7f;
    [SerializeField] private int minRequiredStrokes = 5;
    [SerializeField] private int maxRequiredStrokes = 10;
    [SerializeField] private float spawnPadding = 0.08f;

    private readonly List<GameObject> spawnedDirt = new List<GameObject>();

    private bool mechanicActive;
    private bool waitingForReset;
    private bool currentStrokeCounted;

    private int requiredStrokes;
    private int completedStrokes;

    private Vector3 previousBrushPosition;
    private Vector3 strokeStartPosition;
    private float lastValidStrokeTopY;

    public void StartCleaningMechanic()
    {
        if (brushTransform == null)
        {
            Debug.LogWarning("TongueSwipeCleaningManager: Brush Transform no asignado.");
            return;
        }

        if (!HasValidBounds())
        {
            Debug.LogWarning("TongueSwipeCleaningManager: Asigna Collider2D o SpriteRenderer para TongueBounds.");
            return;
        }

        ClearSpawnedDirt();

        completedStrokes = 0;
        requiredStrokes = UnityEngine.Random.Range(minRequiredStrokes, maxRequiredStrokes + 1);
        waitingForReset = false;
        currentStrokeCounted = false;
        mechanicActive = true;

        previousBrushPosition = brushTransform.position;
        strokeStartPosition = brushTransform.position;

        SpawnDirt(requiredStrokes);

        Debug.Log($"[TongueSwipe] Pasadas necesarias: {requiredStrokes}");
    }

    private void Update()
    {
        if (!mechanicActive || brushTransform == null)
            return;

        Vector3 currentPosition = brushTransform.position;
        bool isInsideTongue = IsPointInsideTongue(currentPosition);

        if (!isInsideTongue)
        {
            if (!waitingForReset)
                strokeStartPosition = currentPosition;

            previousBrushPosition = currentPosition;
            return;
        }

        if (waitingForReset)
        {
            if (currentPosition.y <= lastValidStrokeTopY - resetDropDistance)
            {
                waitingForReset = false;
                currentStrokeCounted = false;
                strokeStartPosition = currentPosition;
                Debug.Log("[TongueSwipe] Cepillo bajo de nuevo: listo para contar otra pasada.");
            }

            previousBrushPosition = currentPosition;
            return;
        }

        if ((currentPosition - previousBrushPosition).sqrMagnitude > 0.000001f && !currentStrokeCounted)
        {
            EvaluateStroke(currentPosition);
        }

        previousBrushPosition = currentPosition;
    }

    private void EvaluateStroke(Vector3 currentPosition)
    {
        float deltaY = currentPosition.y - strokeStartPosition.y;
        float deltaX = currentPosition.x - strokeStartPosition.x;

        bool enoughVerticalDistance = deltaY >= minVerticalDistance;
        bool withinHorizontalTolerance = Mathf.Abs(deltaX) <= maxHorizontalTolerance;
        bool mainlyVertical = Mathf.Abs(deltaY) > Mathf.Abs(deltaX);
        bool movingUp = currentPosition.y > previousBrushPosition.y;

        if (!enoughVerticalDistance || !withinHorizontalTolerance || !mainlyVertical || !movingUp)
            return;

        currentStrokeCounted = true;
        waitingForReset = true;
        lastValidStrokeTopY = currentPosition.y;
        completedStrokes++;

        Debug.Log($"[TongueSwipe] Pasada valida detectada: {completedStrokes}/{requiredStrokes}");

        RemoveOneDirtInOrder();

        if (completedStrokes >= requiredStrokes)
        {
            CompleteMechanic();
        }
    }

    private void SpawnDirt(int amount)
    {
        if (dirtPrefabs == null || dirtPrefabs.Length == 0)
        {
            Debug.LogWarning("TongueSwipeCleaningManager: No hay prefabs de suciedad asignados.");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            GameObject prefab = dirtPrefabs[UnityEngine.Random.Range(0, dirtPrefabs.Length)];
            Vector2 spawnPosition = GetRandomPointInsideTongue();
            GameObject dirt = Instantiate(prefab, spawnPosition, Quaternion.identity, dirtContainer);
            dirt.transform.localScale *= dirtScaleMultiplier;
            spawnedDirt.Add(dirt);
        }
    }

    private Vector2 GetRandomPointInsideTongue()
    {
        Bounds bounds = GetTongueBounds();
        Vector2 candidate = Vector2.zero;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            float x = UnityEngine.Random.Range(bounds.min.x + spawnPadding, bounds.max.x - spawnPadding);
            float y = UnityEngine.Random.Range(bounds.min.y + spawnPadding, bounds.max.y - spawnPadding);
            candidate = new Vector2(x, y);

            if (IsPointInsideTongue(candidate))
                return candidate;
        }

        return bounds.center;
    }

    private void RemoveOneDirtInOrder()
    {
        GameObject topMostDirt = null;
        float topMostY = float.MinValue;

        for (int i = 0; i < spawnedDirt.Count; i++)
        {
            if (spawnedDirt[i] == null || !spawnedDirt[i].activeSelf)
                continue;

            float candidateY = spawnedDirt[i].transform.position.y;
            if (candidateY > topMostY)
            {
                topMostY = candidateY;
                topMostDirt = spawnedDirt[i];
            }
        }

        if (topMostDirt == null)
            return;

        topMostDirt.SetActive(false);
        if (BrushingScoreManager.Instance != null)
            BrushingScoreManager.Instance.AddPointsForTongueStage1();

        Debug.Log($"[TongueSwipe] Suciedad eliminada arriba->abajo en Y={topMostY:F2}");
    }

    private void CompleteMechanic()
    {
        mechanicActive = false;
        Debug.Log("[TongueSwipe] Mecanica completada. Lengua limpia.");
        CleaningCompleted?.Invoke();
    }

    public void ClearSpawnedDirt()
    {
        for (int i = 0; i < spawnedDirt.Count; i++)
        {
            if (spawnedDirt[i] != null)
                Destroy(spawnedDirt[i]);
        }

        spawnedDirt.Clear();
    }

    private bool HasValidBounds()
    {
        return tongueBoundsCollider != null || tongueBoundsSpriteRenderer != null;
    }

    private bool IsPointInsideTongue(Vector2 point)
    {
        if (tongueBoundsCollider != null)
            return tongueBoundsCollider.OverlapPoint(point);

        if (tongueBoundsSpriteRenderer != null)
            return tongueBoundsSpriteRenderer.bounds.Contains(point);

        return false;
    }

    private Bounds GetTongueBounds()
    {
        if (tongueBoundsCollider != null)
            return tongueBoundsCollider.bounds;

        return tongueBoundsSpriteRenderer.bounds;
    }
}

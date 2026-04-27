using System.Collections.Generic;
using UnityEngine;

public class BrushCircleDetector : MonoBehaviour
{
    public float closeDistance = 1.0f;
    public int minPoints = 6;
    public float minMovement = 0.05f;

    public BrushCleaner brushCleaner;

    private List<Vector2> points = new List<Vector2>();

    void Update()
    {
        if (TryGetPointerWorldPosition(out Vector2 pointerWorldPosition))
        {
            // evitar puntos duplicados
            if (points.Count == 0 || Vector2.Distance(points[points.Count - 1], pointerWorldPosition) > minMovement)
            {
                points.Add(pointerWorldPosition);
            }

            if (points.Count > 20)
                points.RemoveAt(0);

            if (IsCircle())
            {
                Debug.Log("CIRCULO DETECTADO");

            
                if (brushCleaner != null && brushCleaner.currentDirt != null)
                {
                    brushCleaner.currentDirt.TakeDamage(1);
                }

                points.Clear();
            }
        }
        else
        {
            points.Clear();
        }
    }

    private bool TryGetPointerWorldPosition(out Vector2 worldPosition)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            worldPosition = default;
            return false;
        }

        if (Input.touchCount > 0)
        {
            worldPosition = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
            return true;
        }

        if (Input.GetMouseButton(0))
        {
            worldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
            return true;
        }

        worldPosition = default;
        return false;
    }

    bool IsCircle()
{
    if (points.Count < minPoints) return false;

    float distanceStartEnd = Vector2.Distance(points[0], points[points.Count - 1]);
    float totalDistance = GetTotalDistance();

    int directionChanges = 0;

    for (int i = 2; i < points.Count; i++)
    {
        Vector2 dir1 = (points[i - 1] - points[i - 2]).normalized;
        Vector2 dir2 = (points[i] - points[i - 1]).normalized;

        float dot = Vector2.Dot(dir1, dir2);

        if (dot < 0.9f)
        {
            directionChanges++;
        }
    }

    return distanceStartEnd < closeDistance 
        && (directionChanges > 3 || totalDistance > 0.8f);
}

    float GetTotalDistance()
    {
        float total = 0f;

        for (int i = 1; i < points.Count; i++)
        {
            total += Vector2.Distance(points[i - 1], points[i]);
        }

        return total;
    }
}
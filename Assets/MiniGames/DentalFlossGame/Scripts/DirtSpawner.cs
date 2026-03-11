using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtSpawner : MonoBehaviour
{
    public GameObject dirtPrefab;
    public float spawnInterval = 2f;
    public Vector2 spawnArea = new Vector2(2f, 4f);

    void Start()
    {
        InvokeRepeating("SpawnDirt", 1f, spawnInterval);
    }

    void SpawnDirt()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameActive)
        {
            // 1. Elegimos lado (-1 izquierda, 1 derecha)
            float side = (Random.value > 0.5f) ? -1f : 1f;

            // 2. Definimos los límites basados en tu medida
            float minX = 4.24f; // El borde donde empieza el diente
            float maxX = 6.0f;  // El borde exterior (ajústalo si se salen mucho)

            // 3. Calculamos posición X (minX a maxX) y luego multiplicamos por el lado
            float posicionX = Random.Range(minX, maxX) * side;
            float posicionY = Random.Range(-spawnArea.y, spawnArea.y);

            // 4. Posición final relativa al Spawner
            Vector3 finalPos = transform.position + new Vector3(posicionX, posicionY, 0);

            Instantiate(dirtPrefab, finalPos, Quaternion.identity);
        }
    }
}
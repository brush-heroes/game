using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtSpawner : MonoBehaviour
{
    public GameObject dirtPrefab; // Aquí arrastras tu Prefab azul desde el Inspector
    public float spawnInterval = 2f;
    public Vector2 spawnArea = new Vector2(2f, 4f); // Rango de aparición

    void Start()
    {
        // Empezar a generar suciedad cada cierto tiempo
        InvokeRepeating("SpawnDirt", 1f, spawnInterval);
    }

    void SpawnDirt()
    {
        // Generar una posición aleatoria dentro del rango
        float randomX = Random.Range(-spawnArea.x, spawnArea.x);
        float randomY = Random.Range(-spawnArea.y, spawnArea.y);
        Vector3 spawnPos = new Vector3(randomX, randomY, 0);

        // Crear la instancia del Prefab
        Instantiate(dirtPrefab, spawnPos, Quaternion.identity);
    }
}
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
        // Elegimos lado: -1 para izquierda, 1 para derecha
        float lado = (Random.value > 0.5f) ? -1f : 1f;

        // Distancia desde el centro. 
        // Prueba con 3.5f. Si quedan muy al centro, sube a 3.8f.
        float posicionX = 3.5f * lado;

        float posicionY = Random.Range(-spawnArea.y, spawnArea.y);

        Instantiate(dirtPrefab, new Vector3(posicionX, posicionY, 0), Quaternion.identity);
    }
}
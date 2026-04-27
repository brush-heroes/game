using UnityEngine;

public class BushingDirtSpawner : MonoBehaviour
{
    public GameObject dirtPrefab;
    public BoxCollider2D spawnArea;

    public int maxDirt = 5;
    public float spawnInterval = 1.5f;

    private float timer;
    private bool isActive = false;

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            TrySpawn();
            timer = 0f;
        }
    }

    void TrySpawn()
    {
        if (transform.childCount >= maxDirt) return;

        SpawnDirt();
    }

    void SpawnDirt()
    {
        Bounds bounds = spawnArea.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        Vector2 spawnPos = new Vector2(randomX, randomY);

        Instantiate(dirtPrefab, spawnPos, Quaternion.identity, transform);
    }

    public void StartSpawning()
    {
        isActive = true;
        timer = 0f;
    }

    public void StopSpawning()
    {
        isActive = false;
    }

    public void ClearAll()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
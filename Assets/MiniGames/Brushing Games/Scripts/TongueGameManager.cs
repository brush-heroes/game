using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TongueGameManager : MonoBehaviour
{
    [Header("Zona de movimiento")]
    public Collider2D tongueBounds;

    [Header("Prefabs")]
    public GameObject[] dirtPrefabs;
    public GameObject[] hygienePrefabs;

    [Header("Cantidad de objetos")]
    public int dirtAmount = 8;
    public int hygieneAmount = 5;

    [Header("Contenedor")]
    public Transform itemsContainer;

    [Header("UI")]
    public TextMeshProUGUI messageText;

    [Header("Spawn")]
    public float spawnPadding = 0.3f;

    private int remainingDirt;
    private bool gameActive;

    private readonly List<GameObject> spawnedItems = new List<GameObject>();

    private void Update()
    {
        if (!gameActive) return;

        HandleInput();
    }

    public void StartTongueGame()
    {
        ClearItems();

        gameActive = true;
        remainingDirt = dirtAmount;

        if (messageText != null)
            messageText.text = "";

        SpawnItems(dirtPrefabs, dirtAmount, true);
        SpawnItems(hygienePrefabs, hygieneAmount, false);
    }

    private void SpawnItems(GameObject[] prefabs, int amount, bool isDirt)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        for (int i = 0; i < amount; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Vector2 spawnPosition = GetRandomPositionInsideTongue();

            GameObject item = Instantiate(prefab, spawnPosition, Quaternion.identity, itemsContainer);

            BrushingTongueItem tongueItem = item.GetComponent<BrushingTongueItem>();
            tongueItem.isDirt = isDirt;
            tongueItem.Init(tongueBounds, this);

            spawnedItems.Add(item);
        }
    }

private Vector2 GetRandomPositionInsideTongue()
{
    Bounds bounds = tongueBounds.bounds;

    Vector2 randomPoint;
    int attempts = 0;

    do
    {
        float x = Random.Range(bounds.min.x + spawnPadding, bounds.max.x - spawnPadding);
        float y = Random.Range(bounds.min.y + spawnPadding, bounds.max.y - spawnPadding);

        randomPoint = new Vector2(x, y);
        attempts++;

    } while (!tongueBounds.OverlapPoint(randomPoint) && attempts < 100);

    return randomPoint;
}

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TrySelectItem(worldPosition);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            TrySelectItem(worldPosition);
        }
    }

    private void TrySelectItem(Vector2 worldPosition)
    {
        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null) return;

        BrushingTongueItem item = hit.GetComponent<BrushingTongueItem>();

        if (item != null)
        {
            item.Select();
        }
    }

    public void RemoveDirt(BrushingTongueItem item)
    {
        remainingDirt--;

        spawnedItems.Remove(item.gameObject);
        Destroy(item.gameObject);

        if (remainingDirt <= 0)
        {
            WinTongueGame();
        }
    }

    public void ClickedHygieneItem(BrushingTongueItem item)
    {
        Debug.Log("Ese no es suciedad.");
    }

    private void WinTongueGame()
    {
        gameActive = false;

        if (messageText != null)
            messageText.text = "¡Lengua limpia!";

        Debug.Log("Victoria en limpieza de lengua");
    }

    private void ClearItems()
    {
        foreach (GameObject item in spawnedItems)
        {
            if (item != null)
                Destroy(item);
        }

        spawnedItems.Clear();
    }
}
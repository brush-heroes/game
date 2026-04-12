using UnityEngine;

public class PlaqueSpawner : MonoBehaviour
{
    public GameObject plaquePrefab;
    public Transform[] teeth;

    void Start()
    {
        foreach (Transform tooth in teeth)
        {
            GameObject plaque = Instantiate(plaquePrefab, tooth);
            plaque.transform.localPosition = Vector3.zero;
        }
    }
}
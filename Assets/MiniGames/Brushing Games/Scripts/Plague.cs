using UnityEngine;

public class Plaque : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Brush"))
        {
            Destroy(gameObject);
        }
    }
}
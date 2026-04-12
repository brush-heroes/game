using UnityEngine;

public class BrushCleaner : MonoBehaviour
{
    public DirtHealth currentDirt;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Dirt"))
        {
            currentDirt = collision.GetComponent<DirtHealth>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Dirt"))
        {
            if (collision.GetComponent<DirtHealth>() == currentDirt)
            {
                currentDirt = null;
            }
        }
    }
}
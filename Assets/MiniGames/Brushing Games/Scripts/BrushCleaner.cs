using UnityEngine;

public class BrushCleaner : MonoBehaviour
{
    public DirtHealth currentDirt;

    public float damageCooldown = 0.3f;
    private float lastDamageTime;

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

    private void Update()
    {
        if (currentDirt != null)
        {
            if (Time.time - lastDamageTime >= damageCooldown)
            {
                currentDirt.TakeDamage(1);
                lastDamageTime = Time.time;
            }
        }
    }
}
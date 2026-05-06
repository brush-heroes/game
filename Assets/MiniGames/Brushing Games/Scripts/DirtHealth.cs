using UnityEngine;

public class DirtHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public bool IsDead()
{
    return currentHealth <= 0;
}

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        Debug.Log(gameObject.name + " vida: " + currentHealth);

        if (currentHealth <= 0)
        {
            if (BrushingScoreManager.Instance != null)
                BrushingScoreManager.Instance.AddPointsForMouthDirt();

            Destroy(gameObject);
        }
    }
}
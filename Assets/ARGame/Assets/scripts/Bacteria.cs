using UnityEngine;

public class Bacteria : MonoBehaviour
{
    public BrushZone Zone { get; private set; }

    [SerializeField] private float maxHealth = 1.0f;
    [SerializeField] private float cleanSpeed = 2.0f;

    private float currentHealth;
    private Vector3 initialScale;
    bool deathScored;

    public void Initialize(BrushZone zone)
    {
        Zone = zone;
        Debug.Log("[Bacteria] Initialized with zone: " + zone);
    }

    void Start()
    {
        initialScale = transform.localScale;
        currentHealth = maxHealth;
    }

    public void Clean(float deltaTime)
    {
        currentHealth -= cleanSpeed * deltaTime;

        float normalized = Mathf.Clamp01(currentHealth / maxHealth);
        transform.localScale = initialScale * normalized;

        if (currentHealth <= 0)
        {
            if (!deathScored)
            {
                deathScored = true;
                ScoreManager.instance?.RegisterClean();
                ScoreManager.instance?.AddScore(1);
            }

            Debug.Log("[Bacteria] Destroyed in zone: " + Zone);

            Destroy(gameObject);
        }
    }
}

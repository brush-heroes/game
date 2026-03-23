using UnityEngine;

public class Bacteria : MonoBehaviour
{
    public BrushZone Zone { get; private set; }

    [SerializeField] private float maxHealth = 1.0f;
    [SerializeField] private float cleanSpeed = 2.0f;

    [Header("Highlight (current brushing zone)")]
    [SerializeField] Color highlightTint = Color.white;
    [SerializeField] Color nonTargetTint = new Color(0.5f, 0.52f, 0.58f, 1f);

    private float currentHealth;
    private Vector3 initialScale;
    private BacteriaSpawner ownerSpawner;
    bool deathScored;
    bool countedRemovalFromSpawner;

    SpriteRenderer[] spriteRenderers;
    Color[] spriteBaseColors;
    MeshRenderer[] meshRenderers;
    Color[] meshBaseColors;
    bool visualsCached;

    public void Initialize(BacteriaSpawner spawner, BrushZone zone)
    {
        ownerSpawner = spawner;
        Zone = zone;
        visualsCached = false;
    }

    public void ApplyTargetHighlight(bool isCurrentTargetZone)
    {
        EnsureVisualsCached();
        Color tint = isCurrentTargetZone ? highlightTint : nonTargetTint;

        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null)
                    continue;
                Color b = spriteBaseColors[i];
                spriteRenderers[i].color = new Color(b.r * tint.r, b.g * tint.g, b.b * tint.b, b.a * tint.a);
            }
            return;
        }

        if (meshRenderers == null || meshRenderers.Length == 0)
            return;

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] == null)
                continue;
            Color b = meshBaseColors[i];
            Color o = new Color(b.r * tint.r, b.g * tint.g, b.b * tint.b, b.a * tint.a);
            Material m = meshRenderers[i].material;
            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", o);
            else
                m.color = o;
        }
    }

    void EnsureVisualsCached()
    {
        if (visualsCached)
            return;

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (spriteRenderers.Length > 0)
        {
            spriteBaseColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
                spriteBaseColors[i] = spriteRenderers[i].color;
            visualsCached = true;
            return;
        }

        meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        meshBaseColors = new Color[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
            meshBaseColors[i] = meshRenderers[i].material.color;
        visualsCached = true;
    }

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void NotifyRemovalFromSpawner()
    {
        if (countedRemovalFromSpawner)
            return;
        countedRemovalFromSpawner = true;
        if (ownerSpawner != null && !ownerSpawner.IsSessionClearInProgress)
            ownerSpawner.NotifyBacteriaDestroyed(Zone);
    }

    void OnDestroy()
    {
        NotifyRemovalFromSpawner();
    }

    void Start()
    {
        initialScale = transform.localScale;
    }

    public void Clean(float deltaTime, BrushZone currentTargetZone, bool penalizeWrongZoneClean)
    {
        if (initialScale == Vector3.zero)
            initialScale = transform.localScale;

        currentHealth -= cleanSpeed * deltaTime;

        float normalized = Mathf.Clamp01(currentHealth / maxHealth);
        transform.localScale = initialScale * normalized;

        if (currentHealth <= 0)
        {
            if (!deathScored)
            {
                deathScored = true;
                ScoreManager.instance?.RegisterCleanResult(Zone, currentTargetZone, penalizeWrongZoneClean);
            }

            NotifyRemovalFromSpawner();
            Destroy(gameObject);
        }
    }
}

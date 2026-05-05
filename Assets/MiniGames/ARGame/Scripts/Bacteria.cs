using UnityEngine;

public class Bacteria : MonoBehaviour
{
    public MouthZone Zone { get; private set; }

    [SerializeField] float maxHealth = 1.0f;
    [SerializeField] float cleanSpeed = 2.0f;

    [Header("Highlight (current brushing zone)")]
    [SerializeField] Color highlightTint = Color.white;
    [SerializeField] Color nonTargetTint = new Color(0.22f, 0.23f, 0.28f, 0.80f);

    float currentHealth;
    BacteriaSpawner ownerSpawner;
    bool deathScored;
    bool countedRemovalFromSpawner;
    bool _dying;

    // Brushed-state tracking (set each Update frame by Clean(), reset in LateUpdate)
    bool _beingBrushed;
    bool _prevBeingBrushed;

    BacteriaAnimator _animator;
    SpriteRenderer[] spriteRenderers;
    Color[] spriteBaseColors;
    MeshRenderer[] meshRenderers;
    Color[] meshBaseColors;
    bool visualsCached;

    public bool IsDying => _dying;

    public void Initialize(BacteriaSpawner spawner, MouthZone zone)
    {
        ownerSpawner = spawner;
        Zone = zone;
        visualsCached = false;
    }

    void Awake()
    {
        currentHealth = maxHealth;
        _animator = GetComponentInChildren<BacteriaAnimator>(true);
    }

    void LateUpdate()
    {
        if (_dying)
        {
            _prevBeingBrushed = _beingBrushed;
            _beingBrushed = false;
            return;
        }

        if (_beingBrushed && !_prevBeingBrushed)
            _animator?.TriggerBrush();
        else if (!_beingBrushed && _prevBeingBrushed)
            _animator?.StopBrush();

        _prevBeingBrushed = _beingBrushed;
        _beingBrushed = false; // reset for next frame — Clean() sets it if brush is touching
    }

    public void Clean(float deltaTime, MouthZone currentTargetZone, bool penalizeWrongZoneClean)
    {
        if (_dying) return;

        _beingBrushed = true;
        currentHealth -= cleanSpeed * deltaTime;

        if (currentHealth <= 0 && !deathScored)
        {
            deathScored = true;
            _dying = true;
            ScoreManager.instance?.RegisterCleanResult(Zone, currentTargetZone, penalizeWrongZoneClean);
            NotifyRemovalFromSpawner();
            TriggerDeathAnim();
        }
    }

    // Called by GuidedModeController to kill the bacteria without collision or scoring.
    public void ForceKill()
    {
        if (_dying) return;
        _dying = true;
        deathScored = true;
        NotifyRemovalFromSpawner();
        TriggerDeathAnim();
    }

    void TriggerDeathAnim()
    {
        ARGameAudioManager.Instance?.PlayBacteriaDeath();
        if (_animator != null)
            _animator.TriggerDeath(() => Destroy(gameObject));
        else
            Destroy(gameObject);
    }

    void NotifyRemovalFromSpawner()
    {
        if (countedRemovalFromSpawner) return;
        countedRemovalFromSpawner = true;
        if (ownerSpawner != null && !ownerSpawner.IsSessionClearInProgress)
            ownerSpawner.NotifyBacteriaDestroyed(Zone);
    }

    void OnDestroy()
    {
        NotifyRemovalFromSpawner();
    }

    // ── Target highlight ─────────────────────────────────────────────────────

    public void ApplyTargetHighlight(bool isCurrentTargetZone)
    {
        EnsureVisualsCached();
        Color tint = isCurrentTargetZone ? highlightTint : nonTargetTint;

        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] == null) continue;
                Color b = spriteBaseColors[i];
                spriteRenderers[i].color = new Color(b.r * tint.r, b.g * tint.g, b.b * tint.b, b.a * tint.a);
            }
            return;
        }

        if (meshRenderers == null || meshRenderers.Length == 0) return;
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] == null) continue;
            Color b = meshBaseColors[i];
            Color o = new Color(b.r * tint.r, b.g * tint.g, b.b * tint.b, b.a * tint.a);
            Material m = meshRenderers[i].material;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", o);
            else m.color = o;
        }
    }

    void EnsureVisualsCached()
    {
        if (visualsCached) return;

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
}

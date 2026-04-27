using UnityEngine;

public class BrushDirt : MonoBehaviour
{
    private DirtHealth dirtHealth;

    private float damageCooldown = 0.3f;
    private float lastDamageTime;

    private Vector3 lastBrushPosition;

    private void Start()
    {
        dirtHealth = GetComponent<DirtHealth>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        BrushZoneDetector brush = other.GetComponent<BrushZoneDetector>();

        if (brush == null || !brush.IsCorrectZone())
            return;

        // Cooldown
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        // Movimiento del cepillo
        Vector3 currentPos = other.transform.position;
        Vector3 movement = currentPos - lastBrushPosition;

        lastBrushPosition = currentPos;

        // Validar que se esté moviendo
        if (movement.magnitude < 0.01f)
            return;

        // Validar que el movimiento sea vertical
        if (Mathf.Abs(movement.y) <= Mathf.Abs(movement.x))
            return;

        // Aplicar daño
        if (dirtHealth != null)
        {
            dirtHealth.TakeDamage(1);

            if (dirtHealth.IsDead())
            {
                BrushGameManager.Instance.AddClean();
            }

            lastDamageTime = Time.time;
        }
    }
}
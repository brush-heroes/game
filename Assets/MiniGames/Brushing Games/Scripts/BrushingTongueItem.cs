using UnityEngine;

public class BrushingTongueItem : MonoBehaviour
{
    [Header("Tipo de item")]
    public bool isDirt = true;

    [Header("Movimiento")]
    public float speed = 2f;

    private Vector2 direction;
    private Collider2D tongueBounds;
    private Collider2D itemCollider;
    private TongueGameManager manager;

    public void Init(Collider2D bounds, TongueGameManager gameManager)
    {
        tongueBounds = bounds;
        manager = gameManager;
        itemCollider = GetComponent<Collider2D>();

        direction = Random.insideUnitCircle.normalized;

        if (direction == Vector2.zero)
            direction = Vector2.right;
    }

    private void Update()
    {
        if (tongueBounds == null) return;

        transform.Translate(direction * speed * Time.deltaTime);

        CheckBounds();
    }

    private void CheckBounds()
    {
        if (!tongueBounds.OverlapPoint(transform.position))
        {
            direction *= -1;
            transform.Translate(direction * speed * Time.deltaTime * 2f);
        }
    }
    public void Select()
    {
        if (manager == null) return;

        if (isDirt)
        {
            manager.RemoveDirt(this);
        }
        else
        {
            manager.ClickedHygieneItem(this);
        }
    }
}
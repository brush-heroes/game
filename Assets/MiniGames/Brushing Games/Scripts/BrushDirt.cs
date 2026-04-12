using UnityEngine;

public class BrushDirt : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        BrushZoneDetector brush = other.GetComponent<BrushZoneDetector>();

        if (brush != null)
        {
            if (brush.IsCorrectZone())
            {
                BrushGameManager.Instance.AddClean();
                Destroy(gameObject);
            }
        }
    }
}
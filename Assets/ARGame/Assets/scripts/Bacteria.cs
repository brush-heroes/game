using UnityEngine;

public class Bacteria : MonoBehaviour
{
    const float MotionCleanDistance = 2f;

    void Update()
    {
        if (Camera.main == null)
            return;

        if (MotionDetector.instance != null && MotionDetector.instance.isMotionDetected)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            if (distance < MotionCleanDistance)
            {
                if (ScoreManager.instance != null)
                    ScoreManager.instance.AddScore(1);
                Destroy(gameObject);
                return;
            }
        }

        Vector2 screenPos = GetInputScreenPosition();
        if (screenPos.x < 0)
            return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
        {
            if (ScoreManager.instance != null)
                ScoreManager.instance.AddScore(1);
            Destroy(gameObject);
        }
    }

    Vector2 GetInputScreenPosition()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            return Input.mousePosition;
#endif
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            return Input.GetTouch(0).position;

        return new Vector2(-1f, -1f);
    }
}

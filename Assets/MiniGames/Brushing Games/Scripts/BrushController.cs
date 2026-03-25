using UnityEngine;
using UnityEngine.InputSystem;


public class BrushController : MonoBehaviour
{

    public Vector3 normalScale = new Vector3(0.9f,0.9f,0.9f);
    public Vector3 zoomScale = new Vector3(0.01f,0.01f,0.01f);

    public void SetZoomMode(bool isZoom)
    {
        transform.localScale = isZoom ? zoomScale : normalScale;
    }

    void Update()
    {
        Vector2 mouse = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mouse);

        float distance = -Camera.main.transform.position.z;

        Vector3 worldPos = ray.origin + ray.direction * distance;

        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

public class BrushController : MonoBehaviour
{
    void Update()
    {
        Vector2 mouse = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mouse);

        float distance = -Camera.main.transform.position.z;

        Vector3 worldPos = ray.origin + ray.direction * distance;

        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
    }
}
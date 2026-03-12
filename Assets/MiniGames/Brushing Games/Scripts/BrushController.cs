using UnityEngine;
using UnityEngine.InputSystem;

public class BrushController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        float distance = -Camera.main.transform.position.z;

        Vector3 worldPos = ray.origin + ray.direction * distance;

        transform.position = worldPos;
    }
}

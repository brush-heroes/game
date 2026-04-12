using UnityEngine;

[AddComponentMenu("ARGame/Billboard")]
public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null)
            return;

        transform.LookAt(Camera.main.transform);
    }
}

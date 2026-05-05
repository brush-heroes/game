using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraZoom : MonoBehaviour
{
    public Camera cam;
    public float finalSize = 2f;
    public float duration = 2.5f; // Control total del tiempo del zoom
    public float startDelay = 0.8f; // Tiempo para que el niño vea la imagen antes de moverse

    void Start()
    {
        // Iniciamos la secuencia automática
        StartCoroutine(ZoomSequence());
    }

    IEnumerator ZoomSequence()
    {
        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        // 1. Espera inicial (Anticipación)
        yield return new WaitForSeconds(startDelay);

        // 2. Zoom suave (Interpolación)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Usamos SmoothStep para que el inicio y el fin sean suaves (curva S)
            t = t * t * (3f - 2f * t);

            cam.orthographicSize = Mathf.Lerp(startSize, finalSize, t);
            yield return null;
        }

        // Aseguramos que llegue al tamaño final
        cam.orthographicSize = finalSize;

        // Nota: No llamamos a SceneManager aquí porque tu script 
        // 'SceneTransitioner' se encargará de cambiar la escena con el fade blanco.
    }
}
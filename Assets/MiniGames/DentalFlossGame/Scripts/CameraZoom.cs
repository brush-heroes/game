using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraZoom : MonoBehaviour
{
    public Camera cam;
    public float finalSize = 2f; 
    public float zoomSpeed = 2f;
    public string gameplayScene = "DentalFlossGame";

    void Update()
    {
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, finalSize, Time.deltaTime * zoomSpeed);

        if (Mathf.Abs(cam.orthographicSize - finalSize) < 0.05f)
        {
            SceneManager.LoadScene(gameplayScene);
        }
    }
}
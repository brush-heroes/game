using UnityEngine;
using UnityEngine.UI;

public class SelfieCamera : MonoBehaviour
{
    public RawImage cameraDisplay;

    WebCamTexture webCam;

    void Start()
    {
        if (cameraDisplay == null)
            return;

        string frontCameraName = null;
        foreach (WebCamDevice device in WebCamTexture.devices)
        {
#if UNITY_EDITOR
            frontCameraName = device.name;
            break;
#else
            if (device.isFrontFacing)
            {
                frontCameraName = device.name;
                break;
            }
#endif
        }

        if (string.IsNullOrEmpty(frontCameraName) && WebCamTexture.devices.Length > 0)
            frontCameraName = WebCamTexture.devices[0].name;

        if (string.IsNullOrEmpty(frontCameraName))
            return;

        webCam = new WebCamTexture(frontCameraName);
        cameraDisplay.texture = webCam;
        webCam.Play();
    }

    void OnDestroy()
    {
        if (webCam != null && webCam.isPlaying)
            webCam.Stop();
    }
}

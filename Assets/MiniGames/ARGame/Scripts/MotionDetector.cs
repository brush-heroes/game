using UnityEngine;

public class MotionDetector : MonoBehaviour
{
    public static MotionDetector instance;

    public float motionThreshold = 0.05f;
    public bool isMotionDetected { get; private set; }

    WebCamTexture webCam;
    Texture2D[] frameBuffers = new Texture2D[2];
    RenderTexture captureRT;
    int readIndex;
    bool hasPreviousFrame;
    int sampleStep = 4;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
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

        if (string.IsNullOrEmpty(frontCameraName))
            frontCameraName = WebCamTexture.devices[0].name;

        webCam = new WebCamTexture(frontCameraName);
        webCam.Play();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
        if (webCam != null && webCam.isPlaying)
            webCam.Stop();
        if (captureRT != null)
            captureRT.Release();
        if (frameBuffers[0] != null) Destroy(frameBuffers[0]);
        if (frameBuffers[1] != null) Destroy(frameBuffers[1]);
    }

    void Update()
    {
        if (webCam == null || !webCam.didUpdateThisFrame || webCam.width < 2 || webCam.height < 2)
            return;

        EnsureTextures();

        Texture2D currentFrame = frameBuffers[readIndex];
        Texture2D previousFrame = frameBuffers[1 - readIndex];

        Graphics.Blit(webCam, captureRT);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = captureRT;
        currentFrame.ReadPixels(new Rect(0, 0, captureRT.width, captureRT.height), 0, 0);
        currentFrame.Apply();
        RenderTexture.active = prev;

        if (hasPreviousFrame)
        {
            float diff = ComputePixelDifference(previousFrame, currentFrame);
            isMotionDetected = diff >= motionThreshold;
        }
        else
        {
            isMotionDetected = false;
        }

        readIndex = 1 - readIndex;
        hasPreviousFrame = true;
    }

    void EnsureTextures()
    {
        int w = webCam.width;
        int h = webCam.height;
        if (captureRT != null && captureRT.width == w && captureRT.height == h)
            return;

        if (captureRT != null)
            captureRT.Release();
        captureRT = new RenderTexture(w, h, 0);

        if (frameBuffers[0] != null) Destroy(frameBuffers[0]);
        if (frameBuffers[1] != null) Destroy(frameBuffers[1]);
        frameBuffers[0] = new Texture2D(w, h);
        frameBuffers[1] = new Texture2D(w, h);
        readIndex = 0;
        hasPreviousFrame = false;
    }

    float ComputePixelDifference(Texture2D a, Texture2D b)
    {
        int w = a.width;
        int h = a.height;
        if (b.width != w || b.height != h)
            return 0f;

        Color[] pixelsA = a.GetPixels();
        Color[] pixelsB = b.GetPixels();
        long sum = 0;
        int count = 0;

        for (int i = 0; i < pixelsA.Length; i += sampleStep)
        {
            Color ca = pixelsA[i];
            Color cb = pixelsB[i];
            sum += Mathf.Abs((int)(ca.r * 255) - (int)(cb.r * 255));
            sum += Mathf.Abs((int)(ca.g * 255) - (int)(cb.g * 255));
            sum += Mathf.Abs((int)(ca.b * 255) - (int)(cb.b * 255));
            count += 3;
        }

        return count > 0 ? (sum / (float)(count * 255)) : 0f;
    }
}

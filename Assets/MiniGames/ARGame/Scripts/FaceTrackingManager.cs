using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Wraps ARFaceManager to expose simple face-detection state and events.
/// Attach this to the same GameObject that has ARFaceManager.
/// </summary>
[RequireComponent(typeof(ARFaceManager))]
public class FaceTrackingManager : MonoBehaviour
{
    ARFaceManager _faceManager;

    public bool IsFaceTracked { get; private set; }

    /// <summary>Fired when at least one face transitions from untracked → tracked.</summary>
    public event Action OnFaceDetected;

    /// <summary>Fired when all tracked faces are lost.</summary>
    public event Action OnFaceLost;

    void Awake()
    {
        _faceManager = GetComponent<ARFaceManager>();
    }

    void OnEnable()
    {
        _faceManager.facesChanged += HandleFacesChanged;
    }

    void OnDisable()
    {
        _faceManager.facesChanged -= HandleFacesChanged;
    }

    void HandleFacesChanged(ARFacesChangedEventArgs args)
    {
        // AR Foundation 5.x re-detects faces automatically when they return to view.
        // We just need to track the current state correctly.
        bool hasTracked = false;
        foreach (ARFace face in _faceManager.trackables)
        {
            if (face.trackingState == TrackingState.Tracking)
            {
                hasTracked = true;
                break;
            }
        }

        bool prev = IsFaceTracked;
        IsFaceTracked = hasTracked;

        if (!prev && hasTracked)
        {
            Debug.Log("[FaceTrackingManager] Face detected.");
            OnFaceDetected?.Invoke();
        }
        else if (prev && !hasTracked)
        {
            Debug.Log("[FaceTrackingManager] Face lost — will re-detect automatically when face returns.");
            OnFaceLost?.Invoke();
        }
    }
}

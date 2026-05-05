using System;
using UnityEngine;

[Serializable]
public struct ZoneSoundEntry
{
    public MouthZone zone;
    public AudioClip clip;
}

// Central audio controller for the AR brushing game.
// Place on any persistent GO in the AR scene.
public class ARGameAudioManager : MonoBehaviour
{
    public static ARGameAudioManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] SessionManager sessionManager;
    [SerializeField] VirtualBrushController brushController;

    [Header("Zone Announcement Sounds")]
    [Tooltip("One entry per zone — plays when the zone transition overlay appears")]
    [SerializeField] ZoneSoundEntry[] zoneSounds;

    [Header("Bacteria Sounds")]
    [SerializeField] AudioClip bacteriaDeathClip;
    [SerializeField] AudioClip bacteriaBornClip;
    [Tooltip("Minimum seconds between consecutive bacteria-sound plays")]
    [SerializeField] float bacteriaSoundCooldown = 0.5f;

    [Header("Brushing Loop")]
    [SerializeField] AudioClip brushingClip;
    [SerializeField] float brushingVolume = 0.7f;

    AudioSource _sfxSource;
    AudioSource _brushSource;
    float _deathTimer;
    float _bornTimer;

    void Awake()
    {
        Instance = this;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.spatialBlend = 0f;
        _sfxSource.playOnAwake = false;

        _brushSource = gameObject.AddComponent<AudioSource>();
        _brushSource.spatialBlend = 0f;
        _brushSource.loop = true;
        _brushSource.playOnAwake = false;
        _brushSource.volume = brushingVolume;
        if (brushingClip != null) _brushSource.clip = brushingClip;
    }

    void OnEnable()
    {
        if (sessionManager != null)
            sessionManager.OnZoneComplete += HandleZoneComplete;
    }

    void OnDisable()
    {
        if (sessionManager != null)
            sessionManager.OnZoneComplete -= HandleZoneComplete;
        _brushSource?.Stop();
    }

    void Update()
    {
        if (_deathTimer > 0) _deathTimer -= Time.deltaTime;
        if (_bornTimer  > 0) _bornTimer  -= Time.deltaTime;

        // Brushing loop: play whenever brush animation is active
        bool sessionOn  = sessionManager != null && sessionManager.IsSessionRunning;
        bool transition = sessionManager != null && sessionManager.IsInTransition;
        bool isBrushing = sessionOn && !transition && brushController != null
            && (brushController.IsMoving || brushController.GuidedMode);

        if (isBrushing && !_brushSource.isPlaying)
            _brushSource.Play();
        else if (!isBrushing && _brushSource.isPlaying)
            _brushSource.Stop();
    }

    // ── Zone sound ───────────────────────────────────────────────────────────

    void HandleZoneComplete(SessionStep nextStep)
    {
        if (zoneSounds == null) return;
        foreach (var entry in zoneSounds)
        {
            if (entry.zone == nextStep.zone && entry.clip != null)
            {
                _sfxSource.PlayOneShot(entry.clip);
                return;
            }
        }
    }

    // ── Bacteria sounds (called by Bacteria.cs and BacteriaSpawner.cs) ───────

    public void PlayBacteriaDeath()
    {
        if (_deathTimer > 0 || bacteriaDeathClip == null) return;
        _deathTimer = bacteriaSoundCooldown;
        _sfxSource.PlayOneShot(bacteriaDeathClip);
    }

    public void PlayBacteriaBorn()
    {
        if (_bornTimer > 0 || bacteriaBornClip == null) return;
        _bornTimer = bacteriaSoundCooldown;
        _sfxSource.PlayOneShot(bacteriaBornClip);
    }
}

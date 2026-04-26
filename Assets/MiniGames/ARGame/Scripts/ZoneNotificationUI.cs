using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Full-screen overlay shown between zones (subscribes to SessionManager.OnZoneComplete).
// Displays the NEXT zone's indicator image and name for the transition duration.
public class ZoneNotificationUI : MonoBehaviour
{
    [SerializeField] SessionManager sessionManager;

    [Header("UI Elements")]
    [SerializeField] CanvasGroup overlay;
    [SerializeField] Image  zoneImage;
    [SerializeField] TMP_Text zoneText;

    [Header("Zone Sprites (same data as BrushingGuideUI)")]
    [SerializeField] ZoneGuideData[] zoneGuideData;

    [SerializeField] float fadeDuration = 0.18f;

    void OnEnable()
    {
        if (sessionManager == null) return;
        sessionManager.OnZoneComplete += HandleZoneComplete;
        sessionManager.OnStepChanged  += HandleStepChanged;
    }

    void OnDisable()
    {
        if (sessionManager == null) return;
        sessionManager.OnZoneComplete -= HandleZoneComplete;
        sessionManager.OnStepChanged  -= HandleStepChanged;
    }

    void Start() => SetVisible(false);

    void HandleZoneComplete(SessionStep nextStep)
    {
        ZoneGuideData data = FindZoneData(nextStep.zone);
        if (zoneImage != null) zoneImage.sprite = data.zoneIndicator;
        if (zoneText  != null) zoneText.text = "Siguiente zona:\n<b>" + GetZoneName(nextStep.zone) + "</b>";
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    void HandleStepChanged(SessionStep step, int stepIndex)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    void SetVisible(bool v)
    {
        if (overlay == null) return;
        overlay.alpha = v ? 1f : 0f;
        overlay.interactable = v;
        overlay.blocksRaycasts = v;
    }

    IEnumerator FadeIn()
    {
        if (overlay == null) yield break;
        overlay.interactable  = true;
        overlay.blocksRaycasts = true;
        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        { overlay.alpha = t / fadeDuration; yield return null; }
        overlay.alpha = 1f;
    }

    IEnumerator FadeOut()
    {
        if (overlay == null) yield break;
        for (float t = fadeDuration; t > 0f; t -= Time.deltaTime)
        { overlay.alpha = t / fadeDuration; yield return null; }
        overlay.alpha = 0f;
        overlay.interactable  = false;
        overlay.blocksRaycasts = false;
    }

    ZoneGuideData FindZoneData(MouthZone zone)
    {
        if (zoneGuideData == null) return default;
        foreach (var d in zoneGuideData)
            if (d.zone == zone) return d;
        return default;
    }

    static string GetZoneName(MouthZone zone) => zone switch
    {
        MouthZone.UpperLeft  => "Superior Izquierdo",
        MouthZone.UpperRight => "Superior Derecho",
        MouthZone.FrontUpper => "Frontal Superior",
        MouthZone.LowerLeft  => "Inferior Izquierdo",
        MouthZone.LowerRight => "Inferior Derecho",
        MouthZone.FrontLower => "Frontal Inferior",
        _ => zone.ToString()
    };
}

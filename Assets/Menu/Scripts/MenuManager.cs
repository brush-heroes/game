using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class MenuManager : MonoBehaviour
{
    const string SCENE_AR    = "ARGame";
    const string SCENE_BRUSH = "BrushingGame";
    const string SCENE_FLOSS = "DentalFlossGame";

    [Header("Stats UI")]
    [SerializeField] TextMeshProUGUI streakText;
    [SerializeField] TextMeshProUGUI starsText;

    [Header("Missions")]
    [SerializeField] TextMeshProUGUI morningLabel;
    [SerializeField] TextMeshProUGUI eveningLabel;

    [Header("Hero message")]
    [SerializeField] TextMeshProUGUI heroMessage;

    [Header("Mascot")]
    [SerializeField] MascotController mascot;

    [Header("Calendar")]
    [SerializeField] CalendarView calendar;

    [Header("Video Background")]
    [SerializeField] RawImage   videoDisplay;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] string      videoPath = "Assets/Menu/Assets/videos/background.mp4";

    RenderTexture _rt;

    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        if (PlayerDataManager.Instance == null)
            new GameObject("PlayerDataManager").AddComponent<PlayerDataManager>();

        SetupVideo();
        RefreshUI();
        StartCoroutine(RebuildLayoutNextFrame());
    }

    // CanvasScaler applies its 1080×1920 reference via Canvas.preWillRenderCanvases,
    // which fires during the first render — after Start() but before Update().
    // Yielding one frame guarantees RectTransform widths are real before we ask
    // every LayoutGroup in the hierarchy (HeroPanel, StatsColumn, Content VLG) to recompute.
    IEnumerator RebuildLayoutNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        var rt = GetComponent<RectTransform>();
        if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    void SetupVideo()
    {
        if (videoPlayer == null || videoDisplay == null) return;

        _rt = new RenderTexture(1080, 1920, 0);
        videoDisplay.texture = _rt;

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = _rt;
        videoPlayer.isLooping = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.Play();
    }

    public void RefreshUI()
    {
        var data = PlayerDataManager.Instance;
        if (data == null) return;

        if (streakText  != null) streakText.text  = data.Streak.ToString();
        if (starsText   != null) starsText.text   = data.TotalStars.ToString();
        if (morningLabel != null)
            morningLabel.text = data.MorningDoneToday ? "Mañana  ✓" : "Mañana";
        if (eveningLabel != null)
            eveningLabel.text = data.EveningDoneToday ? "Tarde  ✓" : "Tarde";
        if (heroMessage != null)
            heroMessage.text = GetHeroMessage(data.Streak);

        if (mascot   != null) mascot.RefreshState(data.Streak);
        if (calendar != null) calendar.Refresh();
    }

    static string GetHeroMessage(int streak)
    {
        if (streak >= 7)  return "¡Imparable! " + streak + " días seguidos";
        if (streak >= 3)  return "¡Buen trabajo! Sigue así";
        if (streak == 1)  return "¡Buen comienzo! A mantenerlo";
        if (streak == 0)  return "¡Hoy es un gran día para empezar!";
        return "¡Sigue cepillando cada día!";
    }

    public void LoadAR()
    {
        SceneManager.LoadScene(SCENE_AR);
    }

    public void LoadBrush()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        SceneManager.LoadScene(SCENE_BRUSH);
    }

    public void LoadFloss()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        SceneManager.LoadScene(SCENE_FLOSS);
    }

    void OnDestroy()
    {
        if (_rt != null) _rt.Release();
    }
}

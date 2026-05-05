using UnityEngine;
using UnityEngine.UI;

public class GameCardUI : MonoBehaviour
{
    [SerializeField] public string sceneName;
    [SerializeField] Image thumbnail;
    [SerializeField] Image cardBackground;
    [SerializeField] Image playBtnBg;

    void Start()
    {
        var btn = GetComponentInChildren<Button>();
        if (btn != null) btn.onClick.AddListener(OnPressed);
    }

    void OnPressed()
    {
        var mgr = FindObjectOfType<MenuManager>();
        if (mgr == null) return;
        switch (sceneName)
        {
            case "ARGame":          mgr.LoadAR();    break;
            case "BrushingGame":    mgr.LoadBrush(); break;
            case "DentalFlossGame": mgr.LoadFloss(); break;
        }
    }
}

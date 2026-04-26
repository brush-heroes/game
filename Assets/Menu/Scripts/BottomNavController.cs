using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomNavController : MonoBehaviour
{
    [SerializeField] GameObject minijuegosPage;
    [SerializeField] GameObject calendarioPage;

    [SerializeField] TextMeshProUGUI labelMinijuegos;
    [SerializeField] TextMeshProUGUI labelCalendario;

    // Thin active-indicator bars below each nav item
    [SerializeField] Image indicatorMinijuegos;
    [SerializeField] Image indicatorCalendario;

    void Start() => ShowTab(0);

    public void ShowMinijuegos() => ShowTab(0);
    public void ShowCalendario()  => ShowTab(1);

    void ShowTab(int idx)
    {
        if (minijuegosPage != null) minijuegosPage.SetActive(idx == 0);
        if (calendarioPage  != null) calendarioPage.SetActive(idx == 1);
        SetActive(labelMinijuegos, indicatorMinijuegos, idx == 0);
        SetActive(labelCalendario,  indicatorCalendario,  idx == 1);
    }

    static void SetActive(TextMeshProUGUI label, Image indicator, bool active)
    {
        if (label     != null) label.color     = active ? UIStyleKit.NavActive : UIStyleKit.NavInactive;
        if (indicator != null) indicator.color = active
            ? UIStyleKit.NavActive
            : new Color(0, 0, 0, 0);
    }
}

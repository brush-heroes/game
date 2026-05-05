using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalendarView : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI monthLabel;
    [SerializeField] public Transform gridParent;

    static readonly string[] HEADERS = { "L", "M", "X", "J", "V", "S", "D" };

    Sprite _circle;
    DateTime _viewMonth;

    void Awake()
    {
        _circle    = UIStyleKit.CircleSprite();
        _viewMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    }
    void Start()  => Refresh();

    public void Refresh()   => BuildMonth(_viewMonth);
    public void PrevMonth() { _viewMonth = _viewMonth.AddMonths(-1); BuildMonth(_viewMonth); }
    public void NextMonth() { _viewMonth = _viewMonth.AddMonths(+1); BuildMonth(_viewMonth); }

    void BuildMonth(DateTime month)
    {
        if (gridParent == null) return;
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        if (monthLabel != null)
        {
            string s = month.ToString("MMMM yyyy");
            monthLabel.text = char.ToUpper(s[0]) + s.Substring(1);
        }

        foreach (var h in HEADERS) AddHeader(h);

        int firstDow = (int)month.DayOfWeek;
        int blanks   = firstDow == 0 ? 6 : firstDow - 1;
        for (int i = 0; i < blanks; i++) AddBlank();

        int days = DateTime.DaysInMonth(month.Year, month.Month);
        for (int d = 1; d <= days; d++)
        {
            var  date   = new DateTime(month.Year, month.Month, d);
            bool today  = date.Date == DateTime.Today;
            bool future = date.Date >  DateTime.Today;
            AddDay(d.ToString(), today, future ? (Color?)null : GetDotColor(date));
        }
    }

    Color? GetDotColor(DateTime date)
    {
        var r = PlayerDataManager.Instance?.GetDay(date);
        if (r == null)       return UIStyleKit.CalDotMissed;
        if (r.score >= 50)   return UIStyleKit.CalDotGreat;
        if (r.score >  0)    return UIStyleKit.CalDotOk;
        return UIStyleKit.CalDotMissed;
    }

    // ── cell builders ──────────────────────────────────────────────────────────

    void AddHeader(string text)
    {
        var go  = Cell();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = 22f; tmp.fontStyle = FontStyles.Bold;
        tmp.color = UIStyleKit.TextSecLight;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    void AddBlank() => Cell();

    void AddDay(string day, bool today, Color? dot)
    {
        var cell = Cell();

        if (today)
        {
            var bg  = Sub(cell, "Bg", 0.15f, 0.20f, 0.85f, 0.92f);
            var img = bg.AddComponent<Image>();
            img.sprite = _circle; img.color = UIStyleKit.CalToday;
        }

        var numGO = Sub(cell, "Num", 0f, 0.18f, 1f, 0.92f);
        var tmp   = numGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = day;
        tmp.fontSize  = 26f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = today ? Color.white : UIStyleKit.TextOnLight;
        tmp.fontStyle = today ? FontStyles.Bold : FontStyles.Normal;

        if (dot.HasValue)
        {
            var dotGO = Sub(cell, "Dot", 0.38f, 0.02f, 0.62f, 0.16f);
            var di    = dotGO.AddComponent<Image>();
            di.sprite = _circle; di.color = dot.Value;
        }
    }

    // Create a cell child parented to gridParent
    GameObject Cell()
    {
        var go = new GameObject("Cell");
        go.transform.SetParent(gridParent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    // Create a sub-object with anchor-based RT (no AbsoluteOffset needed)
    static GameObject Sub(GameObject parent, string name,
        float minX, float minY, float maxX, float maxY)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(minX, minY);
        rt.anchorMax = new Vector2(maxX, maxY);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return go;
    }
}

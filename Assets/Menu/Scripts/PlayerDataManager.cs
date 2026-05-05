using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    const string KEY_STREAK      = "menu_streak";
    const string KEY_STARS       = "menu_stars";
    const string KEY_RECORDS     = "menu_daily_records";
    const string KEY_INITIALIZED = "menu_initialized";

    [Serializable]
    public class DayRecord
    {
        public int  score;
        public bool morningDone;
        public bool eveningDone;
    }

    [Serializable]
    class RecordsWrapper
    {
        public List<string>    keys   = new List<string>();
        public List<DayRecord> values = new List<DayRecord>();
    }

    readonly Dictionary<string, DayRecord> _records = new Dictionary<string, DayRecord>();

    public int Streak     { get; private set; }
    public int TotalStars { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public DayRecord GetDay(DateTime date) =>
        _records.TryGetValue(DateKey(date), out var r) ? r : null;

    public bool MorningDoneToday => GetDay(DateTime.Today)?.morningDone ?? false;
    public bool EveningDoneToday => GetDay(DateTime.Today)?.eveningDone ?? false;

    public void RecordSession(bool isMorning, int score)
    {
        string key = DateKey(DateTime.Today);
        if (!_records.TryGetValue(key, out var r)) r = new DayRecord();
        if (isMorning) r.morningDone = true;
        else           r.eveningDone = true;
        r.score = Mathf.Max(r.score, score);
        _records[key] = r;
        TotalStars += score;
        RebuildStreak();
        Save();
    }

    void RebuildStreak()
    {
        int streak = 0;
        for (int i = 0; i < 365; i++)
        {
            var d = DateTime.Today.AddDays(-i);
            var r = GetDay(d);
            if (r != null && (r.morningDone || r.eveningDone))
                streak++;
            else
                break;
        }
        Streak = streak;
    }

    void Load()
    {
        Streak     = PlayerPrefs.GetInt(KEY_STREAK, 0);
        TotalStars = PlayerPrefs.GetInt(KEY_STARS, 0);

        string json = PlayerPrefs.GetString(KEY_RECORDS, "");
        if (!string.IsNullOrEmpty(json))
        {
            var w = JsonUtility.FromJson<RecordsWrapper>(json);
            if (w != null)
                for (int i = 0; i < w.keys.Count; i++)
                    _records[w.keys[i]] = w.values[i];
        }

        if (PlayerPrefs.GetInt(KEY_INITIALIZED, 0) == 0)
        {
            GenerateMockData();
            PlayerPrefs.SetInt(KEY_INITIALIZED, 1);
        }

        RebuildStreak();
        Save();
    }

    void Save()
    {
        PlayerPrefs.SetInt(KEY_STREAK, Streak);
        PlayerPrefs.SetInt(KEY_STARS, TotalStars);

        var w = new RecordsWrapper();
        foreach (var kvp in _records) { w.keys.Add(kvp.Key); w.values.Add(kvp.Value); }
        PlayerPrefs.SetString(KEY_RECORDS, JsonUtility.ToJson(w));
        PlayerPrefs.Save();
    }

    void GenerateMockData()
    {
        var rng = new System.Random(42);
        for (int i = 30; i >= 1; i--)
        {
            bool hasData = i <= 5 || rng.NextDouble() > 0.35;
            if (!hasData) continue;
            var d = DateTime.Today.AddDays(-i);
            _records[DateKey(d)] = new DayRecord
            {
                score       = rng.Next(30, 100),
                morningDone = rng.NextDouble() > 0.35,
                eveningDone = rng.NextDouble() > 0.45
            };
        }
        _records[DateKey(DateTime.Today)] = new DayRecord
        {
            score = 75, morningDone = true, eveningDone = false
        };
        RebuildStreak();
        TotalStars = 0;
        foreach (var r in _records.Values) TotalStars += r.score;
    }

    static string DateKey(DateTime d) => d.ToString("yyyy-MM-dd");
}

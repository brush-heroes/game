using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

// Central player-data store.
// Persists to JSON file at Application.persistentDataPath/playerData.json.
// All minigames report sessions via Instance.RecordSession(tipo, puntaje, duracion, completada).
// Aggregate progress (puntajeTotal, racha, mañana/tarde) is recomputed from sessions automatically.
public class PlayerDataManager : MonoBehaviour
{
    static PlayerDataManager _instance;
    public static PlayerDataManager Instance
    {
        get
        {
            if (_instance != null) return _instance;
            var found = FindObjectOfType<PlayerDataManager>();
            if (found != null) { _instance = found; return _instance; }
            var go = new GameObject("PlayerDataManager");
            _instance = go.AddComponent<PlayerDataManager>();
            return _instance;
        }
    }

    const string FILE_NAME = "playerData.json";
    const string KEY_MOCK_GENERATED = "menu_mock_generated_v2";

    PlayerData _data;
    public PlayerData Data => _data;

    public event Action OnDataChanged;

    public int Streak     => _data.progreso.racha;
    public int TotalStars => _data.progreso.puntajeTotal;

    static string FilePath => Path.Combine(Application.persistentDataPath, FILE_NAME);

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    // ── Persistence ──────────────────────────────────────────────────────────

    void Load()
    {
        bool freshInstall = !File.Exists(FilePath);

        if (!freshInstall)
        {
            try { _data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(FilePath)); }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerDataManager] Failed to load JSON: {e}. Starting fresh.");
                _data = null;
                freshInstall = true;
            }
        }

        if (_data == null) _data = new PlayerData();
        if (_data.jugador  == null) _data.jugador  = new PlayerInfo();
        if (_data.progreso == null) _data.progreso = new PlayerProgress();
        if (_data.sesiones == null) _data.sesiones = new List<PlayerSession>();
        if (_data.logros   == null) _data.logros   = new List<PlayerAchievement>();

        if (freshInstall || string.IsNullOrEmpty(_data.jugador.id))
            InitializeNewPlayer();

        if (PlayerPrefs.GetInt(KEY_MOCK_GENERATED, 0) == 0)
        {
            GenerateMockSessions();
            PlayerPrefs.SetInt(KEY_MOCK_GENERATED, 1);
            PlayerPrefs.Save();
        }

        RecomputeProgress();
        Save();
    }

    void InitializeNewPlayer()
    {
        _data.jugador.id          = Guid.NewGuid().ToString();
        _data.jugador.nombre      = "Jugador";
        _data.jugador.avatarId    = 0;
        _data.jugador.fechaInicio = DateTime.UtcNow.ToString("o");
    }

    void Save()
    {
        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(_data, true));
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Failed to save JSON: {e}");
        }
    }

    // ── Public API for minigames ─────────────────────────────────────────────

    /// <summary>
    /// Report a minigame session. Use a constant from <see cref="MinigameTypes"/>.
    /// Aggregate progress (total score, streak, mañana/tarde) is recomputed from all sessions.
    /// </summary>
    public void RecordSession(string tipo, int puntaje, int duracionSegundos, bool completada)
    {
        var session = new PlayerSession
        {
            id         = Guid.NewGuid().ToString(),
            tipo       = tipo,
            fecha      = DateTime.UtcNow.ToString("o"),
            duracion   = Mathf.Max(0, duracionSegundos),
            completada = completada,
            puntaje    = puntaje
        };
        _data.sesiones.Add(session);
        RecomputeProgress();
        Save();
        OnDataChanged?.Invoke();
        Debug.Log($"[PlayerDataManager] Session recorded: tipo={tipo} puntaje={puntaje} duracion={duracionSegundos}s completada={completada}");
    }

    // ── Aggregate calculations (derived from sessions) ───────────────────────

    void RecomputeProgress()
    {
        int total = 0;
        for (int i = 0; i < _data.sesiones.Count; i++)
            total += _data.sesiones[i].puntaje;

        _data.progreso.puntajeTotal = total;
        _data.progreso.racha        = ComputeStreak();
    }

    int ComputeStreak()
    {
        var datesWithSession = new HashSet<string>();
        for (int i = 0; i < _data.sesiones.Count; i++)
        {
            if (TryParseDate(_data.sesiones[i].fecha, out var dt))
                datesWithSession.Add(LocalDateKey(dt));
        }
        int streak = 0;
        for (int i = 0; i < 365; i++)
        {
            string key = DateKey(DateTime.Today.AddDays(-i));
            if (datesWithSession.Contains(key)) streak++;
            else break;
        }
        return streak;
    }

    // ── Compatibility helpers for the existing menu UI ───────────────────────
    // MenuManager + CalendarView already consume these; they remain unchanged.

    [Serializable]
    public class DayRecord
    {
        public int  score;
        public bool morningDone;
        public bool eveningDone;
    }

    public DayRecord GetDay(DateTime date)
    {
        DayRecord r = null;
        string targetKey = DateKey(date);
        for (int i = 0; i < _data.sesiones.Count; i++)
        {
            var s = _data.sesiones[i];
            if (!TryParseDate(s.fecha, out var dt)) continue;
            var local = dt.ToLocalTime();
            if (LocalDateKey(dt) != targetKey) continue;

            if (r == null) r = new DayRecord();
            r.score += s.puntaje;
            if (local.Hour < 12) r.morningDone = true;
            else                 r.eveningDone = true;
        }
        return r;
    }

    public bool MorningDoneToday => GetDay(DateTime.Today)?.morningDone ?? false;
    public bool EveningDoneToday => GetDay(DateTime.Today)?.eveningDone ?? false;

    // ── Mock data for first-run demo ─────────────────────────────────────────

    void GenerateMockSessions()
    {
        var rng = new System.Random(42);
        for (int i = 30; i >= 1; i--)
        {
            bool hasData = i <= 5 || rng.NextDouble() > 0.35;
            if (!hasData) continue;
            var date = DateTime.Today.AddDays(-i);
            if (rng.NextDouble() > 0.35) AddMockSession(date, true,  rng);
            if (rng.NextDouble() > 0.45) AddMockSession(date, false, rng);
        }
        AddMockSession(DateTime.Today, true, rng); // Today: morning done.
    }

    void AddMockSession(DateTime localDate, bool isMorning, System.Random rng)
    {
        int hour = isMorning ? 8 : 20;
        var localDt = new DateTime(localDate.Year, localDate.Month, localDate.Day, hour, 0, 0, DateTimeKind.Local);
        var utc     = localDt.ToUniversalTime();

        _data.sesiones.Add(new PlayerSession
        {
            id         = Guid.NewGuid().ToString(),
            tipo       = MinigameTypes.AR,
            fecha      = utc.ToString("o"),
            duracion   = rng.Next(60, 180),
            completada = rng.NextDouble() > 0.2,
            puntaje    = rng.Next(20, 60)
        });
    }

    // ── Date helpers ─────────────────────────────────────────────────────────

    static bool TryParseDate(string iso, out DateTime dt)
        => DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt);

    static string DateKey(DateTime localDate) => localDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    static string LocalDateKey(DateTime utcDate) => utcDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

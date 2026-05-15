using System;
using System.Collections.Generic;

// JSON schema definitions for local persistence.
// Mirrors the structure required by the data-persistence requirement (requerimientos6).

[Serializable]
public class PlayerData
{
    public int version = 1;
    public PlayerInfo jugador = new PlayerInfo();
    public PlayerProgress progreso = new PlayerProgress();
    public List<PlayerSession> sesiones = new List<PlayerSession>();
    public List<PlayerAchievement> logros = new List<PlayerAchievement>();
}

[Serializable]
public class PlayerInfo
{
    public string id;
    public string nombre;
    public int avatarId;
    public string fechaInicio;
}

[Serializable]
public class PlayerProgress
{
    public int puntajeTotal;
    public int nivelActual = 1;
    public int racha;
}

[Serializable]
public class PlayerSession
{
    public string id;
    public string tipo;
    public string fecha;
    public int duracion;
    public bool completada;
    public int puntaje;
}

[Serializable]
public class PlayerAchievement
{
    public string id;
    public bool desbloqueado;
    public string fechaDesbloqueo;
}

// Single source of truth for the "tipo" field.
// Adding a new minigame is a one-liner here — no other code needs to change.
public static class MinigameTypes
{
    public const string AR        = "RA";
    public const string Cepillado = "MinijuegoCepillado";
    public const string Seda      = "MinijuegoSeda";
}

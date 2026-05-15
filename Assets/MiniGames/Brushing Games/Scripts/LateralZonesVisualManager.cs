using System;
using UnityEngine;

public enum LateralVisualPhase
{
    OutsideRight,
    InsideRight,
    OutsideLeft,
    InsideLeft
}

[Serializable]
public class LateralZoneVisualEntry
{
    public SpriteRenderer teethRenderer;
    public Sprite dirtySprite;
    public Sprite cleanSprite;

    public void SetDirty()
    {
        if (teethRenderer != null && dirtySprite != null)
            teethRenderer.sprite = dirtySprite;
    }

    public void SetClean()
    {
        if (teethRenderer != null && cleanSprite != null)
            teethRenderer.sprite = cleanSprite;
    }
}

/// <summary>
/// Muestra sprite sucio al iniciar cada minijuego lateral y cambia a limpio
/// si se ganan al menos <see cref="pointsRequired"/> puntos durante esa fase.
/// </summary>
public class LateralZonesVisualManager : MonoBehaviour
{
    [SerializeField] private int pointsRequired = 80;

    [Header("Derecha")]
    [SerializeField] private LateralZoneVisualEntry outsideRight;
    [SerializeField] private LateralZoneVisualEntry insideRight;

    [Header("Izquierda")]
    [SerializeField] private LateralZoneVisualEntry outsideLeft;
    [SerializeField] private LateralZoneVisualEntry insideLeft;

    private LateralVisualPhase? activePhase;
    private int scoreAtPhaseStart;
    private bool isCleanForActivePhase;

    private void OnEnable()
    {
        if (BrushingScoreManager.Instance != null)
            BrushingScoreManager.Instance.ScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        if (BrushingScoreManager.Instance != null)
            BrushingScoreManager.Instance.ScoreChanged -= HandleScoreChanged;
    }

    public void BeginPhase(LateralVisualPhase phase)
    {
        activePhase = phase;
        isCleanForActivePhase = false;
        scoreAtPhaseStart = BrushingScoreManager.Instance != null
            ? BrushingScoreManager.Instance.CurrentScore
            : 0;

        GetEntry(phase)?.SetDirty();
    }

    public void EndPhase(LateralVisualPhase phase)
    {
        if (activePhase != phase)
            return;

        activePhase = null;
    }

    private void HandleScoreChanged(int totalScore)
    {
        if (activePhase == null || isCleanForActivePhase)
            return;

        int earnedThisPhase = totalScore - scoreAtPhaseStart;
        if (earnedThisPhase < pointsRequired)
            return;

        isCleanForActivePhase = true;
        GetEntry(activePhase.Value)?.SetClean();
    }

    private LateralZoneVisualEntry GetEntry(LateralVisualPhase phase)
    {
        switch (phase)
        {
            case LateralVisualPhase.OutsideRight: return outsideRight;
            case LateralVisualPhase.InsideRight: return insideRight;
            case LateralVisualPhase.OutsideLeft: return outsideLeft;
            case LateralVisualPhase.InsideLeft: return insideLeft;
            default: return null;
        }
    }
}

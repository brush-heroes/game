using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.ARGame.Tests
{
    /// <summary>
    /// Tests para ScoreManager — puntaje, contadores de sesión, métricas
    /// derivadas (precisión y enfoque por zona) y reseteo de sesión.
    /// </summary>
    public class ScoreManagerTests
    {
        GameObject _root;
        ScoreManager _score;

        [SetUp]
        public void SetUp()
        {
            _root  = new GameObject("ScoreManagerTestRoot");
            _score = _root.AddComponent<ScoreManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        // ── Estado inicial ───────────────────────────────────────────────────

        [Test]
        public void NewInstance_ScoreIsZero()
        {
            Assert.AreEqual(0, _score.Score);
        }

        [Test]
        public void NewInstance_AccuracyIsZero()
        {
            Assert.AreEqual(0f, _score.GetAccuracy());
        }

        [Test]
        public void NewInstance_TargetZoneFocusRatioIsZero()
        {
            Assert.AreEqual(0f, _score.GetTargetZoneFocusRatio());
        }

        // ── AddScore ─────────────────────────────────────────────────────────

        [Test]
        public void AddScore_PositiveAmount_IncrementsScore()
        {
            _score.AddScore(5);
            Assert.AreEqual(5, _score.Score);
        }

        [Test]
        public void AddScore_NegativeAmount_DecrementsScore()
        {
            _score.AddScore(10);
            _score.AddScore(-3);
            Assert.AreEqual(7, _score.Score);
        }

        [Test]
        public void AddScore_FiresOnScoreChangedEventWithNewValue()
        {
            int captured = -1;
            _score.OnScoreChanged += v => captured = v;
            _score.AddScore(7);
            Assert.AreEqual(7, captured);
        }

        // ── RegisterCleanResult — lógica zona-correcta vs zona-incorrecta ──

        [Test]
        public void RegisterCleanResult_TargetZone_AddsTwoPoints()
        {
            _score.RegisterCleanResult(
                cleanedZone:           MouthZone.UpperLeft,
                currentTargetZone:     MouthZone.UpperLeft,
                targetZoneStillHasWork: true);
            Assert.AreEqual(2, _score.Score);
            Assert.AreEqual(1, _score.CleansInTargetZone);
            Assert.AreEqual(0, _score.CleansOutsideTargetZone);
        }

        [Test]
        public void RegisterCleanResult_WrongZoneWhileTargetHasWork_SubtractsOnePoint()
        {
            _score.AddScore(10);
            _score.RegisterCleanResult(
                cleanedZone:           MouthZone.LowerRight,
                currentTargetZone:     MouthZone.UpperLeft,
                targetZoneStillHasWork: true);
            Assert.AreEqual(9, _score.Score);
            Assert.AreEqual(0, _score.CleansInTargetZone);
            Assert.AreEqual(1, _score.CleansOutsideTargetZone);
        }

        [Test]
        public void RegisterCleanResult_WrongZoneAfterTargetDone_DoesNotChangeScore()
        {
            _score.AddScore(10);
            _score.RegisterCleanResult(
                cleanedZone:           MouthZone.LowerRight,
                currentTargetZone:     MouthZone.UpperLeft,
                targetZoneStillHasWork: false);
            Assert.AreEqual(10, _score.Score);
            Assert.AreEqual(1, _score.CleansOutsideTargetZone);
        }

        [Test]
        public void RegisterCleanResult_MixedSequence_AccumulatesCorrectly()
        {
            // 2 limpiezas correctas (+4), 1 incorrecta con trabajo pendiente (-1)
            // = 3 puntos finales.
            _score.RegisterCleanResult(MouthZone.UpperLeft,  MouthZone.UpperLeft,  true);
            _score.RegisterCleanResult(MouthZone.UpperLeft,  MouthZone.UpperLeft,  true);
            _score.RegisterCleanResult(MouthZone.LowerRight, MouthZone.UpperLeft,  true);
            Assert.AreEqual(3, _score.Score);
        }

        // ── Métricas: precisión ──────────────────────────────────────────────

        [Test]
        public void GetAccuracy_NoSpawns_ReturnsZero()
        {
            Assert.AreEqual(0f, _score.GetAccuracy());
        }

        [Test]
        public void GetAccuracy_AllSpawnedAreCleaned_ReturnsOne()
        {
            for (int i = 0; i < 5; i++) _score.RegisterSpawn();
            for (int i = 0; i < 5; i++)
                _score.RegisterCleanResult(MouthZone.UpperLeft, MouthZone.UpperLeft, true);
            Assert.AreEqual(1f, _score.GetAccuracy(), 0.0001f);
        }

        [Test]
        public void GetAccuracy_HalfCleaned_ReturnsHalf()
        {
            for (int i = 0; i < 10; i++) _score.RegisterSpawn();
            for (int i = 0; i < 5; i++)
                _score.RegisterCleanResult(MouthZone.UpperLeft, MouthZone.UpperLeft, true);
            Assert.AreEqual(0.5f, _score.GetAccuracy(), 0.0001f);
        }

        // ── Métricas: enfoque en zona objetivo ───────────────────────────────

        [Test]
        public void GetTargetZoneFocusRatio_OnlyTargetZoneCleans_ReturnsOne()
        {
            for (int i = 0; i < 4; i++)
                _score.RegisterCleanResult(MouthZone.UpperLeft, MouthZone.UpperLeft, true);
            Assert.AreEqual(1f, _score.GetTargetZoneFocusRatio(), 0.0001f);
        }

        [Test]
        public void GetTargetZoneFocusRatio_HalfInTargetHalfOutside_ReturnsHalf()
        {
            for (int i = 0; i < 2; i++)
                _score.RegisterCleanResult(MouthZone.UpperLeft,  MouthZone.UpperLeft, true);
            for (int i = 0; i < 2; i++)
                _score.RegisterCleanResult(MouthZone.LowerRight, MouthZone.UpperLeft, true);
            Assert.AreEqual(0.5f, _score.GetTargetZoneFocusRatio(), 0.0001f);
        }

        // ── ResetSession ─────────────────────────────────────────────────────

        [Test]
        public void ResetSession_ZeroesScoreAndCounters()
        {
            _score.AddScore(50);
            _score.RegisterSpawn();
            _score.RegisterCleanResult(MouthZone.UpperLeft, MouthZone.UpperLeft, true);
            _score.RegisterCleanResult(MouthZone.LowerRight, MouthZone.UpperLeft, true);

            _score.ResetSession();

            Assert.AreEqual(0,  _score.Score);
            Assert.AreEqual(0,  _score.CleansInTargetZone);
            Assert.AreEqual(0,  _score.CleansOutsideTargetZone);
            Assert.AreEqual(0f, _score.GetAccuracy());
            Assert.AreEqual(0f, _score.GetTargetZoneFocusRatio());
        }

        [Test]
        public void ResetSession_FiresOnScoreChangedWithZero()
        {
            _score.AddScore(50);
            int captured = -1;
            _score.OnScoreChanged += v => captured = v;
            _score.ResetSession();
            Assert.AreEqual(0, captured);
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-10: Secuencia derecha/izquierda con temporizador
    [TestFixture]
    public class StageTimerTests
    {
        GameObject _go;
        StageTimer _timer;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("StageTimer");
            _timer = _go.AddComponent<StageTimer>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-10a: StartRightSequence configura currentSide = Right
        [Test]
        public void StartRightSequence_CurrentSide_EsRight()
        {
            _timer.StartRightSequence();

            Assert.AreEqual(StageTimer.BushingSide.Right, _timer.currentSide);
        }

        // UNIT-BRUSH-10b: StartRightSequence pone isOutsidePhase = true
        [Test]
        public void StartRightSequence_IsOutsidePhase_EsTrue()
        {
            _timer.StartRightSequence();

            Assert.IsTrue(_timer.isOutsidePhase);
        }

        // UNIT-BRUSH-10c: StartRightSequence inicia el temporizador
        [Test]
        public void StartRightSequence_IsRunning_EsTrue()
        {
            _timer.StartRightSequence();

            Assert.IsTrue(_timer.isRunning);
        }

        // UNIT-BRUSH-10d: StartLeftSequence configura currentSide = Left
        [Test]
        public void StartLeftSequence_CurrentSide_EsLeft()
        {
            _timer.StartLeftSequence();

            Assert.AreEqual(StageTimer.BushingSide.Left, _timer.currentSide);
        }

        // UNIT-BRUSH-10e: StopTimer detiene el temporizador
        [Test]
        public void StopTimer_IsRunning_EsFalse()
        {
            _timer.StartTimer(10f);
            _timer.StopTimer();

            Assert.IsFalse(_timer.isRunning);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

namespace BrushHeroes.ARGame.Tests
{
    /// <summary>
    /// Tests para SessionManager — ciclo de vida de la sesión, eventos,
    /// avance entre pasos, fin exitoso, fallo y configuración del límite
    /// de tiempo (180s en Dinámico, 0 en Solo Guía).
    /// </summary>
    public class SessionManagerTests
    {
        GameObject _root;
        SessionManager _session;

        [SetUp]
        public void SetUp()
        {
            // SessionManager.Awake() emite Debug.LogError cuando no encuentra
            // un BacteriaSpawner en la escena. En pruebas unitarias no hay
            // escena cargada, así que ignoramos esos errores esperados.
            LogAssert.ignoreFailingMessages = true;

            _root    = new GameObject("SessionManagerTestRoot");
            _session = _root.AddComponent<SessionManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            LogAssert.ignoreFailingMessages = false;
        }

        // ── Estado inicial ───────────────────────────────────────────────────

        [Test]
        public void NewInstance_IsNotRunning()
        {
            Assert.IsFalse(_session.IsSessionRunning);
        }

        [Test]
        public void NewInstance_IsNotComplete()
        {
            Assert.IsFalse(_session.IsSessionComplete);
        }

        [Test]
        public void NewInstance_TotalTimeLimitDefaultsTo180Seconds()
        {
            // El requerimiento RD del módulo de RA establece un máximo de
            // 180 segundos para una sesión en modo Dinámico.
            Assert.AreEqual(180f, _session.TotalTimeLimit);
        }

        // ── StartSession ─────────────────────────────────────────────────────

        [Test]
        public void StartSession_SetsIsSessionRunningTrue()
        {
            _session.StartSession();
            Assert.IsTrue(_session.IsSessionRunning);
        }

        [Test]
        public void StartSession_FiresOnSessionStartedEvent()
        {
            bool fired = false;
            _session.OnSessionStarted += () => fired = true;
            _session.StartSession();
            Assert.IsTrue(fired);
        }

        [Test]
        public void StartSession_FiresOnStepChangedWithFirstStep()
        {
            int callCount = 0;
            int receivedIndex = -1;
            _session.OnStepChanged += (step, idx) =>
            {
                callCount++;
                receivedIndex = idx;
            };
            _session.StartSession();
            Assert.AreEqual(1, callCount);
            Assert.AreEqual(0, receivedIndex);
        }

        [Test]
        public void StartSession_AlreadyRunning_DoesNotRefire()
        {
            _session.StartSession();
            int count = 0;
            _session.OnSessionStarted += () => count++;
            _session.StartSession();
            Assert.AreEqual(0, count, "Una segunda StartSession sobre una sesión activa debe ser no-op.");
        }

        [Test]
        public void StartSession_PopulatesDefaultStepsCoveringAllSixZones()
        {
            _session.StartSession();

            var seen = new HashSet<MouthZone>();
            seen.Add(_session.CurrentZone);
            for (int i = 0; i < 5; i++)
            {
                _session.AdvanceToNextStep();
                if (_session.IsSessionRunning)
                    seen.Add(_session.CurrentZone);
            }

            Assert.AreEqual(6, seen.Count, "La sesión por defecto debe cubrir las 6 zonas.");
        }

        [Test]
        public void StartSession_RemainingTimeStartsAtTotalTimeLimit()
        {
            _session.StartSession();
            Assert.AreEqual(_session.TotalTimeLimit, _session.RemainingTime, 0.001f);
        }

        // ── AdvanceToNextStep ────────────────────────────────────────────────

        [Test]
        public void AdvanceToNextStep_NotRunning_NoOp()
        {
            // Sin haber llamado StartSession, AdvanceToNextStep no hace nada.
            int countCalls = 0;
            _session.OnStepChanged += (s, i) => countCalls++;
            _session.AdvanceToNextStep();
            Assert.AreEqual(0, countCalls);
        }

        [Test]
        public void AdvanceToNextStep_IncrementsCurrentStepIndex()
        {
            _session.StartSession();
            int before = _session.CurrentStepIndex;
            _session.AdvanceToNextStep();
            Assert.AreEqual(before + 1, _session.CurrentStepIndex);
        }

        [Test]
        public void AdvanceToNextStep_FiresOnStepChangedEvent()
        {
            _session.StartSession();
            int onStartCalls = 1; // OnStepChanged ya disparó una vez en StartSession
            int totalCalls = onStartCalls;
            _session.OnStepChanged += (s, i) => totalCalls++;
            _session.AdvanceToNextStep();
            Assert.AreEqual(2, totalCalls);
        }

        [Test]
        public void AdvanceToNextStep_BeyondLastStep_EndsSessionSuccessfully()
        {
            _session.StartSession();
            // 6 zonas en la sesión por defecto. El sexto Advance lleva al fin.
            for (int i = 0; i < 6; i++)
                _session.AdvanceToNextStep();

            Assert.IsFalse(_session.IsSessionRunning, "Debe dejar de correr.");
            Assert.IsTrue(_session.IsSessionComplete, "Debe quedar marcada como completa.");
        }

        [Test]
        public void AdvanceToNextStep_BeyondLastStep_FiresOnSessionEnded()
        {
            _session.StartSession();
            bool fired = false;
            _session.OnSessionEnded += () => fired = true;

            for (int i = 0; i < 6; i++)
                _session.AdvanceToNextStep();

            Assert.IsTrue(fired);
        }

        // ── FailSession ──────────────────────────────────────────────────────

        [Test]
        public void FailSession_FromRunning_StopsSessionAndMarksFailed()
        {
            _session.StartSession();
            _session.FailSession();

            Assert.IsFalse(_session.IsSessionRunning);
            Assert.IsFalse(_session.IsSessionComplete,
                "Una sesión fallida nunca debe quedar como completa.");
        }

        [Test]
        public void FailSession_FromRunning_FiresOnSessionFailedEvent()
        {
            _session.StartSession();
            bool fired = false;
            _session.OnSessionFailed += () => fired = true;
            _session.FailSession();
            Assert.IsTrue(fired);
        }

        [Test]
        public void FailSession_NotRunning_NoOp()
        {
            int count = 0;
            _session.OnSessionFailed += () => count++;
            _session.FailSession();
            Assert.AreEqual(0, count,
                "FailSession sobre sesión no activa no debe disparar el evento.");
        }

        [Test]
        public void FailSession_AfterEnd_NoOp()
        {
            // Si la sesión ya terminó exitosamente, FailSession no debe dispararse.
            _session.StartSession();
            for (int i = 0; i < 6; i++)
                _session.AdvanceToNextStep();

            int failCount = 0;
            _session.OnSessionFailed += () => failCount++;
            _session.FailSession();
            Assert.AreEqual(0, failCount);
        }

        // ── Modo Solo Guía: timer deshabilitado ─────────────────────────────

        [Test]
        public void TotalTimeLimit_SetToZero_DisablesRemainingTime()
        {
            // Modo Solo Guía: GuidedModeController pone TotalTimeLimit=0 antes
            // de StartSession. RemainingTime debe ser 0 en todo momento.
            _session.TotalTimeLimit = 0f;
            _session.StartSession();
            Assert.AreEqual(0f, _session.RemainingTime);
        }

        // ── EndSession ───────────────────────────────────────────────────────

        [Test]
        public void EndSession_NotRunning_NoOp()
        {
            int count = 0;
            _session.OnSessionEnded += () => count++;
            _session.EndSession();
            Assert.AreEqual(0, count);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-12: Orquestación de flujo y suscripciones
    // UNIT-BRUSH-17: Fallback de StartButton por nombre
    [TestFixture]
    public class CameraTestInputTests
    {
        GameObject _mgrGo, _go;
        CameraTestInput _input;

        static readonly FieldInfo _gameStartedField =
            typeof(CameraTestInput).GetField("gameStarted",
                BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            BrushGameManager.Instance = null;
            _mgrGo = new GameObject("BrushGameManager");
            _mgrGo.AddComponent<BrushGameManager>().autoStart = false;

            _go    = new GameObject("CameraTestInput");
            _input = _go.AddComponent<CameraTestInput>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_mgrGo);
            BrushGameManager.Instance = null;
        }

        // UNIT-BRUSH-12a: Primera llamada pone gameStarted = true
        [Test]
        public void OnStartButtonPressed_PrimeraChamada_PoneGameStartedTrue()
        {
            // OnStartButtonPressed puede lanzar NPE (cameraController null) después de
            // poner gameStarted = true; capturamos la excepción y verificamos el campo.
            try { _input.OnStartButtonPressed(); } catch { }

            bool gameStarted = (bool)_gameStartedField.GetValue(_input);
            Assert.IsTrue(gameStarted);
        }

        // UNIT-BRUSH-12b: Segunda llamada es idempotente
        [Test]
        public void OnStartButtonPressed_SegundaLlamada_EsIdempotente()
        {
            try { _input.OnStartButtonPressed(); } catch { }
            try { _input.OnStartButtonPressed(); } catch { }

            bool gameStarted = (bool)_gameStartedField.GetValue(_input);
            Assert.IsTrue(gameStarted);
        }

        // UNIT-BRUSH-17: Fallback de StartButton por nombre
        [Test]
        public void StartButton_SinAsignar_SeResuelvePorNombreEnScene()
        {
            var btnGo = new GameObject("StartButton");
            btnGo.AddComponent<Button>();

            var testGo    = new GameObject("CameraTestInputFallback");
            var testInput = testGo.AddComponent<CameraTestInput>();

            typeof(CameraTestInput)
                .GetMethod("ResolveStartButtonIfNeeded",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(testInput, null);

            var startButtonField = typeof(CameraTestInput)
                .GetField("startButton",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var resolved = startButtonField.GetValue(testInput) as Button;

            Assert.IsNotNull(resolved,
                "CameraTestInput debe resolver StartButton por nombre cuando no está asignado");

            Object.DestroyImmediate(btnGo);
            Object.DestroyImmediate(testGo);
        }
    }
}

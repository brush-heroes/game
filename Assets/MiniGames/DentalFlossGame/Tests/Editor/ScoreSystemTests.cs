using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.DentalFlossGame.Tests
{
    // UNIT-FLOSS-08: Singleton y actualización de texto de score
    [TestFixture]
    public class ScoreSystemTests
    {
        GameObject _go;
        ScoreSystem _sys;

        static readonly MethodInfo _awakeMethod =
            typeof(ScoreSystem).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            ScoreSystem.Instance = null;
            _go = new GameObject("ScoreSystem");
            _sys = _go.AddComponent<ScoreSystem>();
            _awakeMethod.Invoke(_sys, null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            ScoreSystem.Instance = null;
        }

        // UNIT-FLOSS-08a: Instancia singleton no es null
        [Test]
        public void Singleton_Instance_NoEsNull()
        {
            Assert.IsNotNull(ScoreSystem.Instance);
        }

        // UNIT-FLOSS-08b: Score inicial es 0
        [Test]
        public void Score_Inicial_EsCero()
        {
            Assert.AreEqual(0, _sys.score);
        }

        // UNIT-FLOSS-08c: AddScore acumula el puntaje
        [Test]
        public void AddScore_Positivo_AcumulaPuntaje()
        {
            _sys.AddScore(50);

            Assert.AreEqual(50, _sys.score);
        }

        // UNIT-FLOSS-08d: Score interno y score del singleton se sincronizan
        [Test]
        public void AddScore_ViaInstancia_ScoreInterno_Sincronizado()
        {
            ScoreSystem.Instance.AddScore(30);
            ScoreSystem.Instance.AddScore(20);

            Assert.AreEqual(50, ScoreSystem.Instance.score);
        }
    }
}

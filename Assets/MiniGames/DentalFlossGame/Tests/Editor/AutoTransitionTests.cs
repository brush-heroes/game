using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.DentalFlossGame.Tests
{
    // UNIT-FLOSS-09: Cambio automático de escena por tiempo
    [TestFixture]
    public class AutoTransitionTests
    {
        GameObject _go;
        AutoTransition _trans;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("AutoTransition");
            _trans = _go.AddComponent<AutoTransition>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-FLOSS-09a: waitTime por defecto es 3.5 segundos
        [Test]
        public void WaitTime_PorDefecto_Es3p5()
        {
            Assert.AreEqual(3.5f, _trans.waitTime, 0.001f,
                "La transición debe ocurrir tras 3.5 segundos por defecto");
        }

        // UNIT-FLOSS-09b: secondSceneName tiene valor por defecto no vacío
        [Test]
        public void SecondSceneName_PorDefecto_NoEsVacio()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_trans.secondSceneName),
                "secondSceneName debe tener un nombre de escena destino por defecto");
        }

        // UNIT-FLOSS-09c: waitTime configurable
        [Test]
        public void WaitTime_Configurable_PersisteCambio()
        {
            _trans.waitTime = 5f;
            Assert.AreEqual(5f, _trans.waitTime, 0.001f);
        }
    }
}

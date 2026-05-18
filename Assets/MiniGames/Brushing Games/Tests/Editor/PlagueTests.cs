using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-18: Colisión con cepillo
    // Nota: OnTriggerEnter2D requiere física (Play Mode).
    // Este test verifica que Plaque es un MonoBehaviour sin estado público.
    [TestFixture]
    public class PlagueTests
    {
        GameObject _go;
        Plaque _plaque;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("Plaque");
            _plaque = _go.AddComponent<Plaque>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-18a: Plaque es un MonoBehaviour
        [Test]
        public void Plaque_Es_MonoBehaviour()
        {
            Assert.IsInstanceOf<MonoBehaviour>(_plaque);
        }

        // UNIT-BRUSH-18b: Plaque no tiene campos públicos de configuración (solo comportamiento en trigger)
        [Test]
        public void Plaque_SinCamposPublicos_DeConfiguracion()
        {
            var publicFields = typeof(Plaque)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            Assert.AreEqual(0, publicFields.Length,
                "Plaque no debe exponer campos públicos; su comportamiento es solo por trigger");
        }

        // UNIT-BRUSH-18c: GameObject de Plaque existe antes de colisión con cepillo
        [Test]
        public void Plaque_AntesDeTrigger_GameObjectActivo()
        {
            Assert.IsTrue(_go.activeSelf,
                "El GameObject de Plaque debe estar activo antes de cualquier colisión");
        }
    }
}

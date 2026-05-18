using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-08: Trigger de suciedad y daño continuo
    // Nota: OnTriggerEnter2D/Exit2D requieren física (Play Mode). Este test verifica configuración.
    [TestFixture]
    public class BrushCleanerTests
    {
        GameObject _go;
        BrushCleaner _cleaner;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("BrushCleaner");
            _cleaner = _go.AddComponent<BrushCleaner>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-08a: damageCooldown por defecto es 0.3 segundos
        [Test]
        public void DamageCooldown_PorDefecto_Es0p3()
        {
            Assert.AreEqual(0.3f, _cleaner.damageCooldown, 0.001f,
                "El cooldown de daño debe ser 0.3 segundos por defecto");
        }

        // UNIT-BRUSH-08b: currentDirt es null al instanciar
        [Test]
        public void CurrentDirt_AlInstanciar_EsNull()
        {
            Assert.IsNull(_cleaner.currentDirt,
                "currentDirt debe ser null antes de entrar en contacto con suciedad");
        }
    }
}

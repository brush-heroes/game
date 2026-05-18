using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-06: Daño vertical con cooldown
    // Nota: OnTriggerStay2D requiere física (Play Mode). Este test verifica configuración.
    [TestFixture]
    public class BrushDirtTests
    {
        GameObject _go;
        BrushDirt _dirt;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("BrushDirt");
            _dirt = _go.AddComponent<BrushDirt>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-06a: damageCooldown por defecto es 0.3 segundos
        [Test]
        public void DamageCooldown_PorDefecto_Es0p3()
        {
            var field = typeof(BrushDirt)
                .GetField("damageCooldown", BindingFlags.NonPublic | BindingFlags.Instance);

            float cooldown = (float)field.GetValue(_dirt);

            Assert.AreEqual(0.3f, cooldown, 0.001f,
                "El cooldown de daño debe ser 0.3 segundos por defecto");
        }

        // UNIT-BRUSH-06b: lastDamageTime inicia en 0
        [Test]
        public void LastDamageTime_AlInstanciar_EsCero()
        {
            var field = typeof(BrushDirt)
                .GetField("lastDamageTime", BindingFlags.NonPublic | BindingFlags.Instance);

            float lastTime = (float)field.GetValue(_dirt);

            Assert.AreEqual(0f, lastTime, 0.001f);
        }
    }
}

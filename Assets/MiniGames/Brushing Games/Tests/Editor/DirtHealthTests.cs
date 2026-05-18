using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    [TestFixture]
    public class DirtHealthTests
    {
        GameObject _go;
        DirtHealth _health;

        static readonly MethodInfo _start =
            typeof(DirtHealth).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo _currentHealth =
            typeof(DirtHealth).GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            _go     = new GameObject("DirtHealth");
            _health = _go.AddComponent<DirtHealth>();
            _start.Invoke(_health, null);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-09a
        [Test] public void MaxHealth_PorDefecto_Es3() => Assert.AreEqual(3, _health.maxHealth);

        // UNIT-BRUSH-09b
        [Test] public void IsDead_Inicial_EsFalse() => Assert.IsFalse(_health.IsDead());

        // UNIT-BRUSH-09c: Verificar la condición de muerte directamente (sin triggear Destroy)
        [Test]
        public void IsDead_TrasDanioTotal_EsTrue()
        {
            // Inyectar currentHealth = 0 via reflexión evita que TakeDamage llame
            // Destroy(gameObject) (inmediato en Edit Mode) antes de que podamos verificar.
            _currentHealth.SetValue(_health, 0);

            Assert.IsTrue(_health.IsDead(),
                "Con currentHealth=0, IsDead debe retornar true");
        }

        // UNIT-BRUSH-09d
        [Test]
        public void IsDead_VidaParcial_EsFalse()
        {
            _health.TakeDamage(1);
            Assert.IsFalse(_health.IsDead());
        }
    }
}

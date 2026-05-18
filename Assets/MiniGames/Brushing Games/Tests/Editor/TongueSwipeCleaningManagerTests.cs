using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-15: Pasadas verticales válidas hasta completion
    // Nota: Seguimiento del movimiento del cepillo requiere Play Mode.
    // Este test verifica la configuración de pasadas requeridas.
    [TestFixture]
    public class TongueSwipeCleaningManagerTests
    {
        GameObject _go;
        TongueSwipeCleaningManager _mgr;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TongueSwipeCleaningManager");
            _mgr = _go.AddComponent<TongueSwipeCleaningManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-15a: minRequiredStrokes es 5 por defecto
        [Test]
        public void MinRequiredStrokes_PorDefecto_Es5()
        {
            var field = typeof(TongueSwipeCleaningManager)
                .GetField("minRequiredStrokes",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            int min = (int)field.GetValue(_mgr);

            Assert.AreEqual(5, min,
                "Se necesitan al menos 5 pasadas verticales válidas");
        }

        // UNIT-BRUSH-15b: maxRequiredStrokes es 10 por defecto
        [Test]
        public void MaxRequiredStrokes_PorDefecto_Es10()
        {
            var field = typeof(TongueSwipeCleaningManager)
                .GetField("maxRequiredStrokes",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            int max = (int)field.GetValue(_mgr);

            Assert.AreEqual(10, max,
                "El máximo de pasadas requeridas es 10");
        }

        // UNIT-BRUSH-15c: mechanicActive es false antes de StartCleaningMechanic
        [Test]
        public void MechanicActive_Inicial_EsFalse()
        {
            var field = typeof(TongueSwipeCleaningManager)
                .GetField("mechanicActive",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            bool active = (bool)field.GetValue(_mgr);

            Assert.IsFalse(active,
                "La mecánica no debe estar activa antes de iniciarla");
        }
    }
}

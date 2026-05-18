using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-07: Detección de gesto circular
    // Nota: La detección real requiere Input (Play Mode). Este test verifica configuración.
    [TestFixture]
    public class BrushCircleDetectorTests
    {
        GameObject _go;
        BrushCircleDetector _detector;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("BrushCircleDetector");
            _detector = _go.AddComponent<BrushCircleDetector>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-07a: closeDistance por defecto
        [Test]
        public void CloseDistance_PorDefecto_Es1()
        {
            Assert.AreEqual(1.0f, _detector.closeDistance, 0.001f);
        }

        // UNIT-BRUSH-07b: minPoints por defecto
        [Test]
        public void MinPoints_PorDefecto_Es6()
        {
            Assert.AreEqual(6, _detector.minPoints);
        }

        // UNIT-BRUSH-07c: minMovement por defecto
        [Test]
        public void MinMovement_PorDefecto_Es0p05()
        {
            Assert.AreEqual(0.05f, _detector.minMovement, 0.001f);
        }
    }
}

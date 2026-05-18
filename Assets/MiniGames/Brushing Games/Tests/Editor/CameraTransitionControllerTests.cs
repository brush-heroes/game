using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-11: Transiciones de cámara
    // Nota: movimiento real de cámara requiere Play Mode. Este test verifica configuración.
    [TestFixture]
    public class CameraTransitionControllerTests
    {
        GameObject _go;
        CameraTransitionController _ctrl;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("CameraTransitionController");
            _ctrl = _go.AddComponent<CameraTransitionController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-11a: moveDuration por defecto es 1 segundo
        [Test]
        public void MoveDuration_PorDefecto_Es1()
        {
            Assert.AreEqual(1f, _ctrl.moveDuration, 0.001f);
        }

        // UNIT-BRUSH-11b: normalSize por defecto es 5
        [Test]
        public void NormalSize_PorDefecto_Es5()
        {
            Assert.AreEqual(5f, _ctrl.normalSize, 0.001f);
        }

        // UNIT-BRUSH-11c: outsideZoomSize por defecto es 3
        [Test]
        public void OutsideZoomSize_PorDefecto_Es3()
        {
            Assert.AreEqual(3f, _ctrl.outsideZoomSize, 0.001f);
        }

        // UNIT-BRUSH-11d: MoveDuration accesible via propiedad pública
        [Test]
        public void MoveDurationProperty_Coincide_ConCampo()
        {
            _ctrl.moveDuration = 2f;
            Assert.AreEqual(2f, _ctrl.MoveDuration, 0.001f);
        }
    }
}

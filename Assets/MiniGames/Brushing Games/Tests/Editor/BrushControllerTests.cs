using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-03: Modo zoom/espejo/poses — rotación y escala consistentes con el flujo
    [TestFixture]
    public class BrushControllerTests
    {
        GameObject _go;
        BrushController _ctrl;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("BrushController");
            _ctrl = _go.AddComponent<BrushController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-03a: SetZoomMode(true) aplica zoomScale
        [Test]
        public void SetZoomMode_True_AplicaZoomScale()
        {
            _ctrl.SetZoomMode(true);

            float absX = Mathf.Abs(_go.transform.localScale.x);
            Assert.AreEqual(_ctrl.zoomScale.x, absX, 0.001f,
                "Con zoom activado, la escala absoluta en X debe ser zoomScale.x");
        }

        // UNIT-BRUSH-03b: SetZoomMode(false) aplica normalScale
        [Test]
        public void SetZoomMode_False_AplicaNormalScale()
        {
            _ctrl.SetZoomMode(true);
            _ctrl.SetZoomMode(false);

            float absX = Mathf.Abs(_go.transform.localScale.x);
            Assert.AreEqual(_ctrl.normalScale.x, absX, 0.001f,
                "Con zoom desactivado, la escala absoluta en X debe ser normalScale.x");
        }

        // UNIT-BRUSH-03c: MirrorDirection invierte el signo de la escala X
        [Test]
        public void MirrorDirection_InvierteEscalaX()
        {
            _ctrl.SetFollowEnabled(false);
            _ctrl.SetZoomMode(false);

            float scaleAntes = _go.transform.localScale.x;

            _ctrl.MirrorDirection();

            float scaleDespues = _go.transform.localScale.x;

            Assert.AreEqual(-scaleAntes, scaleDespues, 0.001f,
                "MirrorDirection debe invertir el signo de localScale.x");
        }

        // UNIT-BRUSH-03d: SetStartPose aplica rotación de cerdas hacia abajo
        [Test]
        public void SetStartPose_AplicaRotacionBristlesDown()
        {
            _ctrl.SetStartPose();

            var expectedRot = Quaternion.Euler(_ctrl.bristlesDownRotationEuler);
            Assert.AreEqual(expectedRot.eulerAngles.z,
                _go.transform.rotation.eulerAngles.z, 0.5f,
                "SetStartPose debe aplicar la rotación de cerdas hacia abajo");
        }
    }
}

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

        // UNIT-BRUSH-03d: SetStartPose aplica rotación de cerdas hacia la izquierda
        [Test]
        public void SetStartPose_AplicaRotacionBristlesLeft()
        {
            _ctrl.SetStartPose();

            var expectedRot = Quaternion.Euler(_ctrl.verticalBristlesLeftEuler);
            Assert.AreEqual(expectedRot.eulerAngles.z,
                _go.transform.rotation.eulerAngles.z, 0.5f,
                "SetStartPose debe aplicar la rotación de cerdas hacia la izquierda");
        }

        // UNIT-BRUSH-03e: Chewing Right → usa verticalBristlesRightEuler + espejo
        [Test]
        public void ApplyDirectionPose_ChewingRight_UsaEulerDeZonaDerecha()
        {
            _ctrl.verticalBristlesRightEuler = new Vector3(0f, 0f, 90f);
            _ctrl.SetZoomMode(false);
            _ctrl.ApplyDirectionPose(ZoneType.Chewing, BrushDirectionSubZone.Right);

            var expectedZ = Quaternion.Euler(_ctrl.verticalBristlesRightEuler).eulerAngles.z;
            Assert.AreEqual(expectedZ, _go.transform.rotation.eulerAngles.z, 0.5f);
            Assert.Less(_go.transform.localScale.x, 0f,
                "Chewing Right debe aplicar cepillo al revés (scale.x negativo)");
        }

        // UNIT-BRUSH-03f: Inside Right → misma rotación base, sin doble espejo
        [Test]
        public void ApplyDirectionPose_InsideRight_MismaRotacionBase()
        {
            _ctrl.ApplyDirectionPose(ZoneType.Inside, BrushDirectionSubZone.Right);

            var expectedZ = Quaternion.Euler(_ctrl.verticalBristlesLeftEuler).eulerAngles.z;
            Assert.AreEqual(expectedZ, _go.transform.rotation.eulerAngles.z, 0.5f);
            Assert.Greater(_go.transform.localScale.x, 0f);
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.Menu.Tests
{
    // UNIT-MENU-11: Orientación al entrar/salir de escena
    [TestFixture]
    public class OrientationControllerTests
    {
        GameObject _go;
        OrientationController _ctrl;

        static readonly FieldInfo _landscapeField =
            typeof(OrientationController).GetField("landscape",
                BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo _startMethod =
            typeof(OrientationController).GetMethod("Start",
                BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo _onDestroyMethod =
            typeof(OrientationController).GetMethod("OnDestroy",
                BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("OrientationController");
            _ctrl = _go.AddComponent<OrientationController>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        // UNIT-MENU-11a: landscape es true por defecto
        [Test]
        public void Landscape_PorDefecto_EsTrue()
        {
            bool landscape = (bool)_landscapeField.GetValue(_ctrl);

            Assert.IsTrue(landscape,
                "OrientationController debe configurar landscape=true por defecto");
        }

        // UNIT-MENU-11b: Start con landscape=true no lanza excepción
        // Nota: Screen.orientation no es modificable en Unity Editor (requiere dispositivo físico)
        [Test]
        public void Start_Landscape_NoLanzaExcepcion()
        {
            Assert.DoesNotThrow(() => _startMethod.Invoke(_ctrl, null),
                "Start() con landscape=true no debe lanzar excepciones");
        }

        // UNIT-MENU-11c: OnDestroy con landscape=true no lanza excepción
        [Test]
        public void OnDestroy_Landscape_NoLanzaExcepcion()
        {
            _startMethod.Invoke(_ctrl, null);

            Assert.DoesNotThrow(() => _onDestroyMethod.Invoke(_ctrl, null),
                "OnDestroy() no debe lanzar excepciones");

            _go = null;
        }

        // UNIT-MENU-11d: Start con landscape=false no lanza excepción
        [Test]
        public void Start_Portrait_NoLanzaExcepcion()
        {
            _landscapeField.SetValue(_ctrl, false);

            Assert.DoesNotThrow(() => _startMethod.Invoke(_ctrl, null),
                "Start() con landscape=false no debe lanzar excepciones");
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.Menu.Tests
{
    // UNIT-MENU-10: Cambio de pestaña y estilos del indicador activo
    [TestFixture]
    public class BottomNavControllerTests
    {
        GameObject _go;
        BottomNavController _ctrl;
        GameObject _minijuegosPage;
        GameObject _calendarioPage;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("BottomNavController");
            _ctrl = _go.AddComponent<BottomNavController>();

            _minijuegosPage = new GameObject("MinijuegosPage");
            _calendarioPage  = new GameObject("CalendarioPage");

            var type = typeof(BottomNavController);
            type.GetField("minijuegosPage", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_ctrl, _minijuegosPage);
            type.GetField("calendarioPage", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(_ctrl, _calendarioPage);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_minijuegosPage);
            Object.DestroyImmediate(_calendarioPage);
        }

        // UNIT-MENU-10a: ShowMinijuegos activa la página de minijuegos
        [Test]
        public void ShowMinijuegos_MinijuegosPaginaActiva()
        {
            _ctrl.ShowCalendario();
            _ctrl.ShowMinijuegos();

            Assert.IsTrue(_minijuegosPage.activeSelf,
                "La página de minijuegos debe estar activa tras ShowMinijuegos()");
        }

        // UNIT-MENU-10b: ShowMinijuegos desactiva la página de calendario
        [Test]
        public void ShowMinijuegos_CalendarioPaginaInactiva()
        {
            _ctrl.ShowCalendario();
            _ctrl.ShowMinijuegos();

            Assert.IsFalse(_calendarioPage.activeSelf,
                "La página de calendario debe estar inactiva tras ShowMinijuegos()");
        }

        // UNIT-MENU-10c: ShowCalendario activa la página de calendario
        [Test]
        public void ShowCalendario_CalendarioPaginaActiva()
        {
            _ctrl.ShowMinijuegos();
            _ctrl.ShowCalendario();

            Assert.IsTrue(_calendarioPage.activeSelf,
                "La página de calendario debe estar activa tras ShowCalendario()");
        }

        // UNIT-MENU-10d: ShowCalendario desactiva la página de minijuegos
        [Test]
        public void ShowCalendario_MinijuegosPaginaInactiva()
        {
            _ctrl.ShowMinijuegos();
            _ctrl.ShowCalendario();

            Assert.IsFalse(_minijuegosPage.activeSelf,
                "La página de minijuegos debe estar inactiva tras ShowCalendario()");
        }
    }
}

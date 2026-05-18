using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.Menu.Tests
{
    // UNIT-MENU-01: Inicialización y RefreshUI
    // UNIT-MENU-02: Navegación a minijuegos
    [TestFixture]
    public class MenuManagerTests
    {
        GameObject _go;
        MenuManager _mgr;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("MenuManager");
            _mgr = _go.AddComponent<MenuManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);

            // Limpiar PlayerDataManager si fue creado por MenuManager.Start
            foreach (var pdm in Object.FindObjectsOfType<PlayerDataManager>())
                Object.DestroyImmediate(pdm.gameObject);

            typeof(PlayerDataManager)
                .GetField("_instance",
                    BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, null);
        }

        // UNIT-MENU-01a: RefreshUI sin referencias UI no lanza excepción
        [Test]
        public void RefreshUI_SinReferenciasUI_NoLanzaExcepcion()
        {
            // Crear PlayerDataManager para que RefreshUI tenga datos
            var pdmGo = new GameObject("PlayerDataManager");
            pdmGo.AddComponent<PlayerDataManager>();

            Assert.DoesNotThrow(() => _mgr.RefreshUI(),
                "RefreshUI debe ser null-safe cuando las referencias de UI no están asignadas");

            Object.DestroyImmediate(pdmGo);
        }

        // UNIT-MENU-01b: MenuManager es un MonoBehaviour
        [Test]
        public void MenuManager_Es_MonoBehaviour()
        {
            Assert.IsInstanceOf<MonoBehaviour>(_mgr);
        }

        // UNIT-MENU-02a: LoadAR es un método público accesible
        [Test]
        public void LoadAR_MetodoPublico_Existe()
        {
            var method = typeof(MenuManager).GetMethod("LoadAR");
            Assert.IsNotNull(method, "MenuManager debe tener método público LoadAR()");
        }

        // UNIT-MENU-02b: LoadBrush es un método público accesible
        [Test]
        public void LoadBrush_MetodoPublico_Existe()
        {
            var method = typeof(MenuManager).GetMethod("LoadBrush");
            Assert.IsNotNull(method, "MenuManager debe tener método público LoadBrush()");
        }

        // UNIT-MENU-02c: LoadFloss es un método público accesible
        [Test]
        public void LoadFloss_MetodoPublico_Existe()
        {
            var method = typeof(MenuManager).GetMethod("LoadFloss");
            Assert.IsNotNull(method, "MenuManager debe tener método público LoadFloss()");
        }
    }
}

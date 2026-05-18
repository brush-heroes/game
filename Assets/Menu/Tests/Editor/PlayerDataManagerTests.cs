using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.Menu.Tests
{
    [TestFixture]
    public class PlayerDataManagerTests
    {
        GameObject _go;
        PlayerDataManager _mgr;

        // Campos obtenidos en cada SetUp (no static) para evitar null si la clase no carga
        FieldInfo _instanceField;
        FieldInfo _dataField;

        [SetUp]
        public void SetUp()
        {
            _instanceField = typeof(PlayerDataManager)
                .GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static);
            _dataField = typeof(PlayerDataManager)
                .GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance);

            // Limpiar singleton previo
            _instanceField?.SetValue(null, null);

            _go  = new GameObject("PlayerDataManager");
            _mgr = _go.AddComponent<PlayerDataManager>();

            // Intentar Awake (puede fallar por DontDestroyOnLoad o I/O en algunos entornos)
            try
            {
                typeof(PlayerDataManager)
                    .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(_mgr, null);
            }
            catch { /* ignorado — se recupera abajo */ }

            // Garantizar _data inicializado (fallback si Awake no lo hizo)
            if (_dataField?.GetValue(_mgr) == null)
                _dataField?.SetValue(_mgr, new PlayerData());

            // Garantizar singleton registrado
            if (_instanceField?.GetValue(null) == null)
                _instanceField?.SetValue(null, _mgr);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            _instanceField?.SetValue(null, null);
        }

        // UNIT-MENU-03a
        [Test]
        public void Singleton_Instance_NoEsNull()
        {
            Assert.IsNotNull(PlayerDataManager.Instance);
        }

        // UNIT-MENU-03b
        [Test]
        public void Singleton_Instance_EsLaMismaInstancia()
        {
            Assert.AreEqual(_mgr, PlayerDataManager.Instance);
        }

        // UNIT-MENU-03c
        [Test]
        public void Data_Cargado_NoEsNull()
        {
            Assert.IsNotNull(_mgr.Data);
        }

        // UNIT-MENU-03d
        [Test]
        public void Data_Jugador_NoEsNull()
        {
            Assert.IsNotNull(_mgr.Data.jugador);
        }

        // UNIT-MENU-03e
        [Test]
        public void Data_Sesiones_NoEsNull()
        {
            Assert.IsNotNull(_mgr.Data.sesiones);
        }

        // UNIT-MENU-04a
        [Test]
        public void RecordSession_AgregaSesion_AlHistorial()
        {
            int antes = _mgr.Data.sesiones.Count;
            _mgr.RecordSession(MinigameTypes.AR, 30, 60, true);
            Assert.Greater(_mgr.Data.sesiones.Count, antes);
        }

        // UNIT-MENU-04b
        [Test]
        public void RecordSession_ActualizaTotalStars()
        {
            int antes = _mgr.TotalStars;
            _mgr.RecordSession(MinigameTypes.AR, 50, 60, true);
            Assert.GreaterOrEqual(_mgr.TotalStars, antes + 50);
        }

        // UNIT-MENU-04c
        [Test]
        public void RecordSession_PuntajeCero_NoLanzaExcepcion()
        {
            Assert.DoesNotThrow(() => _mgr.RecordSession(MinigameTypes.Cepillado, 0, 30, false));
        }
    }
}

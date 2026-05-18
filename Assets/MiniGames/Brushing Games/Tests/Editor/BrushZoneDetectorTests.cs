using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-05: Coincidencia de zona activa vs esperada
    [TestFixture]
    public class BrushZoneDetectorTests
    {
        GameObject _mgrGo, _detectorGo, _zoneGo;
        BrushGameManager _mgr;
        BrushZoneDetector _detector;
        BrushZone _zone;

        static readonly MethodInfo _awake =
            typeof(BrushGameManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            BrushGameManager.Instance = null;

            _mgrGo = new GameObject("BrushGameManager");
            _mgr   = _mgrGo.AddComponent<BrushGameManager>();
            _mgr.autoStart = false;
            _awake.Invoke(_mgr, null); // Instance = _mgr

            _detectorGo = new GameObject("BrushZoneDetector");
            _detector   = _detectorGo.AddComponent<BrushZoneDetector>();

            _zoneGo = new GameObject("BrushZone");
            _zone   = _zoneGo.AddComponent<BrushZone>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_mgrGo);
            Object.DestroyImmediate(_detectorGo);
            Object.DestroyImmediate(_zoneGo);
            BrushGameManager.Instance = null;
        }

        // UNIT-BRUSH-05a
        [Test]
        public void IsCorrectZone_TipoYLadoCoindicen_DevuelveTrue()
        {
            _mgr.currentType = ZoneType.Chewing;  _mgr.currentSide = ZoneSide.Right;
            _zone.zoneType   = ZoneType.Chewing;  _zone.zoneSide   = ZoneSide.Right;
            _detector.currentZone = _zone;
            Assert.IsTrue(_detector.IsCorrectZone());
        }

        // UNIT-BRUSH-05b
        [Test]
        public void IsCorrectZone_TipoNoCoinicide_DevuelveFalse()
        {
            _mgr.currentType = ZoneType.Chewing;  _mgr.currentSide = ZoneSide.Right;
            _zone.zoneType   = ZoneType.Inside;   _zone.zoneSide   = ZoneSide.Right;
            _detector.currentZone = _zone;
            Assert.IsFalse(_detector.IsCorrectZone());
        }

        // UNIT-BRUSH-05c
        [Test]
        public void IsCorrectZone_LadoNoCoinicide_DevuelveFalse()
        {
            _mgr.currentType = ZoneType.Chewing;  _mgr.currentSide = ZoneSide.Right;
            _zone.zoneType   = ZoneType.Chewing;  _zone.zoneSide   = ZoneSide.Left;
            _detector.currentZone = _zone;
            Assert.IsFalse(_detector.IsCorrectZone());
        }

        // UNIT-BRUSH-05d
        [Test]
        public void IsCorrectZone_SinZonaActual_DevuelveFalse()
        {
            _detector.currentZone = null;
            Assert.IsFalse(_detector.IsCorrectZone());
        }
    }
}

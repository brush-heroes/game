using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-04: Integridad de enums y asignación de zona
    [TestFixture]
    public class BrushZoneTests
    {
        GameObject _go;
        BrushZone _zone;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("BrushZone");
            _zone = _go.AddComponent<BrushZone>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-04a: ZoneType persiste después de asignarse
        [Test]
        public void ZoneType_Asignado_Persiste()
        {
            _zone.zoneType = ZoneType.Inside;

            Assert.AreEqual(ZoneType.Inside, _zone.zoneType);
        }

        // UNIT-BRUSH-04b: ZoneSide persiste después de asignarse
        [Test]
        public void ZoneSide_Asignado_Persiste()
        {
            _zone.zoneSide = ZoneSide.Left;

            Assert.AreEqual(ZoneSide.Left, _zone.zoneSide);
        }

        // UNIT-BRUSH-04c: Combinación de ZoneType y ZoneSide persiste
        [Test]
        public void ZoneTypeYSide_Combinados_Persisten()
        {
            _zone.zoneType = ZoneType.Outside;
            _zone.zoneSide = ZoneSide.Right;

            Assert.AreEqual(ZoneType.Outside, _zone.zoneType);
            Assert.AreEqual(ZoneSide.Right, _zone.zoneSide);
        }

        // UNIT-BRUSH-04d: Los enums tienen los valores definidos por el protocolo
        [Test]
        public void Enums_ValoresDefinidos_Correctos()
        {
            Assert.AreEqual(3, System.Enum.GetValues(typeof(ZoneType)).Length,
                "ZoneType debe tener 3 valores: Chewing, Outside, Inside");
            Assert.AreEqual(2, System.Enum.GetValues(typeof(ZoneSide)).Length,
                "ZoneSide debe tener 2 valores: Left, Right");
        }
    }
}

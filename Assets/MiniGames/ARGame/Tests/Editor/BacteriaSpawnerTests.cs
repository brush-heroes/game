using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.ARGame.Tests
{
    /// <summary>
    /// Tests de seguridad para las consultas públicas de BacteriaSpawner sobre
    /// estado de zonas. La lógica de spawn requiere AR Foundation, MouthZoneManager
    /// y un ciclo Start/Update real, por lo que esa parte se verifica con pruebas
    /// de integración manuales (ver documento de pruebas).
    ///
    /// Aquí se valida que las consultas sobre un spawner sin inicializar no
    /// lancen excepciones y devuelvan valores neutros.
    /// </summary>
    public class BacteriaSpawnerTests
    {
        GameObject _root;
        BacteriaSpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _root    = new GameObject("BacteriaSpawnerTestRoot");
            _spawner = _root.AddComponent<BacteriaSpawner>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void GetSpawnedCount_UninitializedSpawner_ReturnsZero()
        {
            foreach (MouthZone zone in System.Enum.GetValues(typeof(MouthZone)))
                Assert.AreEqual(0, _spawner.GetSpawnedCount(zone),
                    $"GetSpawnedCount({zone}) debe devolver 0 en un spawner sin inicializar.");
        }

        [Test]
        public void GetRemainingCount_UninitializedSpawner_ReturnsZero()
        {
            foreach (MouthZone zone in System.Enum.GetValues(typeof(MouthZone)))
                Assert.AreEqual(0, _spawner.GetRemainingCount(zone));
        }

        [Test]
        public void GetMaxCount_UninitializedSpawner_ReturnsZero()
        {
            foreach (MouthZone zone in System.Enum.GetValues(typeof(MouthZone)))
                Assert.AreEqual(0, _spawner.GetMaxCount(zone));
        }

        [Test]
        public void IsZoneFullySpawned_UninitializedSpawner_ReturnsFalse()
        {
            foreach (MouthZone zone in System.Enum.GetValues(typeof(MouthZone)))
                Assert.IsFalse(_spawner.IsZoneFullySpawned(zone));
        }

        [Test]
        public void IsZoneComplete_UninitializedSpawner_ReturnsFalse()
        {
            foreach (MouthZone zone in System.Enum.GetValues(typeof(MouthZone)))
                Assert.IsFalse(_spawner.IsZoneComplete(zone));
        }
    }
}

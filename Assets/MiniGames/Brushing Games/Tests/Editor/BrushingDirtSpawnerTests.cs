using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-13: Spawn con límite máximo
    [TestFixture]
    public class BrushingDirtSpawnerTests
    {
        GameObject _go;
        BushingDirtSpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("BushingDirtSpawner");
            _spawner = _go.AddComponent<BushingDirtSpawner>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-13a: maxDirt por defecto es 5
        [Test]
        public void MaxDirt_PorDefecto_Es5()
        {
            Assert.AreEqual(5, _spawner.maxDirt);
        }

        // UNIT-BRUSH-13b: spawnInterval por defecto es 1.5 segundos
        [Test]
        public void SpawnInterval_PorDefecto_Es1p5()
        {
            Assert.AreEqual(1.5f, _spawner.spawnInterval, 0.001f);
        }

        // UNIT-BRUSH-13c: StartSpawning activa el spawner
        [Test]
        public void StartSpawning_IsActive_EsTrue()
        {
            _spawner.StartSpawning();

            var field = typeof(BushingDirtSpawner)
                .GetField("isActive", BindingFlags.NonPublic | BindingFlags.Instance);

            bool isActive = (bool)field.GetValue(_spawner);

            Assert.IsTrue(isActive, "El spawner debe estar activo tras StartSpawning()");
        }

        // UNIT-BRUSH-13d: StopSpawning desactiva el spawner
        [Test]
        public void StopSpawning_IsActive_EsFalse()
        {
            _spawner.StartSpawning();
            _spawner.StopSpawning();

            var field = typeof(BushingDirtSpawner)
                .GetField("isActive", BindingFlags.NonPublic | BindingFlags.Instance);

            bool isActive = (bool)field.GetValue(_spawner);

            Assert.IsFalse(isActive, "El spawner debe estar inactivo tras StopSpawning()");
        }
    }
}

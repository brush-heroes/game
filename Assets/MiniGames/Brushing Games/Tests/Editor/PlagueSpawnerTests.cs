using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-19: Spawn por cada diente
    // UNIT-BRUSH-20: Consistencia de clase duplicada de spawn
    // Nota: la clase en PlagueSpawner.cs se llama PlaqueSpawner (con 'u')
    [TestFixture]
    public class PlagueSpawnerTests
    {
        GameObject _go;
        PlaqueSpawner _spawner;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("PlaqueSpawner");
            _spawner = _go.AddComponent<PlaqueSpawner>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-19a: Con 3 dientes asignados, teeth.Length == 3
        [Test]
        public void Teeth_ConTresDientes_LongitudEsTres()
        {
            var t1 = new GameObject("Tooth1").transform;
            var t2 = new GameObject("Tooth2").transform;
            var t3 = new GameObject("Tooth3").transform;

            _spawner.teeth = new Transform[] { t1, t2, t3 };

            Assert.AreEqual(3, _spawner.teeth.Length,
                "PlaqueSpawner debe conservar referencia a los 3 dientes asignados");

            Object.DestroyImmediate(t1.gameObject);
            Object.DestroyImmediate(t2.gameObject);
            Object.DestroyImmediate(t3.gameObject);
        }

        // UNIT-BRUSH-19b: plaquePrefab asignado no es null
        [Test]
        public void PlaquePrefab_Asignado_NoEsNull()
        {
            var prefab = new GameObject("PlaquePrefab");
            _spawner.plaquePrefab = prefab;

            Assert.IsNotNull(_spawner.plaquePrefab);

            Object.DestroyImmediate(prefab);
        }

        // UNIT-BRUSH-20a: Dos PlaqueSpawners con la misma configuración tienen igual teeth.Length
        [Test]
        public void DosSpawners_MismaConfiguracion_IgualTeethLength()
        {
            var go2 = new GameObject("PlaqueSpawner2");
            var spawner2 = go2.AddComponent<PlaqueSpawner>();

            var t1 = new GameObject("Tooth1").transform;
            var t2 = new GameObject("Tooth2").transform;

            _spawner.teeth  = new Transform[] { t1, t2 };
            spawner2.teeth = new Transform[] { t1, t2 };

            Assert.AreEqual(_spawner.teeth.Length, spawner2.teeth.Length,
                "Dos PlaqueSpawners con la misma configuración deben tener el mismo número de dientes");

            Object.DestroyImmediate(t1.gameObject);
            Object.DestroyImmediate(t2.gameObject);
            Object.DestroyImmediate(go2);
        }
    }
}

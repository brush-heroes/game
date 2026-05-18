using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.DentalFlossGame.Tests
{
    [TestFixture]
    public class GameManagerTests
    {
        GameObject _go;
        GameManager _mgr;

        static readonly MethodInfo _awake =
            typeof(GameManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            GameManager.Instance = null;
            _go  = new GameObject("GameManager");
            _mgr = _go.AddComponent<GameManager>();
            _awake.Invoke(_mgr, null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            GameManager.Instance = null;
        }

        // UNIT-FLOSS-01a
        [Test]
        public void Singleton_UnaInstancia_NoEsNull()
        {
            Assert.IsNotNull(GameManager.Instance);
        }

        // UNIT-FLOSS-01b: sin invocar Awake en el segundo GO, Instance no cambia
        [Test]
        public void Singleton_SegundoGameObject_NoSobreescribeInstancia()
        {
            var go2 = new GameObject("GameManager2");
            go2.AddComponent<GameManager>(); // Awake NO se invoca → Instance no cambia

            Assert.AreEqual(_mgr, GameManager.Instance,
                "Instance debe mantenerse al agregar un segundo GameManager sin inicializar");

            Object.DestroyImmediate(go2);
        }

        // UNIT-FLOSS-01c
        [Test] public void IsGameActive_AlInstanciar_EsTrue() => Assert.IsTrue(_mgr.isGameActive);

        // UNIT-FLOSS-02a
        [Test]
        public void AddScore_JuegoActivo_SumaPuntos()
        {
            _mgr.isGameActive = true; _mgr.totalScore = 0;
            _mgr.AddScore(10);
            Assert.AreEqual(10, _mgr.totalScore);
        }

        // UNIT-FLOSS-02b
        [Test]
        public void AddScore_JuegoInactivo_NoSumaPuntos()
        {
            _mgr.isGameActive = false; _mgr.totalScore = 0;
            _mgr.AddScore(10);
            Assert.AreEqual(0, _mgr.totalScore);
        }

        // UNIT-FLOSS-02c
        [Test]
        public void AddScore_Multiplas_AcumulaCorrectamente()
        {
            _mgr.isGameActive = true; _mgr.totalScore = 0;
            _mgr.AddScore(5); _mgr.AddScore(15);
            Assert.AreEqual(20, _mgr.totalScore);
        }
    }
}

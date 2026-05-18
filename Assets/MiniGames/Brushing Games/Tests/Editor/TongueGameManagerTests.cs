using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    [TestFixture]
    public class TongueGameManagerTests
    {
        GameObject _go;
        TongueGameManager _mgr;

        static readonly MethodInfo _winTongueGame =
            typeof(TongueGameManager).GetMethod("WinTongueGame",
                BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            _go  = new GameObject("TongueGameManager");
            _mgr = _go.AddComponent<TongueGameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-BRUSH-14a
        [Test] public void DirtAmount_PorDefecto_Es8() => Assert.AreEqual(8, _mgr.dirtAmount);

        // UNIT-BRUSH-14b: Verificar TongueGameCompleted via WinTongueGame directamente
        // (evita Destroy inmediato de Edit Mode en RemoveDirt)
        [Test]
        public void RemoveDirt_UltimaUnidad_DisparaTongueGameCompleted()
        {
            bool completed = false;
            _mgr.TongueGameCompleted += () => completed = true;

            _winTongueGame.Invoke(_mgr, null);

            Assert.IsTrue(completed,
                "TongueGameCompleted debe dispararse al invocar WinTongueGame");
        }

        // UNIT-BRUSH-14c
        [Test]
        public void RemoveDirt_UltimaUnidad_DisparaTongueGameWon()
        {
            bool won = false;
            _mgr.TongueGameWon += () => won = true;

            _winTongueGame.Invoke(_mgr, null);

            Assert.IsTrue(won, "TongueGameWon debe dispararse al invocar WinTongueGame");
        }
    }
}

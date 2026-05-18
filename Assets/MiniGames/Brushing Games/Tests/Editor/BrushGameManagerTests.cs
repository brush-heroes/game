using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-01: Estado inicial y activación de grupos
    // UNIT-BRUSH-02: Progresión por AddClean y eventos
    [TestFixture]
    public class BrushGameManagerTests
    {
        GameObject _go;
        BrushGameManager _mgr;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("BrushGameManager");
            _mgr = _go.AddComponent<BrushGameManager>();
            _mgr.autoStart = false;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            BrushGameManager.Instance = null;
        }

        // UNIT-BRUSH-01a: Después de StartFromChewingRight, currentType es Chewing
        [Test]
        public void StartFromChewingRight_CurrentType_EsChewing()
        {
            _mgr.StartFromChewingRight();

            Assert.AreEqual(ZoneType.Chewing, _mgr.currentType);
        }

        // UNIT-BRUSH-01b: Después de StartFromChewingRight, currentSide es Right
        [Test]
        public void StartFromChewingRight_CurrentSide_EsRight()
        {
            _mgr.StartFromChewingRight();

            Assert.AreEqual(ZoneSide.Right, _mgr.currentSide);
        }

        // UNIT-BRUSH-01c: cleaned empieza en 0 tras StartFromChewingRight
        [Test]
        public void StartFromChewingRight_Cleaned_EsCero()
        {
            _mgr.cleaned = 5;
            _mgr.StartFromChewingRight();

            Assert.AreEqual(0, _mgr.cleaned);
        }

        // UNIT-BRUSH-02a: AddClean acumula hasta target y dispara el siguiente paso
        [Test]
        public void AddClean_HastaTarget_CambiaFase()
        {
            _mgr.StartFromChewingRight();
            _mgr.target = 3;

            // 3 limpiezas en Chewing Right → pasa a Outside Right
            _mgr.AddClean();
            _mgr.AddClean();
            _mgr.AddClean();

            Assert.AreEqual(ZoneType.Outside, _mgr.currentType,
                "Tras 3 AddClean en Chewing Right debe pasar a Outside Right");
        }

        // UNIT-BRUSH-02b: BrushingCompleted se dispara al completar toda la secuencia
        [Test]
        public void AddClean_SecuenciaCompleta_DisparaBrushingCompleted()
        {
            _mgr.StartFromChewingRight();
            _mgr.target = 3;

            bool completed = false;
            _mgr.BrushingCompleted += () => completed = true;

            // Secuencia: Chewing R → Outside R → Inside R → Chewing L → Outside L → Inside L
            // 6 fases × 3 AddClean = 18 llamadas
            for (int i = 0; i < 18; i++)
                _mgr.AddClean();

            Assert.IsTrue(completed, "BrushingCompleted debe dispararse al terminar toda la secuencia");
        }

        // UNIT-BRUSH-02c: OutsideRightCompleted se dispara al completar Outside Right
        [Test]
        public void AddClean_OutsideRight_DisparaOutsideRightCompleted()
        {
            _mgr.StartFromChewingRight();
            _mgr.target = 3;

            bool outsideRightFired = false;
            _mgr.OutsideRightCompleted += () => outsideRightFired = true;

            // Chewing Right (3) → Outside Right (3) → dispara OutsideRightCompleted
            for (int i = 0; i < 6; i++)
                _mgr.AddClean();

            Assert.IsTrue(outsideRightFired, "OutsideRightCompleted debe dispararse al completar Outside Right");
        }
    }
}

using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.Menu.Tests
{
    // UNIT-MENU-07: Cambio de estado por racha
    [TestFixture]
    public class MascotControllerTests
    {
        GameObject _go;
        MascotController _mascot;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("MascotController");
            _mascot = _go.AddComponent<MascotController>();
            // Sin SpriteAnimator: SetState actualiza _state pero no ejecuta animaciones
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        MascotController.MascotState GetState()
        {
            return (MascotController.MascotState)typeof(MascotController)
                .GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_mascot);
        }

        // UNIT-MENU-07a: Racha 0 → estado Sad
        [Test]
        public void RefreshState_Racha0_EstadoSad()
        {
            _mascot.RefreshState(0);

            Assert.AreEqual(MascotController.MascotState.Sad, GetState());
        }

        // UNIT-MENU-07b: Racha 3 → estado Happy
        [Test]
        public void RefreshState_Racha3_EstadoHappy()
        {
            _mascot.RefreshState(3);

            Assert.AreEqual(MascotController.MascotState.Happy, GetState());
        }

        // UNIT-MENU-07c: Racha 7 → estado Cheering
        [Test]
        public void RefreshState_Racha7_EstadoCheering()
        {
            _mascot.RefreshState(7);

            Assert.AreEqual(MascotController.MascotState.Cheering, GetState());
        }

        // UNIT-MENU-07d: Racha 1 → estado Neutral
        [Test]
        public void RefreshState_Racha1_EstadoNeutral()
        {
            _mascot.RefreshState(1);

            Assert.AreEqual(MascotController.MascotState.Neutral, GetState());
        }

        // UNIT-MENU-07e: RefreshState con mismo estado no cambia (idempotente)
        [Test]
        public void RefreshState_MismoEstado_NoModificaEstado()
        {
            _mascot.RefreshState(7); // Cheering
            _mascot.RefreshState(7); // Cheering de nuevo

            Assert.AreEqual(MascotController.MascotState.Cheering, GetState());
        }
    }
}

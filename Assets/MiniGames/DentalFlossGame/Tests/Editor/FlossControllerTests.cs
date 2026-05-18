using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.DentalFlossGame.Tests
{
    // UNIT-FLOSS-03: Clamp de posición y límites por lado
    // UNIT-FLOSS-04: Activación de modo curva (estado inicial)
    // UNIT-FLOSS-05: Bloqueo por UI/DesactivarClicks
    [TestFixture]
    public class FlossControllerTests
    {
        GameObject _go;
        FlossController _ctrl;

        [SetUp]
        public void SetUp()
        {
            // GameManager necesario para que FlossController no lance NPE en Update
            var gmGo = new GameObject("GameManager");
            gmGo.AddComponent<GameManager>();

            _go = new GameObject("FlossController");
            _ctrl = _go.AddComponent<FlossController>();
        }

        [TearDown]
        public void TearDown()
        {
            // Destruir todos los GameObjects creados
            foreach (var gm in Object.FindObjectsOfType<GameManager>())
                Object.DestroyImmediate(gm.gameObject);

            Object.DestroyImmediate(_go);
            GameManager.Instance = null;
        }

        // UNIT-FLOSS-03a: Límites X del lado derecho configurados por defecto
        [Test]
        public void LimitesDerechos_Configurados_PorDefecto()
        {
            Assert.AreEqual(2.10f, _ctrl.rightMinX, 0.001f);
            Assert.AreEqual(4.40f, _ctrl.rightMaxX, 0.001f);
        }

        // UNIT-FLOSS-03b: Límites X del lado izquierdo configurados por defecto
        [Test]
        public void LimitesIzquierdos_Configurados_PorDefecto()
        {
            Assert.AreEqual(-4.40f, _ctrl.leftMinX, 0.001f);
            Assert.AreEqual(-2.10f, _ctrl.leftMaxX, 0.001f);
        }

        // UNIT-FLOSS-03c: Límites Y configurados por defecto
        [Test]
        public void LimitesY_Configurados_PorDefecto()
        {
            Assert.AreEqual(-0.33f, _ctrl.minY, 0.001f);
            Assert.AreEqual(1.66f,  _ctrl.maxY, 0.001f);
        }

        // UNIT-FLOSS-04: enModoCurva inicia en false (estado inicial antes de interacción)
        [Test]
        public void EnModoCurva_AlInstanciar_EsFalse()
        {
            var field = typeof(FlossController)
                .GetField("enModoCurva", BindingFlags.NonPublic | BindingFlags.Instance);

            bool enModoCurva = (bool)field.GetValue(_ctrl);

            Assert.IsFalse(enModoCurva,
                "enModoCurva debe ser false antes de cualquier interacción con input");
        }

        // UNIT-FLOSS-05: DesactivarClicks pone puedeSerClickeado en false
        [Test]
        public void DesactivarClicks_PonerPuedeSerClickeadoFalse()
        {
            _ctrl.DesactivarClicks();

            var field = typeof(FlossController)
                .GetField("puedeSerClickeado", BindingFlags.NonPublic | BindingFlags.Instance);

            bool puedeSerClickeado = (bool)field.GetValue(_ctrl);

            Assert.IsFalse(puedeSerClickeado,
                "Después de DesactivarClicks() el campo puedeSerClickeado debe ser false");
        }
    }
}

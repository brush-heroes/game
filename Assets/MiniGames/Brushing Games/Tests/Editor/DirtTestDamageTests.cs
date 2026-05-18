using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-21: Daño manual por teclado (Space → TakeDamage)
    // Nota: Input.GetKeyDown requiere Play Mode. Este test verifica asignación de targetDirt.
    [TestFixture]
    public class DirtTestDamageTests
    {
        GameObject _go;
        GameObject _dirtGo;
        DirtTestDamage _damage;
        DirtHealth _dirtHealth;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("DirtTestDamage");
            _damage = _go.AddComponent<DirtTestDamage>();

            _dirtGo = new GameObject("DirtHealth");
            _dirtHealth = _dirtGo.AddComponent<DirtHealth>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_dirtGo);
        }

        // UNIT-BRUSH-21a: targetDirt asignado no es null
        [Test]
        public void TargetDirt_Asignado_NoEsNull()
        {
            _damage.targetDirt = _dirtHealth;

            Assert.IsNotNull(_damage.targetDirt,
                "targetDirt debe referenciar el DirtHealth asignado");
        }

        // UNIT-BRUSH-21b: targetDirt es null por defecto
        [Test]
        public void TargetDirt_PorDefecto_EsNull()
        {
            Assert.IsNull(_damage.targetDirt,
                "targetDirt debe ser null antes de ser asignado");
        }

        // UNIT-BRUSH-21c: El componente es MonoBehaviour
        [Test]
        public void DirtTestDamage_Es_MonoBehaviour()
        {
            Assert.IsInstanceOf<MonoBehaviour>(_damage);
        }
    }
}

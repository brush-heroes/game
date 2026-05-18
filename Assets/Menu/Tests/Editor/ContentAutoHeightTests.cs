using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.Menu.Tests
{
    // UNIT-MENU-09: Autoajuste de contenido según hijos y padding
    [TestFixture]
    public class ContentAutoHeightTests
    {
        GameObject _go;
        ContentAutoHeight _comp;
        RectTransform _rt;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("ContentAutoHeight");
            _comp = _go.AddComponent<ContentAutoHeight>();
            // RequireComponent(RectTransform) lo agrega automáticamente
            _rt = _go.GetComponent<RectTransform>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-MENU-09a: Sin hijos, sizeDelta.y es solo el padding (60)
        [Test]
        public void Recalculate_SinHijos_SizeDeltaEsPadding()
        {
            _comp.Recalculate();

            Assert.AreEqual(60f, _rt.sizeDelta.y, 0.01f,
                "Sin hijos, la altura debe ser solo el bottomPadding (60)");
        }

        // UNIT-MENU-09b: Con un hijo a Y=-100 y altura=50, sizeDelta.y = 150 + 60 = 210
        [Test]
        public void Recalculate_ConUnHijo_AjustaAltura()
        {
            var childGo = new GameObject("Child");
            childGo.transform.SetParent(_go.transform);
            var childRt = childGo.AddComponent<RectTransform>();
            childRt.anchoredPosition = new Vector2(0f, -100f);
            childRt.sizeDelta = new Vector2(100f, 50f);

            _comp.Recalculate();

            // extent = |anchoredPosition.y| + rect.height = 100 + 50 = 150
            // sizeDelta.y = 150 + 60 (padding) = 210
            Assert.AreEqual(210f, _rt.sizeDelta.y, 0.01f,
                "La altura debe ser |anchoredPosition.y| + rect.height + bottomPadding");

            Object.DestroyImmediate(childGo);
        }

        // UNIT-MENU-09c: Con dos hijos, toma el extent máximo
        [Test]
        public void Recalculate_ConDosHijos_TomaExtentMaximo()
        {
            var child1Go = new GameObject("Child1");
            child1Go.transform.SetParent(_go.transform);
            var child1Rt = child1Go.AddComponent<RectTransform>();
            child1Rt.anchoredPosition = new Vector2(0f, -50f);
            child1Rt.sizeDelta = new Vector2(100f, 30f);
            // extent1 = 50 + 30 = 80

            var child2Go = new GameObject("Child2");
            child2Go.transform.SetParent(_go.transform);
            var child2Rt = child2Go.AddComponent<RectTransform>();
            child2Rt.anchoredPosition = new Vector2(0f, -120f);
            child2Rt.sizeDelta = new Vector2(100f, 40f);
            // extent2 = 120 + 40 = 160 ← máximo

            _comp.Recalculate();

            // sizeDelta.y = 160 + 60 = 220
            Assert.AreEqual(220f, _rt.sizeDelta.y, 0.01f,
                "Debe usar el extent máximo entre todos los hijos");

            Object.DestroyImmediate(child1Go);
            Object.DestroyImmediate(child2Go);
        }
    }
}

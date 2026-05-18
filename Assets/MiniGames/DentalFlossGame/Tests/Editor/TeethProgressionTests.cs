using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace BrushHeroes.DentalFlossGame.Tests
{
    // UNIT-FLOSS-06: Umbrales de sprite por score
    // UNIT-FLOSS-07: FlipDientes
    [TestFixture]
    public class TeethProgressionTests
    {
        GameObject _gmGo;
        GameObject _go;
        TeethProgression _teeth;

        static readonly MethodInfo _gmAwake =
            typeof(GameManager).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo _teethStart =
            typeof(TeethProgression).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo _teethUpdate =
            typeof(TeethProgression).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo _currentSpriteField =
            typeof(TeethProgression).GetField("currentActiveSprite", BindingFlags.NonPublic | BindingFlags.Instance);

        [SetUp]
        public void SetUp()
        {
            GameManager.Instance = null;
            _gmGo = new GameObject("GameManager");
            var gm = _gmGo.AddComponent<GameManager>();
            _gmAwake.Invoke(gm, null);

            _go = new GameObject("TeethProgression");
            _teeth = _go.AddComponent<TeethProgression>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
            Object.DestroyImmediate(_gmGo);
            GameManager.Instance = null;
        }

        static Sprite CreateFakeSprite()
        {
            var tex = new Texture2D(2, 2);
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
        }

        // UNIT-FLOSS-06a: Umbral mediumLimit es 500 por defecto
        [Test]
        public void MediumLimit_PorDefecto_Es500()
        {
            Assert.AreEqual(500, _teeth.mediumLimit);
        }

        // UNIT-FLOSS-06b: Umbral cleanLimit es 1000 por defecto
        [Test]
        public void CleanLimit_PorDefecto_Es1000()
        {
            Assert.AreEqual(1000, _teeth.cleanLimit);
        }

        // UNIT-FLOSS-06c: Con score < 500 usa dirtyTeeth
        [Test]
        public void SpriteActivo_ScoreBajo_EsDirty()
        {
            var dirty  = CreateFakeSprite();
            var medium = CreateFakeSprite();
            var clean  = CreateFakeSprite();

            _teeth.dirtyTeeth  = dirty;
            _teeth.mediumTeeth = medium;
            _teeth.cleanTeeth  = clean;
            _teeth.teethRenderers = new List<SpriteRenderer>();

            GameManager.Instance.totalScore = 0;
            _teethStart.Invoke(_teeth, null);

            var currentSprite = (Sprite)_currentSpriteField.GetValue(_teeth);

            Assert.AreEqual(dirty, currentSprite);
        }

        // UNIT-FLOSS-06d: Con score >= 500 y < 1000 usa mediumTeeth (via Update)
        [Test]
        public void SpriteActivo_ScoreMedio_EsMedium()
        {
            var dirty  = CreateFakeSprite();
            var medium = CreateFakeSprite();
            var clean  = CreateFakeSprite();

            _teeth.dirtyTeeth  = dirty;
            _teeth.mediumTeeth = medium;
            _teeth.cleanTeeth  = clean;
            _teeth.teethRenderers = new List<SpriteRenderer>();

            GameManager.Instance.totalScore = 0;
            _teethStart.Invoke(_teeth, null);

            GameManager.Instance.totalScore = 600;
            _teethUpdate.Invoke(_teeth, null);

            var currentSprite = (Sprite)_currentSpriteField.GetValue(_teeth);

            Assert.AreEqual(medium, currentSprite);
        }

        // UNIT-FLOSS-07a: FlipDientes(true) → scale.x positivo en todos los renderers
        [Test]
        public void FlipDientes_MirandoDerecha_ScaleXPositivo()
        {
            var childGo = new GameObject("Tooth");
            childGo.transform.SetParent(_go.transform);
            var sr = childGo.AddComponent<SpriteRenderer>();
            childGo.transform.localScale = new Vector3(-1f, 1f, 1f);

            _teeth.teethRenderers = new List<SpriteRenderer> { sr };

            _teeth.FlipDientes(true);

            Assert.Greater(sr.transform.localScale.x, 0f,
                "Al mirar a la derecha, scale.x debe ser positivo");

            Object.DestroyImmediate(childGo);
        }

        // UNIT-FLOSS-07b: FlipDientes(false) → scale.x negativo en todos los renderers
        [Test]
        public void FlipDientes_MirandoIzquierda_ScaleXNegativo()
        {
            var childGo = new GameObject("Tooth");
            childGo.transform.SetParent(_go.transform);
            var sr = childGo.AddComponent<SpriteRenderer>();
            childGo.transform.localScale = new Vector3(1f, 1f, 1f);

            _teeth.teethRenderers = new List<SpriteRenderer> { sr };

            _teeth.FlipDientes(false);

            Assert.Less(sr.transform.localScale.x, 0f,
                "Al mirar a la izquierda, scale.x debe ser negativo");

            Object.DestroyImmediate(childGo);
        }
    }
}

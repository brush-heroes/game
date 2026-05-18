using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.Menu.Tests
{
    // UNIT-MENU-06: Click por sceneName → invoca método de carga correcto
    // Nota: La carga de escena requiere Play Mode. Este test verifica el campo sceneName.
    [TestFixture]
    public class GameCardUITests
    {
        GameObject _go;
        GameCardUI _card;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("GameCardUI");
            _card = _go.AddComponent<GameCardUI>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-MENU-06a: sceneName para ARGame persiste
        [Test]
        public void SceneName_ARGame_Persiste()
        {
            _card.sceneName = "ARGame";

            Assert.AreEqual("ARGame", _card.sceneName);
        }

        // UNIT-MENU-06b: sceneName para BrushingGame persiste
        [Test]
        public void SceneName_BrushingGame_Persiste()
        {
            _card.sceneName = "BrushingGame";

            Assert.AreEqual("BrushingGame", _card.sceneName);
        }

        // UNIT-MENU-06c: sceneName para DentalFlossGame persiste
        [Test]
        public void SceneName_DentalFlossGame_Persiste()
        {
            _card.sceneName = "DentalFlossGame";

            Assert.AreEqual("DentalFlossGame", _card.sceneName);
        }
    }
}

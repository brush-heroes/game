using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.DentalFlossGame.Tests
{
    // UNIT-FLOSS-11: Reinicio manual de escena
    [TestFixture]
    public class RestartButtonTests
    {
        GameObject _go;
        RestartButton _btn;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("RestartButton");
            _btn = _go.AddComponent<RestartButton>();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            Object.DestroyImmediate(_go);
        }

        // UNIT-FLOSS-11a: Reiniciar() restablece Time.timeScale a 1
        [Test]
        public void Reiniciar_RestablecTimeScale_AUno()
        {
            Time.timeScale = 0f;

            // Reiniciar() pone Time.timeScale = 1 antes de LoadScene.
            // En Edit Mode, LoadScene puede fallar; capturamos la excepción
            // y solo verificamos el efecto sobre Time.timeScale.
            try { _btn.Reiniciar(); }
            catch { /* SceneManager.LoadScene no opera en Edit Mode - esperado */ }

            Assert.AreEqual(1f, Time.timeScale, 0.001f,
                "Time.timeScale debe ser 1 después de Reiniciar(), independientemente del estado de la escena");
        }
    }
}

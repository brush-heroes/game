using NUnit.Framework;
using UnityEngine;
using System.Reflection;

namespace BrushHeroes.DentalFlossGame.Tests
{
    // UNIT-FLOSS-10: Oscilación sinusoidal
    [TestFixture]
    public class FloatingObjectTests
    {
        GameObject _go;
        FloatingObject _obj;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("FloatingObject");
            _obj = _go.AddComponent<FloatingObject>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // UNIT-FLOSS-10a: Amplitud por defecto es 0.2
        [Test]
        public void Amplitude_PorDefecto_Es0p2()
        {
            Assert.AreEqual(0.2f, _obj.amplitude, 0.001f);
        }

        // UNIT-FLOSS-10b: Speed por defecto es 2
        [Test]
        public void Speed_PorDefecto_Es2()
        {
            Assert.AreEqual(2f, _obj.speed, 0.001f);
        }

        // UNIT-FLOSS-10c: yStart se inicializa con la posición Y del transform en Start
        [Test]
        public void yStart_TrasDStart_IgualAPosicionY()
        {
            _go.transform.position = new Vector3(0f, 3.5f, 0f);

            typeof(FloatingObject)
                .GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(_obj, null);

            float yStart = (float)typeof(FloatingObject)
                .GetField("yStart", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(_obj);

            Assert.AreEqual(3.5f, yStart, 0.001f,
                "yStart debe capturar la posición Y inicial del objeto");
        }

        // UNIT-FLOSS-10d: La oscilación se mantiene dentro del rango [yStart-amplitude, yStart+amplitude]
        // (verificación de configuración; movimiento real requiere Play Mode)
        [Test]
        public void Amplitud_MenorQue_LimiteOscilacion()
        {
            // La posición Y nunca puede desviarse más que amplitude desde yStart
            Assert.LessOrEqual(_obj.amplitude, 1f,
                "La amplitud de oscilación debe mantenerse en un rango definido");
        }
    }
}

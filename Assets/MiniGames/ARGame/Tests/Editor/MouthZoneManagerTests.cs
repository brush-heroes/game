using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.ARGame.Tests
{
    /// <summary>
    /// Tests para MouthZoneManager.GetTransformForZone — verifica que cada
    /// MouthZone se mapee al Transform configurado correspondiente.
    /// </summary>
    public class MouthZoneManagerTests
    {
        GameObject _root;
        MouthZoneManager _manager;
        Transform _ul, _ur, _fu, _ll, _lr, _fl;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("MouthZoneManagerTestRoot");
            _manager = _root.AddComponent<MouthZoneManager>();

            _ul = MakeChild("UL");
            _ur = MakeChild("UR");
            _fu = MakeChild("FU");
            _ll = MakeChild("LL");
            _lr = MakeChild("LR");
            _fl = MakeChild("FL");

            // Inyección de los Transform privados serializados vía reflexión:
            // los campos se exponen al Inspector con [SerializeField] private,
            // así que en pruebas necesitamos asignarlos por reflexión.
            SetField("upperLeft",  _ul);
            SetField("upperRight", _ur);
            SetField("frontUpper", _fu);
            SetField("lowerLeft",  _ll);
            SetField("lowerRight", _lr);
            SetField("frontLower", _fl);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
        }

        Transform MakeChild(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root.transform);
            return go.transform;
        }

        void SetField(string name, Transform value)
        {
            FieldInfo f = typeof(MouthZoneManager).GetField(
                name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(f, $"Campo privado '{name}' no encontrado en MouthZoneManager.");
            f.SetValue(_manager, value);
        }

        [Test]
        public void GetTransformForZone_UpperLeft_ReturnsUpperLeftTransform()
        {
            Assert.AreSame(_ul, _manager.GetTransformForZone(MouthZone.UpperLeft));
        }

        [Test]
        public void GetTransformForZone_UpperRight_ReturnsUpperRightTransform()
        {
            Assert.AreSame(_ur, _manager.GetTransformForZone(MouthZone.UpperRight));
        }

        [Test]
        public void GetTransformForZone_FrontUpper_ReturnsFrontUpperTransform()
        {
            Assert.AreSame(_fu, _manager.GetTransformForZone(MouthZone.FrontUpper));
        }

        [Test]
        public void GetTransformForZone_LowerLeft_ReturnsLowerLeftTransform()
        {
            Assert.AreSame(_ll, _manager.GetTransformForZone(MouthZone.LowerLeft));
        }

        [Test]
        public void GetTransformForZone_LowerRight_ReturnsLowerRightTransform()
        {
            Assert.AreSame(_lr, _manager.GetTransformForZone(MouthZone.LowerRight));
        }

        [Test]
        public void GetTransformForZone_FrontLower_ReturnsFrontLowerTransform()
        {
            Assert.AreSame(_fl, _manager.GetTransformForZone(MouthZone.FrontLower));
        }

        [Test]
        public void GetTransformForZone_AllSixZones_ReturnDistinctTransforms()
        {
            var seen = new System.Collections.Generic.HashSet<Transform>();
            foreach (MouthZone zone in System.Enum.GetValues(typeof(MouthZone)))
            {
                Transform t = _manager.GetTransformForZone(zone);
                Assert.IsNotNull(t, $"Zona {zone} devolvió null.");
                Assert.IsTrue(seen.Add(t), $"Zona {zone} comparte Transform con otra zona.");
            }
            Assert.AreEqual(6, seen.Count);
        }

        [Test]
        public void GetZoneTransform_IsAliasOfGetTransformForZone()
        {
            foreach (MouthZone zone in System.Enum.GetValues(typeof(MouthZone)))
            {
                Assert.AreSame(
                    _manager.GetTransformForZone(zone),
                    _manager.GetZoneTransform(zone),
                    $"Mismatch entre los dos métodos para la zona {zone}.");
            }
        }
    }
}

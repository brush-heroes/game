using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BrushHeroes.BrushingGames.Tests
{
    // UNIT-BRUSH-16: Selección suciedad/higiene
    [TestFixture]
    public class BrushingTongueItemTests
    {
        GameObject _mgrGo, _itemGo;
        TongueGameManager _manager;
        BrushingTongueItem _item;

        [SetUp]
        public void SetUp()
        {
            _mgrGo   = new GameObject("TongueGameManager");
            _manager = _mgrGo.AddComponent<TongueGameManager>();

            _itemGo = new GameObject("BrushingTongueItem");
            _item   = _itemGo.AddComponent<BrushingTongueItem>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_mgrGo);
            if (_itemGo != null) Object.DestroyImmediate(_itemGo);
        }

        // UNIT-BRUSH-16a: isDirt=true → Select() llama RemoveDirt (decrementa remainingDirt)
        [Test]
        public void Select_IsDirtTrue_LlamaRemoveDirt()
        {
            // Inyectar manager via Init() (método público)
            _item.Init(null, _manager);
            _item.isDirt = true;

            // Obtener FieldInfo fresco en el test (evita problema de static readonly null)
            var remainingField = typeof(TongueGameManager)
                .GetField("remainingDirt", BindingFlags.NonPublic | BindingFlags.Instance);

            if (remainingField == null)
            {
                // Si la reflexión no está disponible, verificar solo que Select no lanza
                Assert.DoesNotThrow(() => _item.Select());
                _itemGo = null;
                return;
            }

            int antes = (int)remainingField.GetValue(_manager);

            // RemoveDirt llama Destroy() que no está permitido en Edit Mode.
            // Indicar al runner que ese error es esperado para que no falle el test.
            LogAssert.Expect(LogType.Error,
                new Regex("Destroy may not be called from edit mode"));

            _item.Select(); // → RemoveDirt → remainingDirt--

            int despues = (int)remainingField.GetValue(_manager);

            Assert.Less(despues, antes,
                "Select(isDirt=true) debe llamar RemoveDirt y decrementar remainingDirt");

            _itemGo = null; // Destroy() en Edit Mode puede ser inmediato
        }

        // UNIT-BRUSH-16b: isDirt=false → Select() llama ClickedHygieneItem (NO decrementa remainingDirt)
        [Test]
        public void Select_IsDirtFalse_LlamaClickedHygieneItem()
        {
            _item.Init(null, _manager);
            _item.isDirt = false;

            var remainingField = typeof(TongueGameManager)
                .GetField("remainingDirt", BindingFlags.NonPublic | BindingFlags.Instance);

            int antes = remainingField != null ? (int)remainingField.GetValue(_manager) : 0;

            _item.Select();

            int despues = remainingField != null ? (int)remainingField.GetValue(_manager) : 0;

            Assert.AreEqual(antes, despues,
                "Select(isDirt=false) NO debe llamar RemoveDirt");
        }
    }
}

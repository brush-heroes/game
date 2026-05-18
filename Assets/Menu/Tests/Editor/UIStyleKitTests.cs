using NUnit.Framework;
using UnityEngine;

namespace BrushHeroes.Menu.Tests
{
    // UNIT-MENU-08: Generación de sprites utilitarios
    [TestFixture]
    public class UIStyleKitTests
    {
        // UNIT-MENU-08a: RoundedSprite devuelve un Sprite no nulo
        [Test]
        public void RoundedSprite_Radio16_NoEsNull()
        {
            var sprite = UIStyleKit.RoundedSprite(16);

            Assert.IsNotNull(sprite, "RoundedSprite debe devolver un Sprite válido");

            Object.DestroyImmediate(sprite.texture);
        }

        // UNIT-MENU-08b: RoundedSprite tiene dimensiones 128×128
        [Test]
        public void RoundedSprite_Dimensiones_Son128x128()
        {
            var sprite = UIStyleKit.RoundedSprite(16);

            Assert.AreEqual(128f, sprite.rect.width, 0.1f);
            Assert.AreEqual(128f, sprite.rect.height, 0.1f);

            Object.DestroyImmediate(sprite.texture);
        }

        // UNIT-MENU-08c: CircleSprite devuelve un Sprite no nulo
        [Test]
        public void CircleSprite_NoEsNull()
        {
            var sprite = UIStyleKit.CircleSprite();

            Assert.IsNotNull(sprite, "CircleSprite debe devolver un Sprite válido");

            Object.DestroyImmediate(sprite.texture);
        }

        // UNIT-MENU-08d: CircleSprite tiene dimensiones 64×64
        [Test]
        public void CircleSprite_Dimensiones_Son64x64()
        {
            var sprite = UIStyleKit.CircleSprite();

            Assert.AreEqual(64f, sprite.rect.width, 0.1f);
            Assert.AreEqual(64f, sprite.rect.height, 0.1f);

            Object.DestroyImmediate(sprite.texture);
        }

        // UNIT-MENU-08e: RoundedSprites con distintos radios generan texturas diferentes
        [Test]
        public void RoundedSprite_RadiosDistintos_TexturasDistintas()
        {
            var sprite16 = UIStyleKit.RoundedSprite(16);
            var sprite32 = UIStyleKit.RoundedSprite(32);

            Assert.AreNotSame(sprite16.texture, sprite32.texture,
                "Diferentes radios deben producir texturas diferentes");

            Object.DestroyImmediate(sprite16.texture);
            Object.DestroyImmediate(sprite32.texture);
        }
    }
}

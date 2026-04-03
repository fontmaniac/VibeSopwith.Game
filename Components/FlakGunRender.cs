using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class FlakGunRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _baseLRTexture = null!;
        private Texture2D _baseRLTexture = null!;
        private Texture2D _LRExplodedTexture = null!;
        private Texture2D _RLExplodedTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            var tex1 = Game.Content.Load<Texture2D>("Textures\\Flak_1_LR.png");
            _baseLRTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex1);
            var tex2 = Game.Content.Load<Texture2D>("Textures\\Flak_1_RL.png");
            _baseRLTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex2);
            var tex3 = Game.Content.Load<Texture2D>("Textures\\Flak_1_Exploded_LR.png");
            _LRExplodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex3);
            var tex4 = Game.Content.Load<Texture2D>("Textures\\Flak_1_Exploded_RL.png");
            _RLExplodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex4);
        }

        private DrawHelper.HandedTexture PickTexture(FlakGun gun) =>
            gun.Spin == BasisSpin.Down
                ? DrawHelper.HandedTexture.LR.Wrap(gun.Exploded ? _LRExplodedTexture : _baseLRTexture)
                : DrawHelper.HandedTexture.RL.Wrap(gun.Exploded ? _RLExplodedTexture : _baseRLTexture);

        public void Draw(FlakGun gun, GameTime gameTime)
        {
            DrawHelper.DrawOriginatedHanded(gun, PickTexture(gun), new Vector2(64, 128), TheGame.SpriteBatch, null);
        }

        public void DrawSnapped(FlakGun gun, GameTime gameTime, Vector2 worldPixelSize)
        {
            var texture = gun.Spin == BasisSpin.Down ? DrawHelper.HandedTexture.LR.Wrap(_baseLRTexture) : DrawHelper.HandedTexture.RL.Wrap(_baseRLTexture);
            DrawHelper.DrawOriginatedHanded(gun, PickTexture(gun), new Vector2(64, 128), TheGame.SpriteBatch, worldPixelSize);
        }

    }
}

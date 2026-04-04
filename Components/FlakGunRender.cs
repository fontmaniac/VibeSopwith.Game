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
        private Texture2D _barrelLRTexture = null!;
        private Texture2D _barrelRLTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _baseLRTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Flak_1_LR.png"));
            _baseRLTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Flak_1_RL.png"));
            _LRExplodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Flak_1_Exploded_LR.png"));
            _RLExplodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Flak_1_Exploded_RL.png"));
            _barrelLRTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\FlakBarrel_1_LR.png"));
            _barrelRLTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\FlakBarrel_1_RL.png"));
        }

        private DrawHelper.HandedTexture PickBaseTexture(FlakGun gun) =>
            gun.Spin == BasisSpin.Down
                ? DrawHelper.HandedTexture.LR.Wrap(gun.Exploded ? _LRExplodedTexture : _baseLRTexture)
                : DrawHelper.HandedTexture.RL.Wrap(gun.Exploded ? _RLExplodedTexture : _baseRLTexture);

        private DrawHelper.HandedTexture PickBarrelTexture(FlakGun gun) =>
            gun.Spin == BasisSpin.Down
                ? DrawHelper.HandedTexture.LR.Wrap(_barrelLRTexture)
                : DrawHelper.HandedTexture.RL.Wrap(_barrelRLTexture);

        public void Draw(FlakGun gun, GameTime gameTime)
        {
            if (!gun.Exploded)
                DrawHelper.DrawOriginatedHanded(gun.Barrel, PickBarrelTexture(gun), new Vector2(128, 128), TheGame.SpriteBatch, null);
            DrawHelper.DrawOriginatedHanded(gun, PickBaseTexture(gun), new Vector2(64, 128), TheGame.SpriteBatch, null);
        }

        public void DrawSnapped(FlakGun gun, GameTime gameTime, Vector2 worldPixelSize)
        {
            if (!gun.Exploded)
                DrawHelper.DrawOriginatedHanded(gun.Barrel, PickBarrelTexture(gun), new Vector2(128, 128), TheGame.SpriteBatch, worldPixelSize);
            DrawHelper.DrawOriginatedHanded(gun, PickBaseTexture(gun), new Vector2(64, 128), TheGame.SpriteBatch, worldPixelSize);
        }

    }
}

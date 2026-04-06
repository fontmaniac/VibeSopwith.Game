using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class FlakGunRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private TextureAtlas _baseLRTexture = null!;
        private TextureAtlas _baseRLTexture = null!;
        private TextureAtlas _LRExplodedTexture = null!;
        private TextureAtlas _RLExplodedTexture = null!;
        private TextureAtlas _barrelLRTexture = null!;
        private TextureAtlas _barrelRLTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _baseLRTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Flak_1_LR.png")).ToAtlas(new Vector2(64, 128));
            _baseRLTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Flak_1_RL.png")).ToAtlas(new Vector2(64, 128));
            _LRExplodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Flak_1_Exploded_LR.png")).ToAtlas(new Vector2(64, 128));
            _RLExplodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Flak_1_Exploded_RL.png")).ToAtlas(new Vector2(64, 128));
            _barrelLRTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\FlakBarrel_1_LR.png")).ToAtlas(new Vector2(128, 120));
            _barrelRLTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\FlakBarrel_1_RL.png")).ToAtlas(new Vector2(128, 120));
        }

        private HandedSlice PickBaseTexture(FlakGun gun) =>
            gun.Spin == BasisSpin.Down
                ? HandedSlice.LR.Wrap(gun.Exploded ? _LRExplodedTexture.GetSlice() : _baseLRTexture.GetSlice())
                : HandedSlice.RL.Wrap(gun.Exploded ? _RLExplodedTexture.GetSlice() : _baseRLTexture.GetSlice());

        private HandedSlice PickBarrelTexture(FlakGun gun) =>
            gun.Spin == BasisSpin.Down
                ? HandedSlice.LR.Wrap(_barrelLRTexture.GetSlice())
                : HandedSlice.RL.Wrap(_barrelRLTexture.GetSlice());

        public void Draw(FlakGun gun, GameTime gameTime)
        {
            if (!gun.Exploded)
                DrawHelper.DrawOriginatedHanded(gun.Barrel, PickBarrelTexture(gun), TheGame.SpriteBatch, null);
            DrawHelper.DrawOriginatedHanded(gun, PickBaseTexture(gun), TheGame.SpriteBatch, null);
        }

        public void DrawSnapped(FlakGun gun, GameTime gameTime, Vector2 worldPixelSize)
        {
            if (!gun.Exploded)
                DrawHelper.DrawOriginatedHanded(gun.Barrel, PickBarrelTexture(gun), TheGame.SpriteBatch, worldPixelSize);
            DrawHelper.DrawOriginatedHanded(gun, PickBaseTexture(gun), TheGame.SpriteBatch, worldPixelSize);
        }

    }
}

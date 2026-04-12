using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nage.Strata.Abstractions;
using Nage.Strata.Graphics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class FlakGunRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _baseLRTexture = null!;
        private HandedSlice.RL _baseRLTexture = null!;
        private HandedSlice.LR _LRExplodedTexture = null!;
        private HandedSlice.RL _RLExplodedTexture = null!;
        private HandedSlice.LR _barrelLRTexture = null!;
        private HandedSlice.RL _barrelRLTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _baseLRTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\Flak_1_LR.png")).ToAtlas(new Vector2(64, 128)).ToLRSlice();
            _baseRLTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\Flak_1_RL.png")).ToAtlas(new Vector2(64, 128)).ToRLSlice();
            _LRExplodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\Flak_1_Exploded_LR.png")).ToAtlas(new Vector2(64, 128)).ToLRSlice();
            _RLExplodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\Flak_1_Exploded_RL.png")).ToAtlas(new Vector2(64, 128)).ToRLSlice();
            _barrelLRTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\FlakBarrel_1_LR.png")).ToAtlas(new Vector2(128, 120)).ToLRSlice();
            _barrelRLTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\FlakBarrel_1_RL.png")).ToAtlas(new Vector2(128, 120)).ToRLSlice();
        }

        private HandedSlice PickBaseTexture(FlakGun gun) =>
            gun.Spin == BasisSpin.Down
                ? (gun.Exploded ? _LRExplodedTexture : _baseLRTexture)
                : (gun.Exploded ? _RLExplodedTexture : _baseRLTexture);

        private HandedSlice PickBarrelTexture(FlakGun gun) =>
            gun.Spin == BasisSpin.Down
                ? _barrelLRTexture
                : _barrelRLTexture;

        public void Draw(FlakGun gun, GameTime gameTime)
        {
            if (!gun.Exploded)
                DrawHelper.DrawSlice(gun.Barrel, PickBarrelTexture(gun), TheGame.SpriteBatchPoint, null);
            DrawHelper.DrawSlice(gun, PickBaseTexture(gun), TheGame.SpriteBatchPoint, null);
        }

        public void DrawSnapped(FlakGun gun, GameTime gameTime, Vector2 worldPixelSize)
        {
            if (!gun.Exploded)
                DrawHelper.DrawSlice(gun.Barrel, PickBarrelTexture(gun), TheGame.SpriteBatchPoint, worldPixelSize);
            DrawHelper.DrawSlice(gun, PickBaseTexture(gun), TheGame.SpriteBatchPoint, worldPixelSize);
        }

    }
}

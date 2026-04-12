using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nage.Strata.Graphics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class FountainRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _baseTexture = null!;
        private HandedSlice.LR _explodedTexture = null!;
        private HandedSlice.LR _nozzleTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _baseTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\FountainBase_1.png")).ToAtlas(new Vector2(64, 128)).ToLRSlice();
            _explodedTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\FountainBase_Exploded_1.png")).ToAtlas(new Vector2(64, 128)).ToLRSlice();
            _nozzleTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\FountainNozzle_1.png")).ToAtlas(new Vector2(6, 64)).ToLRSlice();
        }

        private HandedSlice.LR PickBaseTexture(Fountain f) =>
            f.Exploded ? _explodedTexture : _baseTexture;
                

        public void Draw(Fountain fountain, GameTime gameTime)
        {
            if (!fountain.Exploded)
                DrawHelper.DrawSlice(fountain.Nozzle, _nozzleTexture, TheGame.SpriteBatchPoint, null);
            DrawHelper.DrawSlice(fountain, PickBaseTexture(fountain), TheGame.SpriteBatchPoint, null);
        }

        public void DrawSnapped(Fountain fountain, GameTime gameTime, Vector2 worldPixelSize)
        {
            if (!fountain.Exploded)
                DrawHelper.DrawSlice(fountain.Nozzle, _nozzleTexture, TheGame.SpriteBatchPoint, worldPixelSize);
            DrawHelper.DrawSlice(fountain, PickBaseTexture(fountain), TheGame.SpriteBatchPoint, worldPixelSize);
        }

    }
}

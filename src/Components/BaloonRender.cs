using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nage.Strata.Graphics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class BaloonRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _baloonTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _baloonTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\Hot_Air_Balloon_1.png")).ToAtlas(Atlas.OriginCentered).ToLRSlice(); ;
        }

        public void Draw(Baloon baloon, GameTime gameTime, Vector2? worldPixelSize = null)
        {
            DrawHelper.DrawSlice(baloon, _baloonTexture, TheGame.SpriteBatchPoint, worldPixelSize);
        }
    }
}

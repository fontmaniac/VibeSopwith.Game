using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class BaloonRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _baloonTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _baloonTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Hot_Air_Balloon_1.png")).ToAtlas(Atlas.OriginCentered).ToLRSlice(); ;
        }

        public void Draw(Baloon baloon, GameTime gameTime, Vector2? worldPixelSize = null)
        {
            DrawHelper.DrawSlice(baloon, _baloonTexture, TheGame.SpriteBatch, worldPixelSize);
        }
    }
}

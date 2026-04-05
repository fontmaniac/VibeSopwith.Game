using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class BaloonRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _baloonTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            var tex1 = Game.Content.Load<Texture2D>("Textures\\Hot_Air_Balloon_1.png");
            _baloonTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex1);
        }

        public void Draw(Baloon baloon, GameTime gameTime)
        {
            DrawHelper.DrawCentered(baloon, _baloonTexture, TheGame.SpriteBatch);
        }

        public void DrawSnapped(Baloon baloon, GameTime gameTime, Vector2 worldPixelSize)
        {
            DrawHelper.DrawCentered(baloon, _baloonTexture, TheGame.SpriteBatch, worldPixelSize);
        }

    }
}

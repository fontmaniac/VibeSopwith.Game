using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class BombRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _bombTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            using var tex1 = Game.Content.Load<Texture2D>("Textures\\Bomb_1.png");
            _bombTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex1);

        }

        public void Draw(Bomb bomb, GameTime gameTime)
        {
            DrawHelper.DrawCentered(bomb, _bombTexture, TheGame.SpriteBatch);
        }
    }
}

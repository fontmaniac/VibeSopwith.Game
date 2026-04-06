using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class AirplaneRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private TextureAtlas _airplaneTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _airplaneTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Plane_1_Q.png")).ToAtlas();
        }

        public void Draw(Airplane airplane, GameTime gameTime)
        {
            DrawHelper.DrawOriginatedHanded(airplane, HandedSlice.LR.Wrap(_airplaneTexture.GetSlice()), new Vector2(115, 100), TheGame.SpriteBatch, null);
        }

        public void DrawSnapped(Airplane airplane, GameTime gameTime, Vector2 worldPixelSize)
        {
            DrawHelper.DrawOriginatedHanded(airplane, HandedSlice.LR.Wrap(_airplaneTexture.GetSlice()), new Vector2(115, 100), TheGame.SpriteBatch, worldPixelSize);
        }

    }
}

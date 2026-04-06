using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class AirplaneRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _airplaneTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _airplaneTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Plane_1_Q.png")).ToAtlas(new Vector2(115, 100)).ToLRSlice();
        }

        public void Draw(Airplane airplane, GameTime gameTime, Vector2? worldPixelSize = null)
        {
            DrawHelper.DrawSlice(airplane, _airplaneTexture, TheGame.SpriteBatch, worldPixelSize);
        }
    }
}

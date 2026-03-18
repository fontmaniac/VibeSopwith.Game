using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class AirplaneRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _airplaneTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            using var tex1 = Game.Content.Load<Texture2D>("Textures\\Plane_1.png");
            _airplaneTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex1);

        }

        public void Draw(Airplane airplane, GameTime gameTime)
        {
            DrawHelper.DrawOriginated(airplane, _airplaneTexture, new Vector2(115, airplane.NormalDown == Winding.Clockwise ? 100 : 0), TheGame.SpriteBatch);
        }
    }
}

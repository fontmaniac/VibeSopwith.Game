using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Components
{
    public class PrimitivesRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        public Texture2D Pixel = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 0.05f)
        {
            var edge = end - start;
            var angle = MathF.Atan2(edge.Y, edge.X);

            TheGame.SpriteBatch.Draw(
                Pixel,
                start,
                null,
                color,
                angle,
                new Vector2(0, 0.5f),
                new Vector2(edge.Length(), thickness),
                SpriteEffects.None,
                0f
            );
        }

    }
}

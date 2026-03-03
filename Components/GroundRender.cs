using VibeSopwith.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Components
{
    internal class GroundRender(Game game) : DrawableGameComponent(game)
    {
        private Texture2D _pixel = null!;
        private const int LineThickness = 3;

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(Ground ground, SpriteBatch spriteBatch)
        {
            for (int i = 1; i < ground.Points.Count; i++)
            {
                var start = ground.Points[i - 1];
                var end = ground.Points[i];

                DrawLine(spriteBatch, start, end, Color.White, LineThickness);
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thickness)
        {
            float distance = Vector2.Distance(point1, point2);
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            spriteBatch.Draw(_pixel!, point1, null, color, angle, Vector2.Zero, new Vector2(distance, thickness), SpriteEffects.None, 0);
        }
    }
}
using VibeSopwith.Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class GroundRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _pixel = null!;

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(Ground ground, float thickness, float scaleVert, SpriteBatch spriteBatch)
        {
            for (int i = 1; i < ground.Points.Count; i++)
            {
                var start = ground.Points[i - 1];
                var end = ground.Points[i];

                DrawLine(spriteBatch, start, end, Color.White, thickness, scaleVert);
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thicknessPx, float scaleVert)
        {
            float dx = point2.X - point1.X;
            float dy = point2.Y - point1.Y;

            float distanceWorld = Vector2.Distance(point1, point2);
            float angle = MathF.Atan2(dy, dx);

            Vector2 scale = new(distanceWorld, thicknessPx/scaleVert);

            spriteBatch.Draw(_pixel, point1, null, color, angle, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
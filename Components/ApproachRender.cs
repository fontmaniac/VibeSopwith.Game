using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class ApproachRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _pixel = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });
        }

        public void Draw(Autopilot.Approach approach, GameTime gameTime)
        {
            DrawZone(approach.PreFinal, Color.CornflowerBlue * 0.5f);
            DrawZone(approach.Final, Color.CornflowerBlue * 0.7f);
            DrawZone(approach.PreTouch, Color.CornflowerBlue * 0.9f);
        }

        private void DrawZone(Autopilot.ApproachZone zone, Color color)
        {
            // Rectangle corners
            var topLeft = new Vector2(zone.EntryX, zone.TopEntryY);
            var topRight = new Vector2(zone.ExitX, zone.TopEntryY);
            var bottomLeft = new Vector2(zone.EntryX, zone.BottomEntryY);
            var bottomRight = new Vector2(zone.ExitX, zone.BottomEntryY);

            // Fill rectangle
            var rectPos = new Vector2(
                MathF.Min(zone.EntryX, zone.ExitX),
                zone.BottomEntryY
            );

            var rectSize = new Vector2(
                MathF.Abs(zone.ExitX - zone.EntryX),
                zone.TopEntryY - zone.BottomEntryY
            );

            TheGame.SpriteBatch.Draw(_pixel, rectPos, null, color, 0f, Vector2.Zero, rectSize, SpriteEffects.None, 0f);

            // Outline (optional but useful)
            DrawLine(topLeft, topRight, Color.Blue);
            DrawLine(topRight, bottomRight, Color.Blue);
            DrawLine(bottomRight, bottomLeft, Color.Blue);
            DrawLine(bottomLeft, topLeft, Color.Blue);
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            var edge = end - start;
            var angle = MathF.Atan2(edge.Y, edge.X);

            TheGame.SpriteBatch.Draw(
                _pixel,
                start,
                null,
                color,
                angle,
                Vector2.Zero,
                new Vector2(edge.Length(), 0.05f),
                SpriteEffects.None,
                0f
            );
        }
    }
}

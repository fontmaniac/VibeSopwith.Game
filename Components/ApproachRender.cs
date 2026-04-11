using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class ApproachRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        public void Draw(Autopilot.Approach approach, GameTime gameTime, Vector2? worldPixelSize)
        {
            for (var n = approach.CounterDirectNode; n != null; n = n.Next)
                DrawZone(n.Zone, Color.CornflowerBlue * 0.5f, worldPixelSize);
            for (var n = approach.CoDirectNode; n != null; n = n.Next)
                DrawZone(n.Zone, Color.CornflowerBlue * 0.7f, worldPixelSize);
            DrawZone(approach.PreTouch, Color.CornflowerBlue * 0.9f, worldPixelSize);
        }

        private void DrawZone(Autopilot.ApproachZone zone, Color color, Vector2? worldPixelSize)
        {
            // Rectangle corners
            var topLeft = new Vector2(zone.EntryX, zone.TopEntryY).SnapToPixel(worldPixelSize);
            var topRight = new Vector2(zone.ExitX, zone.TopEntryY).SnapToPixel(worldPixelSize);
            var bottomLeft = new Vector2(zone.EntryX, zone.BottomEntryY).SnapToPixel(worldPixelSize);
            var bottomRight = new Vector2(zone.ExitX, zone.BottomEntryY).SnapToPixel(worldPixelSize);

            // Fill rectangle
            var rectPos = new Vector2(
                MathF.Min(zone.EntryX, zone.ExitX),
                zone.BottomEntryY
            ).SnapToPixel(worldPixelSize);

            var rectSize = new Vector2(
                MathF.Abs(zone.ExitX - zone.EntryX),
                zone.TopEntryY - zone.BottomEntryY
            );

            TheGame.SpriteBatchPoint.Draw(TheGame.Primitives.Pixel, rectPos, null, color, 0f, Vector2.Zero, rectSize, SpriteEffects.None, 0f);

            // Outline (optional but useful)
            TheGame.Primitives.DrawLine(topLeft, topRight, Color.Blue, 0.1f);
            TheGame.Primitives.DrawLine(topRight, bottomRight, Color.Blue, 0.1f);
            TheGame.Primitives.DrawLine(bottomRight, bottomLeft, Color.Blue, 0.1f);
            TheGame.Primitives.DrawLine(bottomLeft, topLeft, Color.Blue, 0.1f);

            // Funnel
            var funnelCenter = new Vector2(zone.EntryX, (zone.BottomEntryY + zone.TopEntryY) / 2f);
            var funnelArrow1 = funnelCenter + zone.Funnel.Ray1 * 2f;
            var funnelArrow2 = funnelCenter + zone.Funnel.Ray2 * 2f;
            TheGame.Primitives.DrawLine(funnelCenter, funnelArrow1, Color.Blue, 0.1f);
            TheGame.Primitives.DrawLine(funnelCenter, funnelArrow2, Color.Blue, 0.1f);
        }
    }
}

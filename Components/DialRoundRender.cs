using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class DialRoundRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private SpriteFont _font = null!;

        public new void LoadContent()
        {
            _font = Game.Content.Load<SpriteFont>("Fonts\\Arial12");
        }

        public record Style(
            float RimThickness, 
            float MarkThickness,
            float MarkOffset,           // % of radius from the rim
            float MarkLength,           // % of radius
            float MarkLabelOffset,      // % of radius from the rim to outside
            float SubMarkThickness,
            float SubMarkOffset,        // % of radius from the rim
            float SubMarkLength,        // % of radius 
            float ArmThickness,         
            float ArmLength             // % of radius 
            );

        public static Style DefaultStyle = new Style(2f, 1.5f, 0.05f, 0.2f, 0.3f, 1f, 0.07f, 0.1f, 1.8f, 0.7f);

        public void Draw(Dial dial, Vector2 center, float radius, Color color, Style style, GameTime gameTime)
        {
            // Draw Rim
            TheGame.Primitives.DrawCirclePrimitive(center, radius, color, style.RimThickness);

            float getAngle(float val) =>
                MathHelper.ToRadians((val - dial.MinVal) / (dial.MaxVal - dial.MinVal) * 360f) - float.Pi / 2f;

            // Draw big marks & labels
            foreach (var markVal in dial.Marks)
            {
                var angle = getAngle(markVal);

                var inner = radius * (1f - style.MarkOffset);
                var outer = inner - radius * style.MarkLength;
                var label = radius + radius * style.MarkLabelOffset;

                var p1 = center + new Vector2(inner * MathF.Cos(angle), inner * MathF.Sin(angle));
                var p2 = center + new Vector2(outer * MathF.Cos(angle), outer * MathF.Sin(angle));
                var p3 = center + new Vector2(label * MathF.Cos(angle), label * MathF.Sin(angle));

                TheGame.Primitives.DrawLine(p1, p2, color, style.MarkThickness);

                var text = markVal.ToString();
                Vector2 size = _font.MeasureString(text);
                Vector2 origin = size / 2f;
                TheGame.SpriteBatchPoint.DrawString(_font, text, p3 - origin, color);
            }

            // Draw submarks
            for (var i = 0; i < dial.SubMarks; ++i)
            {
                var subVal = dial.MinVal + (dial.MaxVal - dial.MinVal) * (i / (float)dial.SubMarks);
                var angle = getAngle(subVal);

                var inner = radius * (1f - style.SubMarkOffset);
                var outer = inner - radius * style.SubMarkLength;

                var p1 = center + new Vector2(inner * MathF.Cos(angle), inner * MathF.Sin(angle));
                var p2 = center + new Vector2(outer * MathF.Cos(angle), outer * MathF.Sin(angle));

                TheGame.Primitives.DrawLine(p1, p2, color, style.SubMarkThickness);
            }

            // Draw name
            {
                var text = dial.Name;
                Vector2 size = _font.MeasureString(text);
                Vector2 origin = size / 2f;
                var angle = getAngle(dial.MinVal);
                var p = center + new Vector2(radius/2f * MathF.Cos(angle), radius/2f * MathF.Sin(angle));
                TheGame.SpriteBatchPoint.DrawString(_font, text, p - origin, color);
            }

            // Draw Arm
            var armVal = dial.GetVal();
            var armAngle = getAngle(armVal);
            var armLength = radius * style.ArmLength;

            var endPoint = center + new Vector2(
                armLength * MathF.Cos(armAngle),
                armLength * MathF.Sin(armAngle)
            );

            TheGame.Primitives.DrawLine(center, endPoint, color, style.ArmThickness);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class DrawHelper
    {
        public static void DrawCentered(this ICentered centered, Texture2D texture, SpriteBatch spriteBatch)
        {
            // Rotation from direction vector (world-space)
            var rotation = (float)Math.Atan2(centered.Direction.Y, centered.Direction.X);

            // Texture origin (center of texture)
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

            // Scale from object dimensions to texture size (in meters)
            float scaleX = centered.Length / texture.Width;
            float scaleY = centered.Height / texture.Height;

            spriteBatch.Draw(
                texture,
                centered.Position, // in world units
                null,
                Color.White,
                rotation,
                origin,
                new Vector2(scaleX, scaleY), // in world units
                centered.NormalDown == Winding.Clockwise ? SpriteEffects.FlipVertically : SpriteEffects.None,
                0f
            );

        }
    }
}

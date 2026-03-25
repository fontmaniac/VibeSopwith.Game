using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class DrawHelper
    {
        public static void DrawOriginated(this ICentered centered, Texture2D texture, Vector2 origin, SpriteBatch spriteBatch)
        {
            // Rotation from direction vector (world-space)
            var rotation = (float)Math.Atan2(centered.Direction.Y, centered.Direction.X);

            // Scale from object dimensions to texture size (in meters)
            float scaleX = centered.Length / texture.Width;
            float scaleY = centered.Height / texture.Height;

            var adjOrigin = new Vector2(origin.X, texture.Height-origin.Y);

            spriteBatch.Draw(
                texture,
                centered.Position, // in world units
                null,
                Color.White,
                rotation,
                adjOrigin,
                new Vector2(scaleX, scaleY), // in world units
                centered.Spin == BasisSpin.Down ? SpriteEffects.FlipVertically : SpriteEffects.None,
                0f
            );
        }

        public static void DrawCentered(this ICentered centered, Texture2D texture, SpriteBatch spriteBatch) =>
            DrawOriginated(centered, texture, new Vector2(texture.Width / 2f, texture.Height / 2f), spriteBatch);
    }
}

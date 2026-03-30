using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class DrawHelper
    {
        public static void DrawOriginated(this ICentered centered, Texture2D texture, Vector2 origin, SpriteBatch spriteBatch, Vector2 worldPixelSize)
        {
            // Snap world position to virtual pixel grid
            Vector2 pos = centered.Position;

            float snappedX = MathF.Floor(pos.X / worldPixelSize.X) * worldPixelSize.X;
            float snappedY = MathF.Floor(pos.Y / worldPixelSize.Y) * worldPixelSize.Y;

            Vector2 snappedPos = new(snappedX, snappedY);
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

        public static void DrawOriginated(this ICentered centered, Texture2D texture, Vector2 origin, SpriteBatch spriteBatch) =>
            DrawOriginated(centered, texture, origin, spriteBatch, Vector2.One);

        public static void DrawCentered(this ICentered centered, Texture2D texture, SpriteBatch spriteBatch, Vector2 worldPixelSize) =>
            DrawOriginated(centered, texture, new Vector2(texture.Width / 2f, texture.Height / 2f), spriteBatch, worldPixelSize);

        public static void DrawCentered(this ICentered centered, Texture2D texture, SpriteBatch spriteBatch) =>
            DrawOriginated(centered, texture, new Vector2(texture.Width / 2f, texture.Height / 2f), spriteBatch, Vector2.One);

    }
}

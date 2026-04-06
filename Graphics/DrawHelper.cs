using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Graphics
{
    internal static class DrawHelper
    {
        public static Vector2 SnapToPixel(this Vector2 pos, Vector2? worldPixelSize)
        {
            float snappedX = worldPixelSize != null ? MathF.Floor(pos.X / worldPixelSize.Value.X) * worldPixelSize.Value.X : pos.X;
            float snappedY = worldPixelSize != null ? MathF.Floor(pos.Y / worldPixelSize.Value.Y) * worldPixelSize.Value.Y : pos.Y;
            return new(snappedX, snappedY);
        }

        public static void DrawOriginated(this IHasLocation loc, Texture2D texture, Vector2 origin, SpriteBatch spriteBatch, Vector2? worldPixelSize) =>
            DrawOriginatedHanded(loc, HandedSlice.LR.Wrap(texture.ToAtlas().GetSlice()), origin, spriteBatch, worldPixelSize);

        public static void DrawOriginatedHanded(this IHasLocation location, HandedSlice slice, Vector2 origin, SpriteBatch spriteBatch, Vector2? worldPixelSize)
        {
            // Assume location IS the world basis.            
            var wb = location;

            Vector2 pos = wb.Position;

            // Rotation from direction vector (world-space)
            var texDirection = wb.Direction * slice.FlipFactor;
            var rotation = texDirection.ToAngle();

            var theSlice = slice.TheSlice;

            // Scale from object dimensions to texture size (in meters)
            float scaleX = location.Length / theSlice.SourceRectangle.Width;
            float scaleY = location.Height / theSlice.SourceRectangle.Height;

            var adjOrigin = new Vector2(origin.X, theSlice.SourceRectangle.Height - origin.Y);

            // It is "flip : noFlip" to account for the fact that my world is Y-flipped relative to screen.
            // If it wasn't, the order would have been "noFlip : flip".
            var (flip, noFlip) = slice switch
            {
                HandedSlice.LR => (SpriteEffects.FlipVertically, SpriteEffects.None),
                HandedSlice.RL => (SpriteEffects.None, SpriteEffects.FlipVertically),
                _ => throw new ArgumentException("Logic error"),
            };

            spriteBatch.Draw(
                theSlice.Texture,
                pos.SnapToPixel(worldPixelSize), // in world units
                theSlice.SourceRectangle,
                Color.White,
                rotation,
                adjOrigin,
                new Vector2(scaleX, scaleY), // in world units
                wb.Spin == BasisSpin.Down ? flip : noFlip,      
                0f
            );
        }


        public static void DrawOriginated(this IHasLocation loc, Texture2D texture, Vector2 origin, SpriteBatch spriteBatch) =>
            DrawOriginated(loc, texture, origin, spriteBatch, null);

        public static void DrawCentered(this IHasLocation loc, Texture2D texture, SpriteBatch spriteBatch, Vector2 worldPixelSize) =>
            DrawOriginated(loc, texture, new Vector2(texture.Width / 2f, texture.Height / 2f), spriteBatch, worldPixelSize);

        public static void DrawCentered(this IHasLocation loc, Texture2D texture, SpriteBatch spriteBatch) =>
            DrawOriginated(loc, texture, new Vector2(texture.Width / 2f, texture.Height / 2f), spriteBatch, null);

    }
}

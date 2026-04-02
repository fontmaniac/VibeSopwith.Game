using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Graphics
{
    internal static class DrawHelper
    {
        public abstract record HandedTexture()
        {
            public sealed record LR(Texture2D Tex) : HandedTexture { public static HandedTexture Wrap(Texture2D x) => new LR(x); };
            public sealed record RL(Texture2D Tex) : HandedTexture { public static HandedTexture Wrap(Texture2D x) => new RL(x); };

            public float FlipFactor => this is LR ? +1f : this is RL ? -1f : throw new ArgumentException("Logic error");
            public Texture2D Texture => this is LR lr ? lr.Tex : this is RL rl ? rl.Tex : throw new ArgumentException("Logic error");
        }

        public static Vector2 SnapToPixel(this Vector2 pos, Vector2? worldPixelSize)
        {
            float snappedX = worldPixelSize != null ? MathF.Floor(pos.X / worldPixelSize.Value.X) * worldPixelSize.Value.X : pos.X;
            float snappedY = worldPixelSize != null ? MathF.Floor(pos.Y / worldPixelSize.Value.Y) * worldPixelSize.Value.Y : pos.Y;
            return new(snappedX, snappedY);
        }

        public static void DrawOriginated(this ILocation loc, Texture2D texture, Vector2 origin, SpriteBatch spriteBatch, Vector2? worldPixelSize) =>
            DrawOriginatedHanded(loc, HandedTexture.LR.Wrap(texture), origin, spriteBatch, worldPixelSize);

        public static void DrawOriginatedHanded(this ILocation loc, HandedTexture tex, Vector2 origin, SpriteBatch spriteBatch, Vector2? worldPixelSize)
        {
            // Snap world position to virtual pixel grid
            Vector2 pos = loc.Position;

            float snappedX = worldPixelSize != null ? MathF.Floor(pos.X / worldPixelSize.Value.X) * worldPixelSize.Value.X : pos.X;
            float snappedY = worldPixelSize != null ? MathF.Floor(pos.Y / worldPixelSize.Value.Y) * worldPixelSize.Value.Y : pos.Y;

            var snappedPos = pos.SnapToPixel(worldPixelSize);
            // Rotation from direction vector (world-space)
            var textDirection = loc.Direction * tex.FlipFactor;
            var rotation = textDirection.ToAngle();
            var texture = tex.Texture;

            // Scale from object dimensions to texture size (in meters)
            float scaleX = loc.Length / texture.Width;
            float scaleY = loc.Height / texture.Height;

            var adjOrigin = new Vector2(origin.X, texture.Height-origin.Y);

            var (flip, noFlip) = tex switch
            {
                HandedTexture.LR => (SpriteEffects.FlipVertically, SpriteEffects.None),
                HandedTexture.RL => (SpriteEffects.None, SpriteEffects.FlipVertically),
                _ => throw new ArgumentException("Logic error"),
            };

            spriteBatch.Draw(
                texture,
                loc.Position, // in world units
                null,
                Color.White,
                rotation,
                adjOrigin,
                new Vector2(scaleX, scaleY), // in world units
                loc.Spin == BasisSpin.Down ? flip : noFlip,
                0f
            );
        }

        public static void DrawOriginated(this ILocation loc, Texture2D texture, Vector2 origin, SpriteBatch spriteBatch) =>
            DrawOriginated(loc, texture, origin, spriteBatch, null);

        public static void DrawCentered(this ILocation loc, Texture2D texture, SpriteBatch spriteBatch, Vector2 worldPixelSize) =>
            DrawOriginated(loc, texture, new Vector2(texture.Width / 2f, texture.Height / 2f), spriteBatch, worldPixelSize);

        public static void DrawCentered(this ILocation loc, Texture2D texture, SpriteBatch spriteBatch) =>
            DrawOriginated(loc, texture, new Vector2(texture.Width / 2f, texture.Height / 2f), spriteBatch, null);

    }
}

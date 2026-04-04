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

        public static void DrawOriginated(this IHasLocation loc, Texture2D texture, Vector2 origin, SpriteBatch spriteBatch, Vector2? worldPixelSize) =>
            DrawOriginatedHanded(loc, HandedTexture.LR.Wrap(texture), origin, spriteBatch, worldPixelSize);

        public static void DrawOriginatedHanded(this IHasLocation location, HandedTexture tex, Vector2 origin, SpriteBatch spriteBatch, Vector2? worldPixelSize)
        {
            // Assume location IS the world basis.            
            var wb = location;

            Vector2 pos = wb.Position;

            // Rotation from direction vector (world-space)
            var texDirection = wb.Direction * tex.FlipFactor;
            var rotation = texDirection.ToAngle();

            var texture = tex.Texture;

            // Scale from object dimensions to texture size (in meters)
            float scaleX = location.Length / texture.Width;
            float scaleY = location.Height / texture.Height;

            var adjOrigin = new Vector2(origin.X, texture.Height - origin.Y);

            // It is "flip : noFlip" to account for the fact that my world is Y-flipped relative to screen.
            // If it wasn't, the order would have been "noFlip : flip".
            var (flip, noFlip) = tex switch
            {
                HandedTexture.LR => (SpriteEffects.FlipVertically, SpriteEffects.None),
                HandedTexture.RL => (SpriteEffects.None, SpriteEffects.FlipVertically),
                _ => throw new ArgumentException("Logic error"),
            };

            spriteBatch.Draw(
                texture,
                pos.SnapToPixel(worldPixelSize), // in world units
                null,
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

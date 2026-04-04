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



        static (Vector2 X, Vector2 Y) BasisAxes(IBasis b)
        {
            var x = Vector2.Normalize(b.Direction);
            var y = b.Spin == BasisSpin.Down
                ? new Vector2(x.Y, x.X)     // Collinear
                : new Vector2(-x.Y, -x.X);  // Counterlinear
            return (x, y);
        }

        static IBasis ResolveWorldBasis(IBasis b)
        {
            if (b is not IRelativeBasis rel)
                return b; // already world

            var parent = ResolveWorldBasis(rel.Parent);

            var (px, py) = BasisAxes(parent);
            var (lx, ly) = BasisAxes(rel);

            // Compose orientation
            var worldX = px * lx.X + py * lx.Y;
            var worldY = px * ly.X + py * ly.Y;

            // Compose position
            var worldPos =
                parent.Position +
                px * rel.Position.X +
                py * rel.Position.Y;

            // Compose spin
            // Is it correct? 
            var worldSpin =
                parent.Spin == rel.Spin ? BasisSpin.Down : BasisSpin.Up;

            return new Basis(worldPos, worldX, worldSpin);
        }


        public static void DrawOriginatedHanded(this IHasLocation loc, HandedTexture tex, Vector2 origin, SpriteBatch spriteBatch, Vector2? worldPixelSize)
        {
            // 1. Resolve world basis
            //var wb = ResolveWorldBasis(loc);

            // 1. New interpretation - assume loc IS the world basis.            
            var wb = loc;

            Vector2 pos = wb.Position;

            // Rotation from direction vector (world-space)
            var texDirection = wb.Direction * tex.FlipFactor;
            var rotation = texDirection.ToAngle();

            var texture = tex.Texture;

            // Scale from object dimensions to texture size (in meters)
            float scaleX = loc.Length / texture.Width;
            float scaleY = loc.Height / texture.Height;

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
                wb.Spin == BasisSpin.Down ? flip : noFlip,      // Previously it was using loc.Spin, which was local one before resolution. I fixed it.
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

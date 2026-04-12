using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    public record struct TextureSlice(Texture2D Texture, Rectangle SourceRectangle, Vector2 TextureOrigin);

    public abstract record HandedSlice()
    {
        public sealed record LR(TextureSlice Slice) : HandedSlice { public static LR Wrap(TextureSlice x) => new LR(x); };
        public sealed record RL(TextureSlice Slice) : HandedSlice { public static RL Wrap(TextureSlice x) => new RL(x); };

        public float FlipFactor => this is LR ? +1f : this is RL ? -1f : throw new ArgumentException("Logic error");
        public TextureSlice TheSlice => this is LR lr ? lr.Slice : this is RL rl ? rl.Slice : throw new ArgumentException("Logic error");
    }

    internal static class Atlas
    {
        public static Func<int, int, Vector2> OriginCentered = (w, h) => new Vector2(w / 2, h / 2);
        public static TextureAtlas.Single ToAtlas(this Texture2D src, Vector2 origin) => new TextureAtlas.Single(src, origin);
        public static TextureAtlas.Single ToAtlas(this Texture2D src, Func<int, int, Vector2> getOrigin) =>
            new TextureAtlas.Single(
                src,
                getOrigin(
                    TextureAtlas.SpriteSheet.GetSpriteWidth(src, 1),
                    TextureAtlas.SpriteSheet.GetSpriteHeight(src, 1)));
        public static TextureAtlas.SpriteSheet ToAtlas(this Texture2D src, Vector2 origin, int cols, int rows) => new TextureAtlas.SpriteSheet(src, origin, cols, rows);
        public static TextureAtlas.SpriteSheet ToAtlas(this Texture2D src, Func<int, int, Vector2> getOrigin, int cols, int rows) => 
            new TextureAtlas.SpriteSheet(
                src, 
                getOrigin(
                    TextureAtlas.SpriteSheet.GetSpriteWidth(src, cols), 
                    TextureAtlas.SpriteSheet.GetSpriteHeight(src, rows)), 
                cols, 
                rows);

        public static HandedSlice.LR ToLRSlice(this TextureAtlas.Single s) => HandedSlice.LR.Wrap(s.GetSlice());
        public static HandedSlice.RL ToRLSlice(this TextureAtlas.Single s) => HandedSlice.RL.Wrap(s.GetSlice());

        public static TextureAtlas.BoundSpriteSheet<T> Bind<T>(this TextureAtlas.SpriteSheet untyped) => new TextureAtlas.BoundSpriteSheet<T>(untyped);
    }

    internal abstract record TextureAtlas()
    {
        public sealed record Single(Texture2D Texture, Vector2 OriginLRSD) : TextureAtlas;
        public sealed record SpriteSheet(Texture2D Texture, Vector2 OriginLRSD, int Cols, int Rows) : TextureAtlas
        {
            public static int GetSpriteWidth(Texture2D tex, int cols) => tex.Width / cols;
            public static int GetSpriteHeight(Texture2D tex, int rows) => tex.Height / rows;
            public int SpriteWidth = GetSpriteWidth(Texture, Cols);
            public int SpriteHeight = GetSpriteHeight(Texture, Rows);
        }

        public abstract record BoundBase(TextureAtlas Untyped) : TextureAtlas;
        public sealed record BoundSingle<T>(Single UntypedSingle) : BoundBase(UntypedSingle);
        public sealed record BoundSpriteSheet<T>(SpriteSheet UntypedSpriteSheet) : BoundBase(UntypedSpriteSheet);

        public TextureSlice GetSlice() =>
            this switch
            {
                Single s => new(s.Texture, new(0, 0, s.Texture.Width, s.Texture.Height), s.OriginLRSD),
                BoundBase bb => bb.Untyped.GetSlice(),
                _ => throw new NotSupportedException()
            };

        public TextureSlice GetSlice(int col, int row) =>
            this switch
            {
                Single s when col == 0 && row == 0 => s.GetSlice(),
                SpriteSheet ss => new(ss.Texture, new Rectangle(col * ss.SpriteWidth + 1, row * ss.SpriteHeight + 1, ss.SpriteWidth - 2, ss.SpriteHeight - 2), ss.OriginLRSD),
                BoundBase bb => bb.Untyped.GetSlice(col, row),
                _ => throw new NotSupportedException()
            };

        public TextureSlice GetSlice(int index) =>
            this switch
            {
                Single s => s.GetSlice(),
                SpriteSheet ss => ss.GetSlice(index % ss.Cols, index / ss.Cols),
                BoundBase bb => bb.Untyped.GetSlice(index),
                _ => throw new NotSupportedException()
            };

    }

}

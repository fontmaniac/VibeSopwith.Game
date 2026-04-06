using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    public record struct TextureSlice(Texture2D Texture, Rectangle SourceRectangle);

    public abstract record HandedSlice()
    {
        public sealed record LR(TextureSlice Slice) : HandedSlice { public static HandedSlice Wrap(TextureSlice x) => new LR(x); };
        public sealed record RL(TextureSlice Slice) : HandedSlice { public static HandedSlice Wrap(TextureSlice x) => new RL(x); };

        public float FlipFactor => this is LR ? +1f : this is RL ? -1f : throw new ArgumentException("Logic error");
        public TextureSlice TheSlice => this is LR lr ? lr.Slice : this is RL rl ? rl.Slice : throw new ArgumentException("Logic error");
    }

    internal static class TextureLoader
    {
        public static Texture2D Load(Microsoft.Xna.Framework.Game game, string fileName) => game.Content.Load<Texture2D>(fileName);
        public static Texture2D LoadMipMap(Microsoft.Xna.Framework.Game game, GraphicsDevice gd, SpriteBatch sb, string fileName) => MipMap.CastWithMipMaps(gd, sb, Load(game, fileName));
        public static TextureAtlas.Single ToAtlas(this Texture2D src) => new TextureAtlas.Single(src);
        public static TextureAtlas.SpriteSheet ToAtlas(this Texture2D src, int cols, int rows) => new TextureAtlas.SpriteSheet(src, cols, rows);
    }

    internal abstract record TextureAtlas()
    {
        public sealed record Single(Texture2D Texture) : TextureAtlas;
        public sealed record SpriteSheet(Texture2D Texture, int Cols, int Rows) : TextureAtlas
        {
            public int SpriteWidth = Texture.Width / Cols;
            public int SpriteHeight = Texture.Height / Rows;
        }
        public sealed record Atlas(Texture2D Texture, IDictionary<string, Rectangle> Regions) : TextureAtlas;

        public static Single LoadSingle(Microsoft.Xna.Framework.Game game, GraphicsDevice gd, SpriteBatch sb, string fileName) =>
            new Single(TextureLoader.LoadMipMap(game, gd, sb, fileName));

        public static SpriteSheet LoadSpriteSheet(Microsoft.Xna.Framework.Game game, GraphicsDevice gd, SpriteBatch sb, int cols, int rows, string fileName) =>
            new SpriteSheet(TextureLoader.LoadMipMap(game, gd, sb, fileName), cols, rows);

        public TextureSlice GetSlice() =>
            this switch
            {
                Single s => new(s.Texture, new(0, 0, s.Texture.Width, s.Texture.Height)),
                _ => throw new NotSupportedException()
            };

        public TextureSlice GetSlice(int col, int row)
        {
            return this switch
            {
                Single s when col == 0 && row == 0 => s.GetSlice(),
                SpriteSheet ss => new(ss.Texture, new Rectangle(col * ss.SpriteWidth + 1, row * ss.SpriteHeight + 1, ss.SpriteWidth - 2, ss.SpriteHeight - 2)),
                _ => throw new NotSupportedException()
            };
        }

        public TextureSlice GetSlice(int index) =>
            this switch
            {
                Single s => s.GetSlice(),
                SpriteSheet ss => ss.GetSlice(index % ss.Cols, index / ss.Cols),
                _ => throw new NotSupportedException()
            };

    }

}

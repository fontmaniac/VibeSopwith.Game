using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class ExplosionRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        public record struct TextureInfo(Explosion.ExplosionVariant Variant, string TexturePath, int SheetRows, int SheetCols, Func<Vector2, Vector2> GetOrigin);

        private static TextureInfo[] Textures = new TextureInfo[]
        {
            new(Explosion.ExplosionVariant.Based1, "Textures\\Explosion_1.png", 5, 10, m => new Vector2(m.X / 2f, m.Y)),
            new(Explosion.ExplosionVariant.Centered1,"Textures\\Explosion_2.png", 4, 4, m => new Vector2(m.X / 2f, m.Y / 2f)),
            new(Explosion.ExplosionVariant.BluePlasma, "Textures\\Explosion_3.png", 8, 5, m => new Vector2(m.X / 2f, m.Y / 2f)),
        };

        private record struct SequenceTemplate(Animation.IStaticPhase<Explosion>[] Phases);

        private IDictionary<Explosion.ExplosionVariant, SequenceTemplate> Templates = null!;

        private record ExplosionPhase(int phaseNumber, TextureSlice slice) : Animation.IStaticPhase<Explosion>
        {
            public TimeSpan GetDuration(Explosion explosion) => explosion.Duration / phaseNumber;
            public HandedSlice GetSlice(Explosion explosion) => HandedSlice.LR.Wrap(slice);
        }

        private (Explosion.ExplosionVariant Variant, SequenceTemplate Template) MakeTemplate(TextureInfo ti)
        {
            var spriteSheet =
                MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>(ti.TexturePath))
                    .ToAtlas((w, h) => ti.GetOrigin(new Vector2(w, h)), ti.SheetCols, ti.SheetRows);
            var totalPhases = spriteSheet.Rows * spriteSheet.Cols;

            var phases =
                Enumerable.Range(0, totalPhases)
                .Select(i => new ExplosionPhase(totalPhases, spriteSheet.GetSlice(i)))
                .ToArray();

            return (ti.Variant, new SequenceTemplate(phases));
        }

        public new void LoadContent()
        {
            base.LoadContent();

            Templates = Textures
                .Select(MakeTemplate)
                .ToDictionary(x => x.Variant, x => x.Template);
        }

        public void Draw(Explosion explosion, GameTime gameTime)
        {
            var variant = Templates[explosion.Variant];
            Animation.DrawStaticSequence(explosion, Animation.Make(explosion.StartTime, variant.Phases, false), gameTime, TheGame.SpriteBatch);
        }
    }
}

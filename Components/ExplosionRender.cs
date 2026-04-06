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

        private record struct VariantInfo(TextureInfo Info, TextureAtlas.SpriteSheet SpriteSheet, Animation.IPhase<Explosion>[] Phases);

        private IDictionary<Explosion.ExplosionVariant, VariantInfo> Variants = null!;

        private record ExplosionPhase(int phaseNumber, TextureSlice slice) : Animation.IPhase<Explosion>
        {
            public TimeSpan GetDuration(Explosion explosion) => explosion.Duration / phaseNumber;

            public void Draw(SpriteBatch sb, Explosion explosion) => DrawHelper.DrawSlice(explosion, HandedSlice.LR.Wrap(slice), sb, null);
        }

        public new void LoadContent()
        {
            base.LoadContent();

            Variants = Textures
                .Select(si =>
                {
                    var spriteSheet = 
                        MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>(si.TexturePath))
                            .ToAtlas((w, h) => si.GetOrigin(new Vector2(w, h)), si.SheetCols, si.SheetRows);
                    var totalPhases = si.SheetRows * si.SheetCols;

                    var phases =
                        Enumerable.Range(0, totalPhases)
                        .Select(i => new ExplosionPhase(totalPhases, spriteSheet.GetSlice(i)))
                        .ToArray();

                    return (new VariantInfo(si, spriteSheet, phases), si.Variant);
                })
                .ToDictionary(x => x.Item2, x => x.Item1);
        }

        public void Draw(Explosion explosion, GameTime gameTime)
        {
            var variant = Variants[explosion.Variant];
            Animation.Draw(explosion, explosion.StartTime, variant.Phases, false, gameTime, TheGame.SpriteBatch);
        }
    }
}

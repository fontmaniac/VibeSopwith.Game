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
            new(Explosion.ExplosionVariant.Based1, "Textures\\Explosion_1.png", 5, 10, m => new Vector2(m.X / 2f, 0f)),
            new(Explosion.ExplosionVariant.Centered1,"Textures\\Explosion_2.png", 4, 4, m => new Vector2(m.X / 2f, m.Y / 2f)),
            new(Explosion.ExplosionVariant.BluePlasma, "Textures\\Explosion_3.png", 8, 5, m => new Vector2(m.X / 2f, m.Y / 2f)),
        };

        private record struct VariantInfo(TextureInfo Info, Texture2D SpriteSheet, Animation.IPhase<Explosion>[] Phases);

        private IDictionary<Explosion.ExplosionVariant, VariantInfo> Variants = null!;

        private record ExplosionPhase(int phaseNumber, Texture2D spriteSheet, Rectangle sheetRect, Vector2 origin) : Animation.IPhase<Explosion>
        {
            public TimeSpan GetDuration(Explosion explosion) => explosion.Duration / phaseNumber;

            public void Draw(SpriteBatch sb, Explosion explosion)
            {
                Vector2 scaleTexture(int texWidth, int texHeight) => new Vector2(explosion.Length / texWidth, explosion.Height / texHeight);
                sb.Draw(
                    spriteSheet, 
                    explosion.RootPosition, 
                    sheetRect, 
                    Color.White, 
                    0f, 
                    origin, 
                    scaleTexture(sheetRect.Width, sheetRect.Height), 
                    SpriteEffects.FlipVertically, 
                    0f);
            }
        }

        public new void LoadContent()
        {
            base.LoadContent();

            Variants = Textures
                .Select(si =>
                {
                    using var tex = Game.Content.Load<Texture2D>(si.TexturePath);
                    var spriteSheet = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex);

                    var frameWidth = spriteSheet.Width / si.SheetCols;
                    var frameHeight = spriteSheet.Height / si.SheetRows;
                    var phaseNumber = si.SheetRows * si.SheetCols;

                    var phases =
                        Enumerable.Range(0, phaseNumber)
                        .Select(i =>
                        {
                            var texX = (i % si.SheetCols) * frameWidth;
                            var texY = (i / si.SheetCols) * frameHeight;
                            var srcRect = new Rectangle(texX + 1, texY + 1, frameWidth - 2, frameHeight - 2);
                            var origin = si.GetOrigin(new Vector2(frameWidth, frameHeight));

                            return new ExplosionPhase(phaseNumber, spriteSheet, srcRect, origin);
                        })
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

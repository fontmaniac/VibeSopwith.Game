using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class ExplosionRender(Microsoft.Xna.Framework.Game game, int variant) : DrawableGameComponent(game)
    {
        private Texture2D _spriteSheet = null!;

        public record struct SpriteInfo(string TexturePath, int SheetRows, int SheetCols, Func<Vector2, Vector2> GetOrigin);

        private static SpriteInfo[] Variants = new SpriteInfo[]
        {
            new("Textures\\Explosion_1.png", 5, 10, m => new Vector2(m.X / 2f, 0f)),
            new("Textures\\Explosion_2.png", 4, 4, m => new Vector2(m.X / 2f, m.Y / 2f)),
            new("Textures\\Explosion_3.png", 8, 5, m => new Vector2(m.X / 2f, m.Y / 2f)),
        };

        private SpriteInfo _si = Variants[variant];

        private Animation.IPhase<Explosion>[] _phases = null!;

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

            using var tex = Game.Content.Load<Texture2D>(_si.TexturePath);
            _spriteSheet = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex);

            var frameWidth = _spriteSheet.Width / _si.SheetCols;
            var frameHeight = _spriteSheet.Height / _si.SheetRows;
            var phaseNumber = _si.SheetRows * _si.SheetCols;

            _phases =
                Enumerable.Range(0, phaseNumber)
                .Select(i =>
                {
                    var texX = (i % _si.SheetCols) * frameWidth;
                    var texY = (i / _si.SheetCols) * frameHeight;
                    var srcRect = new Rectangle(texX + 1, texY + 1, frameWidth - 2, frameHeight - 2);
                    var origin = _si.GetOrigin(new Vector2(frameWidth, frameHeight));

                    return new ExplosionPhase(phaseNumber, _spriteSheet, srcRect, origin);
                })
                .ToArray();
        }

        public void Draw(Explosion explosion, GameTime gameTime)
        {
            Animation.Draw(explosion, explosion.StartTime, _phases, false, gameTime, TheGame.SpriteBatch);
        }
    }
}

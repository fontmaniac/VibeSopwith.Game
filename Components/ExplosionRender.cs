using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class ExplosionRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _spriteSheet = null!;
        private const int SheetRows = 4;
        private const int SheetCols = 4;

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

            using var tex = Game.Content.Load<Texture2D>("Textures\\Explosion_2.png");
            _spriteSheet = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex);

            var frameWidth = _spriteSheet.Width / SheetCols;
            var frameHeight = _spriteSheet.Height / SheetRows;
            var phaseNumber = SheetRows * SheetCols;

            _phases =
                Enumerable.Range(0, phaseNumber)
                .Select(i =>
                {
                    var texX = (i % SheetCols) * frameWidth;
                    var texY = (i / SheetCols) * frameHeight;
                    var srcRect = new Rectangle(texX + 1, texY + 1, frameWidth - 2, frameHeight - 2);
                    var origin = new Vector2(frameWidth / 2f, frameHeight / 2f);

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

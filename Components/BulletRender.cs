using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class BulletRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _bulletTexture = null!;
        private const float BulletLengthPx = 4f;    // Bullet rendered at fixed pixel size regardless of scale.
        private const float BulletHeightPx = 4f;

        public new void LoadContent()
        {
            base.LoadContent();

            var texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            _bulletTexture = texture.ToAtlas(Atlas.OriginCentered).ToLRSlice();
        }

        public void Draw(Bullet bullet, float pxPerUnit, GameTime gameTime)
        {
            DrawHelper.DrawSlice(
                Location.OffInterface(bullet) with 
                { 
                    Length = BulletLengthPx / pxPerUnit,  
                    Height = BulletHeightPx / pxPerUnit,
                }, 
                _bulletTexture, TheGame.SpriteBatch);
        }

    }
}

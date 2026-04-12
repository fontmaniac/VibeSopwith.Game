using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nage.Strata.Graphics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class BombRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _bombTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();
            _bombTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\Bomb_1.png")).ToAtlas(Atlas.OriginCentered).ToLRSlice();
        }

        public void Draw(Bomb bomb, GameTime gameTime)
        {
            DrawHelper.DrawSlice(bomb, _bombTexture, TheGame.SpriteBatchPoint);
        }
    }
}

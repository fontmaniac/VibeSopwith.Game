using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core.ParticleSystem;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class ParticleSystemRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _pixelTexture = null!;
        private HandedSlice.RL _dropletTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            var texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            _pixelTexture = texture.ToAtlas(Atlas.OriginCentered).ToLRSlice();

            _dropletTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Water_Shot_4.png")).ToAtlas(new Vector2(18, 18)).ToRLSlice();

        }

        public void Draw(Prototype particleSystem, GameTime gameTime)
        {
            foreach (var particle in particleSystem.Particles)
            {
                DrawHelper.DrawSlice(particle, _dropletTexture, TheGame.SpriteBatch);
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nage.Strata.Graphics;

namespace VibeSopwith.Game.Utils.ParticleSystem.Special
{
    internal class EmitterWaterJetRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _pixelTexture = null!;
        private HandedSlice.RL _dropletTexture0 = null!;
        private HandedSlice.RL _dropletTexture1 = null!;
        private HandedSlice.RL _dropletTexture2 = null!;
        private HandedSlice.RL _dropletTexture3 = null!;
        private HandedSlice.RL _dropletTexture4 = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            var texture = new Texture2D(GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.White });
            _pixelTexture = texture.ToAtlas(Atlas.OriginCentered).ToLRSlice();

            var dropletTexture = Game.Content.Load<Texture2D>("Textures\\Water_Shot_4.png");
            _dropletTexture0 = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, dropletTexture.PremultiplyAlpha()).ToAtlas(new Vector2(18, 18)).ToRLSlice();
            _dropletTexture1 = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, dropletTexture.ScaleAlpha(0.8f, 48).PremultiplyAlpha()).ToAtlas(new Vector2(18, 18)).ToRLSlice();
            _dropletTexture2 = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, dropletTexture.ScaleAlpha(0.6f, 48).PremultiplyAlpha()).ToAtlas(new Vector2(18, 18)).ToRLSlice();
            _dropletTexture3 = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, dropletTexture.ScaleAlpha(0.4f, 48).PremultiplyAlpha()).ToAtlas(new Vector2(18, 18)).ToRLSlice();
            _dropletTexture4 = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, dropletTexture.ScaleAlpha(0.2f, 48).PremultiplyAlpha()).ToAtlas(new Vector2(18, 18)).ToRLSlice();
        }

        public void Draw(EmitterWaterJet particleSystem, GameTime gameTime)
        {
            foreach (var particle in particleSystem.Particles)
            {
                var texture =
                    particle.AgePct > 0.6f ? _dropletTexture4 :
                    particle.AgePct > 0.5f ? _dropletTexture3 :
                    particle.AgePct > 0.4f ? _dropletTexture2 :
                    particle.AgePct > 0.2f ? _dropletTexture1 :
                    _dropletTexture0;
                particle.DrawSlice(texture, TheGame.SpriteBatchLinear);
            }
        }
    }
}

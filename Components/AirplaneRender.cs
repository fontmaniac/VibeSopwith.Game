using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class AirplaneRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _airplaneTexture = null!;
        private TextureAtlas.BoundSpriteSheet<Airplane> _airplaneSprites = null!;
        private Animation.IStaticSequence<Airplane> _sequence = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _airplaneTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Plane_1_Q.png")).ToAtlas(new Vector2(115, 100)).ToLRSlice();
            _airplaneSprites = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Plane_SS_1.png")).ToAtlas(new Vector2(115, 100), 1, 4).Bind<Airplane>();
            var phases = new[] 
            {
                _airplaneSprites.MakeStaticPhase(0, 0.25f, HandedSlice.LR.Wrap),
                _airplaneSprites.MakeStaticPhase(1, 0.25f, HandedSlice.LR.Wrap),
                _airplaneSprites.MakeStaticPhase(2, 0.25f, HandedSlice.LR.Wrap),
                _airplaneSprites.MakeStaticPhase(3, 0.25f, HandedSlice.LR.Wrap),
            };
            _sequence = new Animation.StaticSequence<Airplane>(TimeSpan.FromSeconds(0), phases, true);
        }

        public void Draw(Airplane airplane, GameTime gameTime, Vector2? worldPixelSize = null)
        {
            Animation.DrawStaticSequence(airplane, _sequence, gameTime, TheGame.SpriteBatch);
            //DrawHelper.DrawSlice(airplane, _airplaneTexture, TheGame.SpriteBatch, worldPixelSize);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class AirplaneRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _airplaneTexture = null!;
        private TextureAtlas.SpriteSheet _airplaneSprites = null!;
        private Animation.IStaticSequence<Airplane> _sequence = null!;

        private record AnimationPhase(TextureAtlas.SpriteSheet ss, int idx) : Animation.IStaticPhase<Airplane>
        {
            public TimeSpan GetDuration(Airplane ctx) => TimeSpan.FromSeconds(0.25f);
            public HandedSlice GetSlice(Airplane ctx) => HandedSlice.LR.Wrap(ss.GetSlice(idx));
            public static Animation.IStaticPhase<Airplane>  Make(TextureAtlas.SpriteSheet ss, int idx) => new AnimationPhase(ss, idx);
        }

        public new void LoadContent()
        {
            base.LoadContent();

            _airplaneTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Plane_1_Q.png")).ToAtlas(new Vector2(115, 100)).ToLRSlice();
            _airplaneSprites = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Plane_SS_1.png")).ToAtlas(new Vector2(115, 100), 1, 4);
            var phases = new[] 
            { 
                AnimationPhase.Make(_airplaneSprites, 0),
                AnimationPhase.Make(_airplaneSprites, 1),
                AnimationPhase.Make(_airplaneSprites, 2),
                AnimationPhase.Make(_airplaneSprites, 3),
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

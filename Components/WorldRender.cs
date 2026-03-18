using VibeSopwith.Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class WorldRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private GroundRender _groundRender = null!;
        private AetherBodyRender _bodyRender = null!;

        private AirplaneRender _airplaneRender = null!;
        private ExplosionRender _explosionRender = null!;


        public override void Initialize()
        {
            base.Initialize();
        }

        public new void LoadContent()
        {
            base.LoadContent();

            _groundRender = new GroundRender(Game); 
            _groundRender.LoadContent(GraphicsDevice);

            _airplaneRender = new AirplaneRender(Game);
            _airplaneRender.LoadContent();

            _explosionRender = new ExplosionRender(Game);
            _explosionRender.LoadContent();

            _bodyRender = new AetherBodyRender(Game);
            _bodyRender.LoadContent();
        }

        protected override void UnloadContent()
        {
            _airplaneRender?.Dispose();
            _groundRender?.Dispose();
            _explosionRender?.Dispose();
        }

        private void DrawStraight(GameWorld world, GameTime gameTime, float cameraPositionX)
        {
            float scaleFactor = (float)GraphicsDevice.Viewport.Height / GameWorld.WorldHeight;

            // 1. Scale: X normal, Y flipped
            var scale = Matrix.CreateScale(scaleFactor, -scaleFactor, 1f);

            // 2. Move world origin (0,0) to bottom-left of screen
            var translateY = Matrix.CreateTranslation(0f, GraphicsDevice.Viewport.Height, 0f);

            // 3. Camera X
            var translateCamera = Matrix.CreateTranslation(-cameraPositionX, 0f, 0f);

            // Final: world -> camera -> scale+flip -> move to screen
            var transform = translateCamera * scale * translateY;

            TheGame.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.LinearClamp, 
                DepthStencilState.None,
                RasterizerState.CullNone, 
                null,
                transform);

            _groundRender.Draw(world.Ground, thickness: 0.2f, TheGame.SpriteBatch);
            _airplaneRender.Draw(world.Plane, gameTime);
            foreach (var explosion in world.Explosions)
                _explosionRender.Draw(explosion, gameTime);
            _bodyRender.Draw(world.Plane.Body, gameTime);
            _bodyRender.Draw(world.Ground.Body, gameTime);

            TheGame.SpriteBatch.End();
        }


        public void Draw(GameWorld world, GameTime gameTime, float cameraPositionX)
        {
            base.Draw(gameTime);

            DrawStraight(world, gameTime, cameraPositionX);
        }


    }
}
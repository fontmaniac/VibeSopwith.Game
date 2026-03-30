using VibeSopwith.Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class WorldRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private GroundRender _groundRender = null!;
        private AetherBodyRender _bodyRender = null!;
        private ApproachRender _approachRender = null!;

        private AirplaneRender _airplaneRender = null!;
        private ExplosionRender _explosionRender = null!;
        private BombRender _bombRender = null!;
        private BulletRender _bulletRender = null!;
        private StaticBuildingRender _buildingRender = null!;



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

            _bombRender = new BombRender(Game);
            _bombRender.LoadContent();

            _bulletRender = new BulletRender(Game);
            _bulletRender.LoadContent();

            _buildingRender = new StaticBuildingRender(Game);
            _buildingRender.LoadContent();

            _bodyRender = new AetherBodyRender(Game);
            _bodyRender.LoadContent();

            _approachRender = new ApproachRender(Game);
            _approachRender.LoadContent();
        }

        protected override void UnloadContent()
        {
            _approachRender?.Dispose();
            _bodyRender?.Dispose();
            _buildingRender?.Dispose();
            _bulletRender?.Dispose();
            _bombRender?.Dispose();
            _airplaneRender?.Dispose();
            _groundRender?.Dispose();
            _explosionRender?.Dispose();
        }

        private void DrawStraight(GameWorld world, GameTime gameTime, float scaleHorz, float scaleVert, bool drawBullets, float groundThicknessPx, float cameraPositionX)
        {
            // 1. Scale: X normal, Y flipped
            var scale = Matrix.CreateScale(scaleHorz, -scaleVert, 1f);

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

            _groundRender.Draw(world.Ground, groundThicknessPx, scaleVert, TheGame.SpriteBatch, transform);
            _bodyRender.Draw(world.Ceiling.Body, gameTime);

            foreach (var approach in world.Approaches)
                _approachRender.Draw(approach, gameTime);

            foreach (var building in world.Buildings)
            {
                _buildingRender.Draw(building, gameTime);
                _bodyRender.Draw(building.Body, gameTime);
            }

            _airplaneRender.Draw(world.Plane, gameTime);

            foreach (var bomb in world.Bombs)
                _bombRender.Draw(bomb, gameTime);

            if (drawBullets)
                foreach (var bullet in world.Bullets)
                    _bulletRender.Draw(bullet, scaleVert, gameTime);

            foreach (var explosion in world.GetExplosions())
                _explosionRender.Draw(explosion, gameTime);

            _bodyRender.Draw(world.Plane.Body, gameTime);
            _bodyRender.Draw(world.Ground.Body, gameTime);


            TheGame.SpriteBatch.End();
        }


        public void Draw(GameWorld world, GameTime gameTime, float cameraPositionX)
        {
            base.Draw(gameTime);

            float scaleVertFactor = (float)GraphicsDevice.Viewport.Height / GameWorld.WorldHeight;

            DrawStraight(world, gameTime, scaleVertFactor, scaleVertFactor, true, 4f, cameraPositionX);
        }

        public void DrawMinimap(GameWorld world, GameTime gameTime)
        {
            base.Draw(gameTime);

            float scaleVertFactor = (float)GraphicsDevice.Viewport.Height / GameWorld.WorldHeight;
            float scaleHorzFactor = (float)GraphicsDevice.Viewport.Width / GameWorld.WorldLength;

            DrawStraight(world, gameTime, scaleHorzFactor, scaleVertFactor, false, 1f, 0f);
        }


    }
}
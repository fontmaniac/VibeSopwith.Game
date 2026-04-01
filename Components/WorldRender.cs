using VibeSopwith.Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Graphics;

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

        private RenderTarget2D _upscaleTarget = null!;
        private RenderTarget2D _postTarget = null!;
        private Effect _postEffect = null!;


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
            _approachRender = new ApproachRender(Game);

            //_postEffect = Game.Content.Load<Effect>("Shaders/PostGreenOnBlack");
            //_postEffect = Game.Content.Load<Effect>("Shaders/PostBarrelDistortion");
            _postEffect = Game.Content.Load<Effect>("Shaders/PostBDandGOB");
            //_postEffect = Game.Content.Load<Effect>("Shaders/PostPixelated");

            _postTarget = EnsurePostTarget(_postTarget);
            _upscaleTarget = EnsurePostTarget(_upscaleTarget);
        }

        private RenderTarget2D EnsurePostTarget(RenderTarget2D postTarget)
        {
            var vp = GraphicsDevice.Viewport;
            if (postTarget == null || postTarget.Width != vp.Width || postTarget.Height != vp.Height)
            {
                postTarget?.Dispose();
                postTarget = new RenderTarget2D(
                    GraphicsDevice,
                    GraphicsDevice.Viewport.Width,
                    GraphicsDevice.Viewport.Height,
                    false,
                    SurfaceFormat.Color,
                    DepthFormat.None
                );
            }

            return postTarget;
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

        private void DrawStraight(GameWorld world, GameTime gameTime, float scaleHorz, float scaleVert, bool drawBullets, float groundThicknessPx, float cameraPositionX, Vector2 worldPixelSize)
        {
            // 1. Scale: X normal, Y flipped
            var scale = Matrix.CreateScale(scaleHorz, -scaleVert, 1f);

            // 2. Move world origin (0,0) to bottom-left of screen
            var translateY = Matrix.CreateTranslation(0f, GraphicsDevice.Viewport.Height, 0f);

            // 3. Camera X
            var camera = new Vector2(-cameraPositionX, 0f).SnapToPixel(worldPixelSize);
            var translateCamera = Matrix.CreateTranslation(camera.X, 0, 0f);

            // Final: world -> camera -> scale+flip -> move to screen
            var transform = translateCamera * scale * translateY;

            TheGame.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp, 
                DepthStencilState.None,
                RasterizerState.CullNone, 
                null,
                transform);

            _groundRender.Draw(world.Ground, groundThicknessPx, scaleVert, transform);
            //_bodyRender.Draw(world.Ceiling.Body, gameTime);

            foreach (var approach in world.Approaches)
                _approachRender.Draw(approach, gameTime, null);

            foreach (var building in world.Buildings)
            {
                _buildingRender.Draw(building, gameTime);
                //_bodyRender.Draw(building.Body, gameTime);
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
            //_bodyRender.Draw(world.Ground.Body, gameTime);


            TheGame.SpriteBatch.End();
        }


        public void Draw(GameWorld world, GameTime gameTime, float cameraPositionX)
        {
            base.Draw(gameTime);

            var vp = GraphicsDevice.Viewport;
            _upscaleTarget = EnsurePostTarget(_upscaleTarget);
            _postTarget = EnsurePostTarget(_postTarget);

            GraphicsDevice.SetRenderTarget(_postTarget);
            GraphicsDevice.Clear(Color.Black);

            var destination = new Rectangle(0, 0, vp.Width, vp.Height);
            var height = vp.Height/1;
            var source = new Rectangle(0, 0, (int)(height * vp.AspectRatio), (int)height);
            GraphicsDevice.Viewport = new Viewport(source.X, source.Y, source.Width, source.Height);

            float scaleVertFactor = (float)GraphicsDevice.Viewport.Height / GameWorld.WorldHeight; // Screen pixels per world unit
            float worldPixelSize = (1f / scaleVertFactor) * 1f; // World units per virtual pixel

            DrawStraight(world, gameTime, scaleVertFactor, scaleVertFactor, true, 4f, cameraPositionX, new Vector2(worldPixelSize, worldPixelSize));

            GraphicsDevice.SetRenderTarget(_upscaleTarget);
            GraphicsDevice.Clear(Color.Black);

            //_postEffect.Parameters["worldToScreenScale"].SetValue(scaleVertFactor);
            //_postEffect.Parameters["cameraWorldPos"].SetValue(new Vector2(cameraPositionX, 0f));
            //_postEffect.Parameters["worldPixelSize"].SetValue(new Vector2(worldPixelSize, worldPixelSize));
            //_postEffect.Parameters["screenSize"].SetValue(new Vector2(vp.Width, vp.Height));

            TheGame.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null);
            TheGame.SpriteBatch.Draw(_postTarget, destination, source, Color.White);
            TheGame.SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.Viewport = vp;

            TheGame.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect: null);
            TheGame.SpriteBatch.Draw(_upscaleTarget, Vector2.Zero, Color.White);
            TheGame.SpriteBatch.End();
        }

        public void DrawMinimap(GameWorld world, GameTime gameTime)
        {
            base.Draw(gameTime);

            float scaleVertFactor = (float)GraphicsDevice.Viewport.Height / GameWorld.WorldHeight;
            float scaleHorzFactor = (float)GraphicsDevice.Viewport.Width / GameWorld.WorldLength;

            DrawStraight(world, gameTime, scaleHorzFactor, scaleVertFactor, false, 1f, 0f, Vector2.One);
        }


    }
}
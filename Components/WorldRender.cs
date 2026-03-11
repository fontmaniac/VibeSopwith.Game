using VibeSopwith.Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class WorldRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private RenderTarget2D _rt = null!;
        private GroundRender _groundRender = null!; 
        private AirplaneRender _airplaneRender = null!;

        public override void Initialize()
        {
            base.Initialize();
        }

        public new void LoadContent()
        {
            base.LoadContent();

            EnsureRenderTarget();

            _groundRender = new GroundRender(Game); 
            _groundRender.LoadContent(GraphicsDevice);

            _airplaneRender = new AirplaneRender(Game);
            _airplaneRender.LoadContent();
        }

        protected override void UnloadContent()
        {
            _airplaneRender?.Dispose();
            _groundRender?.Dispose();
        }

        void EnsureRenderTarget()
        {
            var vp = GraphicsDevice.Viewport;
            if (_rt == null || _rt.Width != vp.Width || _rt.Height != vp.Height)
            {
                _rt?.Dispose();
                _rt = new RenderTarget2D(GraphicsDevice, vp.Width, vp.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }
        }


        public void Draw(GameWorld world, GameTime gameTime, float cameraPositionX)
        {
            base.Draw(gameTime);

            float scaleFactor = (float)GraphicsDevice.Viewport.Height / GameWorld.WorldHeight;

            var translation = Matrix.CreateTranslation(-cameraPositionX, 0, 0);
            var scale = Matrix.CreateScale(scaleFactor, scaleFactor, 1);

            // Final transform
            var transform = translation * scale;

            // Main world render pass; rendering into "global" render target.
            var vp = GraphicsDevice.Viewport;
            EnsureRenderTarget();
            GraphicsDevice.SetRenderTarget(_rt);
            GraphicsDevice.Clear(Color.Black);

            TheGame.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, transform);
            
            _groundRender.Draw(world.Ground, thickness: 0.2f, TheGame.SpriteBatch);
            _airplaneRender.Draw(world.Plane, gameTime);

            TheGame.SpriteBatch.End();

            // Screen-render pass - paste _rt flipped vertically.
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);  // Without this the screen becomes "cornflower".
            GraphicsDevice.Viewport = vp;

            TheGame.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null);

            TheGame.SpriteBatch.Draw(
                texture: _rt,
                destinationRectangle: new Rectangle(0, 0, _rt.Width, _rt.Height),
                sourceRectangle: null,
                color: Color.White, rotation: 0f, origin: Vector2.Zero,
                effects: SpriteEffects.FlipVertically,
                layerDepth: 0f);

            TheGame.SpriteBatch.End();
        }
    }
}
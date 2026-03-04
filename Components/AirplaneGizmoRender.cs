using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class AirplaneGizmoRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private RenderTarget2D _rt = null!;
        private Texture2D _airplaneTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            EnsureRenderTarget();

            using var tex1 = Game.Content.Load<Texture2D>("Textures\\Plane_2.png");
            _airplaneTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex1);

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

        public void PreparedDraw(Airplane airplane)
        {
            var scaleFactor = (float)GraphicsDevice.Viewport.Height / TheGame.PlaneGizmoHeight;
            var scale = Matrix.CreateScale(scaleFactor, scaleFactor, 1);

            // Final transform
            var transform = scale;

            // Main world render pass; rendering into "global" render target.
            var vp = GraphicsDevice.Viewport;
            EnsureRenderTarget();
            GraphicsDevice.SetRenderTarget(_rt);
            GraphicsDevice.Clear(Color.Black);

            TheGame.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, transform);

            DrawHelper.DrawCentered(
                Centered.OffInterface(airplane) with { Position = new Vector2(TheGame.PlaneGizmoWidth / 2f, TheGame.PlaneGizmoHeight / 2f) },
                _airplaneTexture,
                TheGame.SpriteBatch);

            TheGame.SpriteBatch.End();

            // Screen-render pass - paste _rt flipped vertically.
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Viewport = vp;

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // Screen-render pass - paste _rt flipped vertically.

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

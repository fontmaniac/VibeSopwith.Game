using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nage.Strata.Abstractions.Spatial;
using Nage.Strata.Graphics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class AirplaneGizmoRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _airplaneTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _airplaneTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>("Textures\\Plane_1.png")).ToAtlas(Atlas.OriginCentered).ToLRSlice();

        }

        public void Draw(Airplane airplane, GameTime gameTime)
        {
            base.Draw(gameTime);

            var scaleFactor = (float)GraphicsDevice.Viewport.Height / TheGame.PlaneGizmoHeight;

            var scale = Matrix.CreateScale(scaleFactor, -scaleFactor, 1);

            // 2. Move world origin (0,0) to bottom-left of screen
            var translateY = Matrix.CreateTranslation(0f, GraphicsDevice.Viewport.Height, 0f);

            // Final transform
            var transform = scale * translateY;

            TheGame.SpriteBatchPoint.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                transform);

            DrawHelper.DrawSlice(
                Location.Capture(airplane) with { Position = new Vector2(TheGame.PlaneGizmoWidth / 2f, TheGame.PlaneGizmoHeight / 2f) },
                _airplaneTexture,
                TheGame.SpriteBatchPoint);

            TheGame.SpriteBatchPoint.End();

        }
    }
}

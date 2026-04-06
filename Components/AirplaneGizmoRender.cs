using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class AirplaneGizmoRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private HandedSlice.LR _airplaneTexture = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _airplaneTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, Game.Content.Load<Texture2D>("Textures\\Plane_1.png")).ToAtlas(Atlas.OriginCentered).ToLRSlice();

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

            TheGame.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                transform);

            DrawHelper.DrawSlice(
                Location.OffInterface(airplane) with { Position = new Vector2(TheGame.PlaneGizmoWidth / 2f, TheGame.PlaneGizmoHeight / 2f) },
                _airplaneTexture,
                TheGame.SpriteBatch);

            TheGame.SpriteBatch.End();

        }
    }
}

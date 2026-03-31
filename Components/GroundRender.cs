using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class GroundRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _groundTexture = null!;

        private VertexPositionColorTexture[] _quadVertsTex = new VertexPositionColorTexture[6];

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            var tex = Game.Content.Load<Texture2D>("Textures\\Rock_Tile.png");
            _groundTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex);
        }

        public void Draw(Ground ground, float thickness, float scaleVert, Matrix transform)
        {
            var gd = Game.GraphicsDevice;

            var effect = TheGame.BasicEffect;
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = true;
            effect.LightingEnabled = false;

            effect.World = transform;
            effect.View = Matrix.Identity;
            //Same projection SpriteBatch uses internally
            effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, gd.Viewport.Width,
                gd.Viewport.Height, 0,
                0, 1
            );

            for (int i = 1; i < ground.Points.Count; i++)
            {
                var start = ground.Points[i - 1];
                var end = ground.Points[i];

                FillUnderLineTexture(gd, start, end, 0, Color.White, _groundTexture, 8);
                FillUnderLineTexture(gd, start, end, GameWorld.WorldHeight, Color.Black, TheGame.Primitives.Pixel, 64);
                TheGame.Primitives.DrawLine(start, end, Color.White, thickness/scaleVert);
            }
        }

        private void FillUnderLineTexture(GraphicsDevice gd, Vector2 p1, Vector2 p2, float baseLine, Color color, Texture2D texture, float tileSizeWorld)
        {
            var tile = tileSizeWorld; // world units per texture tile

            Vector3 v1 = new(p1.X, p1.Y, 0);
            Vector3 v2 = new(p2.X, p2.Y, 0);
            Vector3 v3 = new(p2.X, baseLine, 0);
            Vector3 v4 = new(p1.X, baseLine, 0);

            // UVs based on world coordinates
            Vector2 uv1 = new(p1.X / tile, p1.Y / tile);
            Vector2 uv2 = new(p2.X / tile, p2.Y / tile);
            Vector2 uv3 = new(p2.X / tile, baseLine / tile);
            Vector2 uv4 = new(p1.X / tile, baseLine / tile);

            // Two triangles
            _quadVertsTex[0] = new VertexPositionColorTexture(v1, color, uv1);
            _quadVertsTex[1] = new VertexPositionColorTexture(v2, color, uv2);
            _quadVertsTex[2] = new VertexPositionColorTexture(v3, color, uv3);

            _quadVertsTex[3] = new VertexPositionColorTexture(v3, color, uv3);
            _quadVertsTex[4] = new VertexPositionColorTexture(v4, color, uv4);
            _quadVertsTex[5] = new VertexPositionColorTexture(v1, color, uv1);

            gd.SamplerStates[0] = SamplerState.PointWrap;
            var effect = TheGame.BasicEffect;
            effect.Texture = texture;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    _quadVertsTex,
                    0,
                    2
                );
            }
        }

    }
}
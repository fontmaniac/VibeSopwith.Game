using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class GroundRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _pixel = null!;
        private Texture2D _pixelBlue = null!;
        private Texture2D _pixelBlack = null!;
        private Texture2D _groundTexture = null!;

        private BasicEffect _effect = null!;
        private VertexPositionColorTexture[] _quadVertsTex = new VertexPositionColorTexture[6];

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _pixelBlue = new Texture2D(graphicsDevice, 1, 1);
            _pixelBlue.SetData(new[] { Color.DarkBlue });

            _pixelBlack = new Texture2D(graphicsDevice, 1, 1);
            _pixelBlack.SetData(new[] { Color.Black });

            var tex = Game.Content.Load<Texture2D>("Textures\\Rock_Tile.png");
            _groundTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex);

            _effect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = false,
                TextureEnabled = true,
                LightingEnabled = false,
                
            };

            _effect.VertexColorEnabled = false;
        }

        public void Draw(Ground ground, float thickness, float scaleVert, SpriteBatch spriteBatch, Matrix transform)
        {
            var gd = Game.GraphicsDevice;

            _effect.World = transform;
            _effect.View = Matrix.Identity;

            //Same projection SpriteBatch uses internally
            _effect.Projection = Matrix.CreateOrthographicOffCenter(
                0, gd.Viewport.Width,
                gd.Viewport.Height, 0,
                0, 1
            );

            for (int i = 1; i < ground.Points.Count; i++)
            {
                var start = ground.Points[i - 1];
                var end = ground.Points[i];

                FillUnderLineTexture(gd, start, end, 0, _pixelBlack, 64);
                FillUnderLineTexture(gd, start, end, GameWorld.WorldHeight, _pixelBlack, 64);
                DrawLine(spriteBatch, start, end, Color.White, thickness, scaleVert);
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 point1, Vector2 point2, Color color, float thicknessPx, float scaleVert)
        {
            float dx = point2.X - point1.X;
            float dy = point2.Y - point1.Y;

            float distanceWorld = Vector2.Distance(point1, point2);
            float angle = MathF.Atan2(dy, dx);

            Vector2 scale = new(distanceWorld, thicknessPx/scaleVert);

            spriteBatch.Draw(_pixel, point1, null, color, angle, new Vector2(0, 0.5f), scale, SpriteEffects.None, 0f);
        }

        private void FillUnderLineTexture(GraphicsDevice gd, Vector2 p1, Vector2 p2, float baseLine, Texture2D texture, float tileSizeWorld)
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
            _quadVertsTex[0] = new VertexPositionColorTexture(v1, Color.White, uv1);
            _quadVertsTex[1] = new VertexPositionColorTexture(v2, Color.White, uv2);
            _quadVertsTex[2] = new VertexPositionColorTexture(v3, Color.White, uv3);

            _quadVertsTex[3] = new VertexPositionColorTexture(v3, Color.White, uv3);
            _quadVertsTex[4] = new VertexPositionColorTexture(v4, Color.White, uv4);
            _quadVertsTex[5] = new VertexPositionColorTexture(v1, Color.White, uv1);

            gd.SamplerStates[0] = SamplerState.PointWrap;
            _effect.Texture = texture;

            foreach (var pass in _effect.CurrentTechnique.Passes)
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
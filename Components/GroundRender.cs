using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class GroundRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _groundTexture = null!;
        private Texture2D _skyTexture = null!;

        private VertexPositionColorTexture[] _quadVertsTex = new VertexPositionColorTexture[0];
        private Guid _lastGroundHash = Guid.Empty;

        public void LoadContent(GraphicsDevice graphicsDevice)
        {
            var tex1 = Game.Content.Load<Texture2D>("Textures\\Rock_Tile.png");
            _groundTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex1);
            var tex2 = Game.Content.Load<Texture2D>("Textures\\Skybox_1.png");
            _skyTexture = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex2);
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

            void singlePass(Action<int, Vector2, Vector2> execute)
            {
                for (int i = 1; i < ground.Points.Count; i++)
                {
                    var start = ground.Points[i - 1];
                    var end = ground.Points[i];

                    execute(i-1, start, end);
                }
            }

            // Allocate/Reallocate _quadVertsTex according to the number of segments, if necessary.
            var triCount = (ground.Points.Count - 1) * 2;
            var vertCount = triCount * 3 * 2;
            if (_lastGroundHash != ground.Hash)
            {
                if (_quadVertsTex.Length != vertCount)
                    _quadVertsTex = new VertexPositionColorTexture[vertCount];
                singlePass((i, start, end) => FillUnderLineTexture(i, start, end, 0, Color.White, new Vector2(8, 8)));
                //singlePass((i, start, end) => FillUnderLineTexture(i + triCount / 2, start, end, GameWorld.WorldHeight, Color.Black, new Vector2(64, 64)));
                singlePass((i, start, end) => FillUnderLineTexture(i + triCount / 2, start, end, GameWorld.WorldHeight, Color.White, new Vector2(GameWorld.WorldLength, GameWorld.WorldHeight)));
            }
            _lastGroundHash = ground.Hash;

            DrawTextures(gd, _groundTexture, 0);
            DrawTextures(gd, _skyTexture, _quadVertsTex.Length/2);

            singlePass((_, start, end) => TheGame.Primitives.DrawLine(start, end, Color.White, thickness / scaleVert));
        }

        private void FillUnderLineTexture(int i, Vector2 p1, Vector2 p2, float baseLine, Color color, Vector2 tileSizeWorld)
        {
            var tile = tileSizeWorld; // world units per texture tile

            Vector3 v1 = new(p1.X, p1.Y, 0);
            Vector3 v2 = new(p2.X, p2.Y, 0);
            Vector3 v3 = new(p2.X, baseLine, 0);
            Vector3 v4 = new(p1.X, baseLine, 0);

            // UVs based on world coordinates. "Minus-Y" because our world is Y-flipped.
            Vector2 uv1 = new(p1.X / tile.X, p1.Y / -tile.Y);
            Vector2 uv2 = new(p2.X / tile.X, p2.Y / -tile.Y);
            Vector2 uv3 = new(p2.X / tile.X, baseLine / -tile.Y);
            Vector2 uv4 = new(p1.X / tile.X, baseLine / -tile.Y);

            // Two triangles
            i = i * 6;
            _quadVertsTex[i+0] = new VertexPositionColorTexture(v1, color, uv1);
            _quadVertsTex[i+1] = new VertexPositionColorTexture(v2, color, uv2);
            _quadVertsTex[i+2] = new VertexPositionColorTexture(v3, color, uv3);

            _quadVertsTex[i+3] = new VertexPositionColorTexture(v3, color, uv3);
            _quadVertsTex[i+4] = new VertexPositionColorTexture(v4, color, uv4);
            _quadVertsTex[i+5] = new VertexPositionColorTexture(v1, color, uv1);
        }

        private void DrawTextures(GraphicsDevice gd, Texture2D texture, int offset)
        {
            gd.SamplerStates[0] = SamplerState.PointWrap;
            var effect = TheGame.BasicEffect;
            effect.Texture = texture;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserPrimitives(
                    PrimitiveType.TriangleList,
                    _quadVertsTex,
                    offset,
                    _quadVertsTex.Length / 2 / 3
                );
            }
        }

    }
}
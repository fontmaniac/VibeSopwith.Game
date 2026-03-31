using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Components
{
    public class PrimitivesRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        public Texture2D Pixel = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 0.05f)
        {
            var edge = end - start;
            var angle = MathF.Atan2(edge.Y, edge.X);

            TheGame.SpriteBatch.Draw(
                Pixel,
                start,
                null,
                color,
                angle,
                new Vector2(0, 0.5f),
                new Vector2(edge.Length(), thickness),
                SpriteEffects.None,
                0f
            );
        }

        public void DrawFilledCirclePrimitive(Vector2 center, float radius, Color color, int segments = 32)
        {
            var verts = new List<VertexPositionColor>();

            // Center vertex
            var center3D = new Vector3(center, 0);
            for (int i = 0; i < segments; i++)
            {
                float angle1 = MathHelper.TwoPi * i / segments;
                float angle2 = MathHelper.TwoPi * (i + 1) / segments;

                var p1 = new Vector3(center.X + radius * MathF.Cos(angle1), center.Y + radius * MathF.Sin(angle1), 0);
                var p2 = new Vector3(center.X + radius * MathF.Cos(angle2), center.Y + radius * MathF.Sin(angle2), 0);

                verts.Add(new VertexPositionColor(center3D, color));
                verts.Add(new VertexPositionColor(p1, color));
                verts.Add(new VertexPositionColor(p2, color));
            }

            using var vb = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), verts.Count, BufferUsage.WriteOnly);
            vb.SetData(verts.ToArray());

            GraphicsDevice.SetVertexBuffer(vb);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            TheGame.BasicEffect.World = Matrix.Identity;
            TheGame.BasicEffect.View = Matrix.Identity;
            TheGame.BasicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
            TheGame.BasicEffect.VertexColorEnabled = true;
            TheGame.BasicEffect.LightingEnabled = false;

            foreach (var pass in TheGame.BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, verts.Count / 3);
            }
        }

        public void DrawCirclePrimitive(Vector2 center, float radius, Color color, float thickness, int segments = 32)
        {
            var verts = new Vector2[segments];

            for (var i = 0; i < segments; ++i)
            {
                var angle1 = MathHelper.TwoPi * i / segments;
                verts[i] = new Vector2(center.X + radius * MathF.Cos(angle1), center.Y + radius * MathF.Sin(angle1));
            }

            for (var i = 0; i < segments; ++i)
            {
                var p1 = i == 0 ? verts[^1] : verts[i-1];
                var p2 = verts[i];
                DrawLine(p1, p2, color, thickness);
            }
        }

    }
}

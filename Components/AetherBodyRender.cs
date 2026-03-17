using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game.Components
{
    public class AetherBodyRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private Texture2D _pixel = null!;

        public new void LoadContent()
        {
            base.LoadContent();

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

        }
        public void Draw(Body body, GameTime gameTime)
        {
            foreach (var fixture in body.FixtureList)
            {
                DrawFixture(fixture, Color.Red);
            }

        }

        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            var edge = end - start;
            var angle = (float)Math.Atan2(edge.Y, edge.X);
            TheGame.SpriteBatch.Draw(_pixel, start, null, color, angle, Vector2.Zero, new Vector2(edge.Length(), 0.02f), SpriteEffects.None, 0f);
        }


        private void DrawFixture(Fixture fixture, Color color)
        {
            if (fixture.Shape is PolygonShape poly)
            {
                var body = fixture.Body;
                var transform = Matrix.CreateRotationZ(body.Rotation) * Matrix.CreateTranslation(body.Position.X, body.Position.Y, 0f);

                foreach (var vertex in poly.Vertices)
                {
                    var worldPos = Vector2.Transform(new Vector2(vertex.X, vertex.Y), transform);
                    TheGame.SpriteBatch.Draw(_pixel, worldPos, null, color, 0f, Vector2.Zero, 0.05f, SpriteEffects.None, 0f);
                }

                // Optionally draw edges
                for (int i = 0; i < poly.Vertices.Count; i++)
                {
                    var a = Vector2.Transform(new Vector2(poly.Vertices[i].X, poly.Vertices[i].Y), transform);
                    var b = Vector2.Transform(new Vector2(poly.Vertices[(i + 1) % poly.Vertices.Count].X, poly.Vertices[(i + 1) % poly.Vertices.Count].Y), transform);
                    DrawLine(a, b, color);
                }
            }
        }

    }
}

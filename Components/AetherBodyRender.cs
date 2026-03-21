using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

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
            TheGame.SpriteBatch.Draw(_pixel, start, null, color, angle, Vector2.Zero, new Vector2(edge.Length(), 0.05f), SpriteEffects.None, 0f);
        }


        private void DrawFixture(Fixture fixture, Color color)
        {
            if (fixture.Shape is PolygonShape poly)
            {
                var body = fixture.Body;

                foreach (var vertex in poly.Vertices)
                {
                    var worldPos = fixture.Body.GetWorldPoint(vertex).ToXna();
                    TheGame.SpriteBatch.Draw(_pixel, worldPos, null, color, 0f, Vector2.Zero, 0.05f, SpriteEffects.None, 0f);
                }

                // Optionally draw edges
                for (int i = 0; i < poly.Vertices.Count; i++)
                {
                    var a = fixture.Body.GetWorldPoint(poly.Vertices[i]).ToXna();
                    var b = fixture.Body.GetWorldPoint(poly.Vertices[(i + 1) % poly.Vertices.Count]).ToXna();   
                    DrawLine(a, b, color);
                }
            }
        }

    }
}

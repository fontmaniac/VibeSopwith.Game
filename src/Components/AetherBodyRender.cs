using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nage.Strata.Physics;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game.Components
{
    public class AetherBodyRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        public void Draw(Body? body, GameTime gameTime)
        {
            if (body == null) return;
            foreach (var fixture in body.FixtureList)
            {
                DrawFixture(fixture, Color.Red);
            }

        }

        private void DrawFixture(Fixture fixture, Color color)
        {
            if (fixture.Shape is PolygonShape poly)
            {
                var body = fixture.Body;

                foreach (var vertex in poly.Vertices)
                {
                    var worldPos = fixture.Body.GetWorldPoint(vertex).ToXna();
                    TheGame.SpriteBatchPoint.Draw(TheGame.Primitives.Pixel, worldPos, null, color, 0f, new Vector2(0.5f, 0.5f), 0.1f, SpriteEffects.None, 0f);
                }

                for (int i = 0; i < poly.Vertices.Count; i++)
                {
                    var a = fixture.Body.GetWorldPoint(poly.Vertices[i]).ToXna();
                    var b = fixture.Body.GetWorldPoint(poly.Vertices[(i + 1) % poly.Vertices.Count]).ToXna();   
                    TheGame.Primitives.DrawLine(a, b, color);
                }
            }
        }

    }
}

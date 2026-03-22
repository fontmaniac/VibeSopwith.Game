using VibeSopwith.Game.Utils;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class Ground 
    {
        public readonly List<Vector2> Points;
        public Body Body = null!;

        private Ground(List<Vector2> points)
        {
            Points = points;
        }

        public static Ground MakeRandom()
        {
            var points = new List<Vector2>();

            points.Add(new Vector2(0, GameWorld.WorldHeight));

            for (int i = 0; i <= 20; i++) 
            {
                var x = GameWorld.WorldLength * 0.025f + (GameWorld.WorldLength * 0.95f) * (i / 20.0f); 
                var y = (float)(GameWorld.WorldSeed.NextDouble() * (GameWorld.WorldHeight * 0.5) + (GameWorld.WorldHeight * 0.25));
                points.Add(new Vector2(x, y));
            }

            points.Add(new Vector2(GameWorld.WorldLength, GameWorld.WorldHeight));

            return new Ground(points);
        }

        public static Ground MakeFlat(float heightPercentage)
        {
            var points = new List<Vector2>();

            for (int i = 0; i <= 20; i++) 
            {
                var x = GameWorld.WorldLength * (i / 20.0f); 
                var y = (GameWorld.WorldHeight * heightPercentage);
                points.Add(new Vector2(x, y));
            }

            return new Ground(points);
        }

        public void SetupRigging(World collisionWorld)
        {
            var groundBody = collisionWorld.CreateBody(Aether.Vector2.Zero, 0f, BodyType.Static);
            groundBody.Tag = this;
            groundBody.IgnoreGravity = true;
            groundBody.Mass = float.MaxValue;

            for (var i = 0; i < Points.Count-1; ++i)
            {
                var p1 = Points[i];
                var p2 = Points[i+1];
                var bottomLeft = new Vector2(p1.X, 0);
                var topLeft = new Vector2(p1.X, p1.Y);
                var topRight = new Vector2(p2.X, p2.Y);
                var bottomRight = new Vector2(p2.X, 0);
                var vertices = new Aether.Vertices();
                vertices.Add(bottomLeft.ToAether());
                vertices.Add(topLeft.ToAether());
                vertices.Add(topRight.ToAether());
                vertices.Add(bottomRight.ToAether());

                var shape = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices, 1.0f);
                var fixture = groundBody.CreateFixture(shape);
                fixture.CollisionCategories = Category.Cat2;
            }

            Body = groundBody;
        }
    }
}
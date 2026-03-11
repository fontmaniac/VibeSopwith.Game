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

            for (int i = 0; i <= 20; i++) 
            {
                var x = GameWorld.WorldLength * (i / 20.0f); 
                var y = (float)(GameWorld.WorldSeed.NextDouble() * (GameWorld.WorldHeight * 0.5) + (GameWorld.WorldHeight * 0.25));
                points.Add(new Vector2(x, y));
            }

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
            var vertices = new Aether.Vertices();

            foreach (var point in Points)
                vertices.Add(point.ToAether());

            vertices.Add(Vector2.UnitX.ToAether() * GameWorld.WorldLength);
            vertices.Add(Vector2.Zero.ToAether());

            var groundBody = collisionWorld.CreateBody(Aether.Vector2.Zero, 0f, BodyType.Static);
            groundBody.Tag = this;

            var shape = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices, 1.0f);
            var fixture = groundBody.CreateFixture(shape);

            Body = groundBody;
        }
    }
}
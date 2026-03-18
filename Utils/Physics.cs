using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Utils
{
    internal static class Physics
    {
        public static Fixture CreateRectangle(this Body body, float width, float height, float density, Aether.Vector2 offset1, float angle, Aether.Vector2 offset2)
        {
            Aether.Vertices vertices = Aether.PolygonTools.CreateRectangle(width / 2f, height / 2f);
            vertices.Translate(ref offset1);
            vertices.Rotate(angle);
            vertices.Translate(ref offset2);
            PolygonShape shape = new PolygonShape(vertices, density);
            return body.CreateFixture(shape);
        }

        public static Vector2 ToXna(this Aether.Vector2 v) => new Vector2(v.X, v.Y);
        public static Aether.Vector2 ToAether(this Vector2 v) => new Aether.Vector2(v.X, v.Y);

        public static float ToAngle(this Vector2 v) => (float)Math.Atan2(v.Y, v.X);

        public static Vector2 ToNormal(this float radAngle) => new Vector2((float)Math.Cos(radAngle), (float)Math.Sin(radAngle));
    }
}

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

        // Point A in 2D space (Vector2(ax, ay));
        // Vector D in the same space (Vector2(dx, dy));
        // Point P in the same space (Vector2(px, py)).
        // I need a function that would "mirror" point P over the straight line represented by points A and A+D.
        public static Vector2 ReflectPointAcrossLine(this Vector2 P, Vector2 A, Vector2 D)
        {
            // Normal to the line direction
            Vector2 n = new Vector2(-D.Y, D.X);

            // Vector from A to P
            Vector2 v = P - A;

            float nLenSq = n.LengthSquared();
            if (nLenSq == 0f)
                return P; // Degenerate line: no direction

            // Signed distance from P to the line along the normal
            float dist = Vector2.Dot(v, n) / nLenSq;

            // Reflection: subtract twice the normal component
            return P - 2f * dist * n;
        }

        public static Vector2 Rotate(this Vector2 orig, float radAngle) => Vector2.Transform(orig, Matrix.CreateRotationZ(radAngle));

        public static Fixture ToPolygon(this (float x, float y)[] srcVertices, Body body, float density = 1.0f)
        {
            var vertices = new Aether.Vertices(srcVertices.Select(t => new Aether.Vector2(t.x, t.y)));
            return body.CreatePolygon(vertices, density);
        }

    }
}

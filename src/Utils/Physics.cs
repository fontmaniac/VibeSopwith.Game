using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
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

        public static Fixture ToPolygon(this (float x, float y)[] srcVertices, Body body, float density = 1.0f)
        {
            var vertices = new Aether.Vertices(srcVertices.Select(t => new Aether.Vector2(t.x, t.y)));
            return body.CreatePolygon(vertices, density);
        }

        private static Aether.Vector2 ResolveContactPoint(Contact ct, string contactType)
        {
            ct.GetWorldManifold(out var normal, out var points);
            var contactPoint = points[0];
            Console.WriteLine($"{contactType} collision detected at {contactPoint.X} - {contactPoint.Y}. Total points {ct.Manifold.PointCount}");
            return contactPoint;
        }

        public static bool OnCollision<TFix1, TFix2>(
            Contact ct,
            string collisionType,
            Func<TFix1, bool> check1,
            Func<TFix2, bool> check2,
            Action<Aether.Vector2, Fixture, Fixture, TFix1, TFix2> execute)
        {
            if (!ct.IsTouching) return false;

            bool exists<T>(object[] arr, Func<T, bool> check, out T? found)
            {
                found = default(T);
                var result = arr.FirstOrDefault(x => x is T fix && check(fix));
                if (result != null) { found = (T)result; return true; }
                return false;
            }

            (bool, T?) castAndCheck<T>(Fixture fix, Func<T, bool> check) => 
                fix.Body.Tag is T fixSingle && check(fixSingle) ? (true, fixSingle) : 
                fix.Body.Tag is object[] arr && exists(arr, check, out var fixElement) ? (true, fixElement) : 
                (false, default(T));

            (TFix1 fix1, TFix2 fix2, Fixture aFix1, Fixture aFix2)? tryPair(Fixture fa, Fixture fb)
            {
                var (ok1, v1) = castAndCheck<TFix1>(fa, check1);
                if (!ok1) return null;

                var (ok2, v2) = castAndCheck<TFix2>(fb, check2);
                return ok2 ? (v1!, v2!, fa, fb) : null;
            }

            var fixtures =
                tryPair(ct.FixtureA, ct.FixtureB) ??
                tryPair(ct.FixtureB, ct.FixtureA);

            if (fixtures == null) return false;

            string formatCollision()
            {
                var d1 = fixtures.Value.fix1 as IDescribeMyself;
                var d2 = fixtures.Value.fix2 as IDescribeMyself;

                return (d1, d2) switch
                {
                    (not null, not null) => string.Format(collisionType, d1.WhoAmI, d2.WhoAmI),
                    (not null, null)     => string.Format(collisionType, d1.WhoAmI),
                    (null, not null)     => string.Format(collisionType, d2.WhoAmI),
                    _ => collisionType
                };
            }

            collisionType = formatCollision();

            var cp = ResolveContactPoint(ct, collisionType);
            execute(cp, fixtures.Value.aFix1, fixtures.Value.aFix2, fixtures.Value.fix1, fixtures.Value.fix2);

            return true;
        }

    }
}

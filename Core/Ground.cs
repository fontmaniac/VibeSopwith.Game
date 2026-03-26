using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;
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

        public enum Units { Pct, Met }     // Met - world units, "Meters"; Pct - percentage of the relevant overall measurement - GameWorld.Height or GameWorld.Length

        public abstract record HOffset(float Value, Units Units)
        {
            public sealed record OffLeft(float Value, Units Units) : HOffset(Value, Units);
            public sealed record OffRight(float Value, Units Units) : HOffset(Value, Units);
            public sealed record OffPrev(float Value, Units Units) : HOffset(Value, Units);
        }

        public abstract record VOffset(float Value, Units Units)
        {
            public sealed record OffFloor(float Value, Units Units) : VOffset(Value, Units);
            public sealed record OffCeiling(float Value, Units Units) : VOffset(Value, Units);
            public sealed record OffPrev(float Value, Units Units) : VOffset(Value, Units);
        }

        private static float Resolve(Units units, float baseValue, float val, float full, float factor) =>
            units switch
            {
                Units.Met => baseValue + val,
                Units.Pct => baseValue + full * (val / 100f) * factor,
                _ => throw new ArgumentOutOfRangeException()
            };

        private static float Resolve(HOffset o, Vector2? prev) => o switch
            {
                HOffset.OffPrev  => Resolve(o.Units, prev!.Value.X,         o.Value, GameWorld.WorldLength, 1f),
                HOffset.OffLeft  => Resolve(o.Units, 0f,                    o.Value, GameWorld.WorldLength, 1f),
                HOffset.OffRight => Resolve(o.Units, GameWorld.WorldLength, o.Value, GameWorld.WorldLength, -1f),
                _ => throw new ArgumentOutOfRangeException()
            };

        private static float Resolve(VOffset o, Vector2? prev) => o switch
        {
            VOffset.OffPrev    => Resolve(o.Units, prev!.Value.Y,         o.Value, GameWorld.WorldHeight, 1f),
            VOffset.OffFloor   => Resolve(o.Units, 0f,                    o.Value, GameWorld.WorldHeight, 1f),
            VOffset.OffCeiling => Resolve(o.Units, GameWorld.WorldHeight, o.Value, GameWorld.WorldHeight, -1f),
            _ => throw new ArgumentOutOfRangeException()
        };

        private sealed class GroundBuilder
        {
            private readonly List<Vector2> _points = new();
            private Vector2? _prev = null;

            public GroundBuilder()
            {
            }

            public GroundBuilder Segment(HOffset x, VOffset y)
            {
                _prev = new Vector2(Resolve(x, _prev), Resolve(y, _prev));
                _points.Add(_prev!.Value);
                return this;
            }

            public GroundBuilder SegmentFlat(HOffset x)
            {
                _prev = new Vector2(Resolve(x, _prev), _prev!.Value.Y);
                _points.Add(_prev!.Value);
                return this;
            }

            public Ground Build() => new Ground(_points);
        }

        private static HOffset.OffLeft OffLeft(float v, Units u) => new HOffset.OffLeft(v, u);
        private static HOffset.OffRight OffRight(float v, Units u) => new HOffset.OffRight(v, u);
        private static HOffset.OffPrev OffPrev(float v, Units u) => new HOffset.OffPrev(v, u);

        private static VOffset.OffFloor OffFloor(float v, Units u) => new VOffset.OffFloor(v, u);
        private static VOffset.OffCeiling OffCeiling(float v, Units u) => new VOffset.OffCeiling(v, u);
        private static VOffset.OffPrev OffPrevY(float v, Units u) => new VOffset.OffPrev(v, u);

        private bool GetSegmentAtOffset(HOffset.OffLeft offset, out (Vector2 p1, Vector2 pm, Vector2 p2) result)
        {
            result = default;

            float x = Resolve(offset, null);

            var pts = Points;
            int n = pts.Count;
            if (n < 2) return false;

            const float eps = 1e-5f;

            // --- 1. Detect strictly vertical segments at X = x -------------------------

            var verticals = new List<(Vector2 a, Vector2 b)>();

            for (int i = 0; i < n - 1; i++)
            {
                var a = pts[i];
                var b = pts[i + 1];

                if (Math.Abs(a.X - b.X) < eps && Math.Abs(a.X - x) < eps)
                {
                    // This segment is vertical and sits exactly at X = x
                    // Normalize ordering: a.Y <= b.Y
                    if (a.Y <= b.Y) verticals.Add((a, b));
                    else verticals.Add((b, a));
                }
            }

            if (verticals.Count > 0)
            {
                // Combine all vertical segments into one continuous vertical span
                float minY = float.PositiveInfinity;
                float maxY = float.NegativeInfinity;

                foreach (var (a, b) in verticals)
                {
                    if (a.Y < minY) minY = a.Y;
                    if (b.Y > maxY) maxY = b.Y;
                }

                var p1 = new Vector2(x, minY);
                var p2 = new Vector2(x, maxY);
                var pm = new Vector2(x, (minY + maxY) * 0.5f);

                result = (p1, pm, p2);
                return true;
            }

            // --- 2. Find the segment whose X-range contains x -------------------------

            for (int i = 0; i < n - 1; i++)
            {
                var a = pts[i];
                var b = pts[i + 1];

                // Check if x lies between a.X and b.X (inclusive)
                bool inRange =
                    (a.X <= x + eps && b.X >= x - eps) ||
                    (b.X <= x + eps && a.X >= x - eps);

                if (!inRange)
                    continue;

                // --- 3. Interpolate Y at X = x ----------------------------------------

                float dx = b.X - a.X;

                // Horizontal segment (flat)
                if (Math.Abs(dx) < eps)
                {
                    // Not vertical (we handled vertical above), so treat as a point
                    float y = 0.5f * (a.Y + b.Y);
                    var pm = new Vector2(x, y);
                    result = (a, pm, b);
                    return true;
                }

                float t = (x - a.X) / dx;
                float yInterp = a.Y + t * (b.Y - a.Y);
                var pm2 = new Vector2(x, yInterp);

                result = (a, pm2, b);
                return true;
            }

            // No segment found
            return false;
        }

        public record struct Runway(float Level, float Start, float End, float ParkingOffset);

        public (Runway, Ground) WithRunway(float level, float start, float end, float parkingOffset)
        {
            // Place physical platform.
            var withPlatform = PlacePlatform(this, new HOffset.OffLeft((start + end) / 2f, Units.Met), Math.Abs(end - start), new VOffset.OffFloor(level, Units.Met));
            // Create runway.
            return (new Runway(level, start, end, parkingOffset), withPlatform);
        }

        private static Ground PlaceRandomPlatforms(Ground src, int number, float width, Func<float, bool> accept, out List<Vector2> platforms)
        {
            var halfWidth = width / 2f;
            var intPlatforms = new List<Vector2>();
            var result = src;

            var fullAccept = (float x) =>
                accept(x) &&
                intPlatforms.All(p => x < p.X - width || p.X + width < x);

            for (var i = 0; i < number; ++i)
            {
                var platformX = 0f;
                do
                {
                    platformX = (float)GameWorld.WorldSeed.NextDouble() * GameWorld.WorldLength;
                } while (!fullAccept(platformX));

                if (!result.GetSegmentAtOffset(new HOffset.OffLeft(platformX, Units.Met), out var seg))
                    throw new ApplicationException("Logic error!");

                var (shiftX, shiftY) =
                    seg.p1.Y < seg.p2.Y ? (-halfWidth + 0.5f, 0f) :   // Incline. Move half-width to the left.
                    seg.p2.Y < seg.p1.Y ? (+halfWidth - 0.5f, 0f) :   // Decline. Move half-width to the right.
                    (0f, 1f);

                var x = platformX + shiftX;
                var y = seg.pm.Y + shiftY;
                intPlatforms.Add(new Vector2(x, y));
                result = PlacePlatform(result, new HOffset.OffLeft(x, Units.Met), width, new VOffset.OffFloor(y, Units.Met));
            }

            platforms = intPlatforms;
            return result;
        }

        public static Ground MakeCustom()
        {
            return
                new GroundBuilder()
                .Segment(OffLeft(-5f, Units.Pct), OffCeiling(0, Units.Pct))
                .SegmentFlat(OffPrev(6f, Units.Pct))
                .Segment(OffPrev(5f, Units.Pct), OffFloor(10f, Units.Pct))
                .SegmentFlat(OffPrev(75f, Units.Met))
                .Segment(OffPrev(25f, Units.Met), OffPrevY(20f, Units.Met))
                .Segment(OffPrev(30f, Units.Met), OffFloor(10f, Units.Met))
                .SegmentFlat(OffRight(10f, Units.Pct))
                .Segment(OffRight(1f, Units.Pct), OffCeiling(0, Units.Pct))
                .SegmentFlat(OffPrev(6f, Units.Pct))
                .Build();
        }

        public static Ground MakeRandom()
        {
            // 25% to 75%
            float randomY() => (float)(25f + GameWorld.WorldSeed.NextDouble() * 50f);

            // First random point at left ceiling
            var b = new GroundBuilder().Segment(OffLeft(0, Units.Pct), OffCeiling(0, Units.Pct));

            var middleSpanPct = 95f;
            var sideSpan = (100f - middleSpanPct) / 2f;
            var pointCount = 21;

            // Middle points
            for (int i = 0; i < pointCount; i++)
                b = b.Segment(OffLeft(2.5f + i*95f/(pointCount-1), Units.Pct), OffFloor(randomY(), Units.Pct));

            // Final point at right ceiling
            var result =  b.Segment(OffRight(0f, Units.Pct), OffCeiling(0f, Units.Pct)).Build();
            
            Console.WriteLine(ReverseBuild(result, Units.Pct, Units.Met));
            return result;
        }

        public static Ground MakeQuasiRandom1() =>
            new GroundBuilder()
                .Segment(OffLeft(0f, Units.Pct), OffCeiling(0f, Units.Met))
                .Segment(OffLeft(2.5f, Units.Pct), OffFloor(14.169f, Units.Met))
                .SegmentFlat(OffLeft(7.25f, Units.Pct))
                .Segment(OffLeft(12f, Units.Pct), OffFloor(31.869f, Units.Met))
                .Segment(OffLeft(16.75f, Units.Pct), OffFloor(25.278f, Units.Met))
                .Segment(OffLeft(21.5f, Units.Pct), OffFloor(32.437f, Units.Met))
                .SegmentFlat(OffLeft(26.25f, Units.Pct))
                .Segment(OffLeft(31f, Units.Pct), OffFloor(16.649f, Units.Met))
                .Segment(OffLeft(35.75f, Units.Pct), OffFloor(30.903f, Units.Met))
                .Segment(OffLeft(40.5f, Units.Pct), OffFloor(19.005f, Units.Met))
                .Segment(OffLeft(45.25f, Units.Pct), OffFloor(25.15f, Units.Met))
                .Segment(OffLeft(50f, Units.Pct), OffFloor(18.257f, Units.Met))
                .Segment(OffLeft(54.75f, Units.Pct), OffFloor(22.179f, Units.Met))
                .Segment(OffLeft(59.5f, Units.Pct), OffFloor(17.9f, Units.Met))
                .Segment(OffLeft(64.25f, Units.Pct), OffFloor(13.02f, Units.Met))
                .Segment(OffLeft(69f, Units.Pct), OffFloor(15.871f, Units.Met))
                .Segment(OffLeft(73.75f, Units.Pct), OffFloor(31.021f, Units.Met))
                .Segment(OffLeft(78.5f, Units.Pct), OffFloor(17.377f, Units.Met))
                .Segment(OffLeft(83.25f, Units.Pct), OffFloor(34.676f, Units.Met))
                .SegmentFlat(OffLeft(88f, Units.Pct))
                .Segment(OffLeft(92.75f, Units.Pct), OffFloor(21.472f, Units.Met))
                .Segment(OffLeft(97.5f, Units.Pct), OffFloor(15.093f, Units.Met))
                .Segment(OffRight(0f, Units.Pct), OffCeiling(0f, Units.Met))
                .Build();

        public static (Ground, List<StaticBuilding>, List<Runway>) MakeWithBuildings()
        {
            var result = MakeQuasiRandom1();
            result = PlacePlatform(result, OffLeft(11, Units.Met), 5, OffFloor(30, Units.Met), float.Pi / 4f);
            result = PlacePlatform(result, OffLeft(26, Units.Met), 5, OffFloor(20, Units.Met), float.Pi / 4f);
            result = PlacePlatform(result, OffLeft(600-26, Units.Met), 5, OffFloor(20, Units.Met), float.Pi / 4f);
            result = PlacePlatform(result, OffLeft(600-11, Units.Met), 5, OffFloor(30, Units.Met), float.Pi / 4f);

            result = PlaceRandomPlatforms(result, 20, 5, (x) => x > 50 && x < GameWorld.WorldLength-50 && (x < 250f || x > 350f), out var platforms);

            var buildings = new List<StaticBuilding>();
            buildings.Add(new StaticBuilding(StaticBuilding.BuildingType.Factory, new Vector2(11, 30f), BasisSpin.Up));
            buildings.Add(new StaticBuilding(StaticBuilding.BuildingType.Cistern, new Vector2(26, 20f), BasisSpin.Up));

            foreach (var platform in platforms)
            {
                var dice = GameWorld.WorldSeed.Next(2);
                var buildingType = 
                    dice == 0 ? StaticBuilding.BuildingType.Factory :
                    dice == 1 ? StaticBuilding.BuildingType.Cistern :
                    StaticBuilding.BuildingType.Factory;

                var spin = GameWorld.WorldSeed.Next(2) == 0 ? BasisSpin.Down : BasisSpin.Up;
                buildings.Add(new StaticBuilding(buildingType, new Vector2(platform.X, platform.Y), spin));
            }

            buildings.Add(new StaticBuilding(StaticBuilding.BuildingType.Factory, new Vector2(600-26, 20f), BasisSpin.Down));
            buildings.Add(new StaticBuilding(StaticBuilding.BuildingType.Cistern, new Vector2(600-11, 30f), BasisSpin.Down));

            var runways = new List<Runway>();
            var (runway, withRunwayPlatform) = result.WithRunway(25, 300-20, 300+20, 5);
            runways.Add(runway);

            return (withRunwayPlatform, buildings, runways);
        }

        public static string ReverseBuild(Ground ground, Units horzUnits, Units vertUnits, bool useOffPrev = false, float flatThreshold = 0.03f)
        {
            var sb = new System.Text.StringBuilder();
            var pts = ground.Points;

            if (pts.Count < 2)
                return "// insufficient points";

            sb.AppendLine("new GroundBuilder()");

            Vector2? prev = null;

            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];

                bool isFirst = p.X < 0 || (i == 1 && p.X >= 0);
                bool isLast = p.X > GameWorld.WorldLength || (i == pts.Count - 1 && p.X <= GameWorld.WorldLength);

                bool isMiddle = !isFirst && !isLast;
                var dx = prev != null ? Math.Abs(p.X - prev.Value.X) : 0.0f;
                bool isFlat = (dx != 0.0f) && Math.Abs(p.Y - prev!.Value.Y) / dx < flatThreshold;

                // Horizontal offset (func, value)
                (string horzFunc, float horzVal) =
                    isFirst ? ("OffLeft",
                        horzUnits == Units.Pct
                            ? (p.X / GameWorld.WorldLength) * 100f
                            : p.X) : 
                    isLast ? ("OffRight",
                        horzUnits == Units.Pct
                            ? ((GameWorld.WorldLength - p.X) / GameWorld.WorldLength) * 100f
                            : (GameWorld.WorldLength - p.X))
                    : useOffPrev && prev != null       // Middle
                        ? ("OffPrev", 
                            horzUnits == Units.Pct
                                ? ((p.X - prev.Value.X) / GameWorld.WorldLength) * 100f
                                : (p.X - prev.Value.X)) 
                        : ("OffLeft",
                            horzUnits == Units.Pct
                                ? (p.X / GameWorld.WorldLength) * 100f
                                : p.X);

                // Vertical offset (func, value)
                (string vertFunc, float vertVal) =
                    isFlat
                    ? (null!, 0f)
                    : ("OffFloor",
                        vertUnits == Units.Pct
                            ? (p.Y / GameWorld.WorldHeight) * 100f
                            : p.Y);

                // Emit code
                sb.AppendLine(
                    isFlat
                        ? $".SegmentFlat({horzFunc}({horzVal:0.###}f, Units.{horzUnits}))"
                        : $".Segment({horzFunc}({horzVal:0.###}f, Units.{horzUnits}), {vertFunc}({vertVal:0.###}f, Units.{vertUnits}))"
                );

                prev = p;
            }

            sb.AppendLine(".Build();");
            return sb.ToString();
        }


        public static Ground MakeFlat(float heightPercentage) =>
            new GroundBuilder()
                .Segment(OffLeft(0, Units.Pct), OffFloor(heightPercentage * 100f, Units.Pct))
                .SegmentFlat(OffRight(0f, Units.Pct))
                .Build();

        public static Ground PlacePlatform(
            Ground src,
            HOffset.OffLeft centerOffset,
            float platformWidth,
            VOffset.OffFloor levelOffset,
            float sideSlopeAngleRad = float.Pi / 6f)
        {
            // --- Local helpers -----------------------------------------------------

            float ResolveX(HOffset.OffLeft o) =>
                o.Units == Units.Pct
                    ? GameWorld.WorldLength * (o.Value / 100f)
                    : o.Value;

            float ResolveY(VOffset.OffFloor o) =>
                o.Units == Units.Pct
                    ? GameWorld.WorldHeight * (o.Value / 100f)
                    : o.Value;

            bool IntersectSegments(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 hit)
            {
                hit = default;

                var r = b - a;
                var s = d - c;
                float rxs = r.X * s.Y - r.Y * s.X;
                if (Math.Abs(rxs) < 1e-6f) return false; // parallel

                var cma = c - a;
                float t = (cma.X * s.Y - cma.Y * s.X) / rxs;
                float u = (cma.X * r.Y - cma.Y * r.X) / rxs;

                if (t < 0 || t > 1 || u < 0 || u > 1) return false;

                hit = a + t * r;
                return true;
            }

            Vector2? ClipToX(Vector2 p1, Vector2 p2, float x)
            {
                const float eps = 1e-4f;

                // If both points are strictly on one side of x, no intersection.
                if ((p1.X < x - eps && p2.X < x - eps) ||
                    (p1.X > x + eps && p2.X > x + eps))
                    return null;

                float t = (x - p1.X) / (p2.X - p1.X);
                if (t < 0 - eps || t > 1 + eps)
                    return null;

                return new Vector2(
                    x,
                    p1.Y + t * (p2.Y - p1.Y)
                );
            }

            IEnumerable<Vector2> ClipSegmentToRange(Vector2 p1, Vector2 p2, float xMin, float xMax)
            {
                if (p1.X > p2.X) (p1, p2) = (p2, p1);

                if (p2.X < xMin || p1.X > xMax)
                    yield break;

                var a = p1;
                var b = p2;

                if (a.X < xMin)
                {
                    var c = ClipToX(a, b, xMin);
                    if (c.HasValue) a = c.Value;
                }

                if (b.X > xMax)
                {
                    var c = ClipToX(a, b, xMax);
                    if (c.HasValue) b = c.Value;
                }

                yield return a;
                if (b != a) yield return b;
            }

            // --- Compute trapezoid geometry ----------------------------------------

            float cx = ResolveX(centerOffset);
            float ty = ResolveY(levelOffset);

            float halfW = platformWidth / 2f;

            var topLeft = new Vector2(cx - halfW, ty);
            var topRight = new Vector2(cx + halfW, ty);

            float dx = ty * MathF.Tan(sideSlopeAngleRad);

            var bottomLeft = new Vector2(topLeft.X - dx, 0);
            var bottomRight = new Vector2(topRight.X + dx, 0);

            var L1 = bottomLeft; var L2 = topLeft;
            var T1 = topLeft; var T2 = topRight;
            var R1 = topRight; var R2 = bottomRight;

            bool x_within(float x, Vector2 p1, Vector2 p2) => x >= p1.X && x <= p2.X;
            float y_within(float x, Vector2 p1, Vector2 p2) =>
                p1.Y + ((x - p1.X) / (p2.X - p1.X)) * (p2.Y - p1.Y);

            float trapezoidYAt(float x) =>
                x_within(x, L1, L2) ? y_within(x, L1, L2) :
                x_within(x, T1, T2) ? y_within(x, T1, T2) :
                x_within(x, R1, R2) ? y_within(x, R1, R2) :
                float.NegativeInfinity;

            bool shouldCut(Vector2 hit)
            {
                float trapY = trapezoidYAt(hit.X);
                float groundY = hit.Y;

                const float eps = 1e-3f;
                return groundY + eps >= trapY;
            }

            // --- Collect intersections ---------------------------------------------

            var hits = new List<(float X, float Y, char Edge)>();
            var pts = src.Points;

            for (int i = 0; i < pts.Count - 1; i++)
            {
                var g1 = pts[i];
                var g2 = pts[i + 1];

                if (IntersectSegments(g1, g2, L1, L2, out var hL) && shouldCut(hL))
                    hits.Add((hL.X, hL.Y, 'L'));

                if (IntersectSegments(g1, g2, T1, T2, out var hT) && shouldCut(hT))
                    hits.Add((hT.X, hT.Y, 'T'));

                if (IntersectSegments(g1, g2, R1, R2, out var hR) && shouldCut(hR))
                    hits.Add((hR.X, hR.Y, 'R'));
            }

            // --- If no intersections: return clone ---------------------------------

            if (hits.Count == 0)
                return new Ground(new List<Vector2>(src.Points));

            foreach (var h in hits)
                Console.WriteLine($"hit: X={h.X}, Y={h.Y}, Edge={h.Edge}");

            // --- Determine cut region ----------------------------------------------

            float cutStartX = hits.Min(h => h.X);
            float cutEndX = hits.Max(h => h.X);

            // --- Build result polyline ---------------------------------------------

            var result = new List<Vector2>();

            // 1. Ground left of cutStartX
            foreach (var p in pts)
                if (p.X < cutStartX)
                    result.Add(p);

            // 2. Insert cutStart intersection
            var startHit = hits.OrderBy(h => Math.Abs(h.X - cutStartX)).First();
            result.Add(new Vector2(startHit.X, startHit.Y));

            // 3. Insert trapezoid edges clipped to [cutStartX, cutEndX]
            void AddClipped(Vector2 a, Vector2 b)
            {
                foreach (var p in ClipSegmentToRange(a, b, cutStartX, cutEndX))
                    result.Add(p);
            }

            // Left slope
            if (cutStartX <= L2.X && cutEndX >= L1.X)
                AddClipped(L1, L2);

            // Top
            if (cutStartX <= T2.X && cutEndX >= T1.X)
                AddClipped(T1, T2);

            // Right slope
            if (cutStartX <= R2.X && cutEndX >= R1.X)
                AddClipped(R1, R2);

            // 4. Insert cutEnd intersection
            var endHit = hits.OrderBy(h => Math.Abs(h.X - cutEndX)).First();
            result.Add(new Vector2(endHit.X, endHit.Y));

            // 5. Ground right of cutEndX
            foreach (var p in pts)
                if (p.X > cutEndX)
                    result.Add(p);

            // 6. Sort by X (stable)
            result.Sort((a, b) => a.X.CompareTo(b.X));

            // Remove consecutive points with identical X (within epsilon)
            var cleaned = new List<Vector2>();
            const float eps = 1e-4f;

            foreach (var p in result)
            {
                if (cleaned.Count == 0 || Math.Abs(cleaned[^1].X - p.X) > eps)
                    cleaned.Add(p);
                else if (p.Y > cleaned[^1].Y)   // Keep the one with the higher Y (visible contour)
                    cleaned[^1] = p;
            }

            return new Ground(cleaned);
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
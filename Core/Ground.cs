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

        public enum Units { Pct, Met }     // Met - world units, "Meters"; Pct - percentage of the relevant overall measurement - GameWorld.Height or GameWorld.Length

        private abstract record HOffset(float Value, Units Units)
        {
            public sealed record OffLeft(float Value, Units Units) : HOffset(Value, Units);
            public sealed record OffRight(float Value, Units Units) : HOffset(Value, Units);
            public sealed record OffPrev(float Value, Units Units) : HOffset(Value, Units);
        }

        private abstract record VOffset(float Value, Units Units)
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

        private static float Resolve(HOffset o, Vector2 prev) => o switch
            {
                HOffset.OffPrev  => Resolve(o.Units, prev.X,                o.Value, GameWorld.WorldLength, 1f),
                HOffset.OffLeft  => Resolve(o.Units, 0f,                    o.Value, GameWorld.WorldLength, 1f),
                HOffset.OffRight => Resolve(o.Units, GameWorld.WorldLength, o.Value, GameWorld.WorldLength, -1f),
                _ => throw new ArgumentOutOfRangeException()
            };

        private static float Resolve(VOffset o, Vector2 prev) => o switch
        {
            VOffset.OffPrev    => Resolve(o.Units, prev.Y,                o.Value, GameWorld.WorldHeight, 1f),
            VOffset.OffFloor   => Resolve(o.Units, 0f,                    o.Value, GameWorld.WorldHeight, 1f),
            VOffset.OffCeiling => Resolve(o.Units, GameWorld.WorldHeight, o.Value, GameWorld.WorldHeight, -1f),
            _ => throw new ArgumentOutOfRangeException()
        };

        private sealed class GroundBuilder
        {
            private readonly List<Vector2> _points = new();
            private Vector2 _prev;

            public GroundBuilder()
            {
                _prev = new Vector2(0, 0);
                _points.Add(_prev);
            }

            public GroundBuilder Segment(HOffset x, VOffset y)
            {
                _prev = new Vector2(Resolve(x, _prev), Resolve(y, _prev));
                _points.Add(_prev);
                return this;
            }

            public GroundBuilder SegmentFlat(HOffset x)
            {
                _prev = new Vector2(Resolve(x, _prev), _prev.Y);
                _points.Add(_prev);
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
            Console.WriteLine(ReverseEngineer(result, Units.Met, Units.Met));
            return result;
        }

        public static Ground MakeQuasiRandom1() =>
            new GroundBuilder()
            .Segment(OffLeft(0f, Units.Met), OffFloor(50f, Units.Met))
            .Segment(OffPrev(15f, Units.Met), OffFloor(14.169f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(14.254f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(31.869f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(25.278f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(32.437f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(33.183f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(16.649f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(30.903f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(19.005f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(25.15f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(18.257f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(22.179f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(17.9f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(13.02f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(15.871f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(31.021f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(17.377f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(34.676f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(34.028f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(21.472f, Units.Met))
            .Segment(OffPrev(28.5f, Units.Met), OffFloor(15.093f, Units.Met))
            .Segment(OffRight(0f, Units.Met), OffFloor(50f, Units.Met))
            .Build();

        public static string ReverseEngineerTask(Ground ground, Units horzUnits, Units vertUnits)
        {
            return "";
            // This method should "reverse-engineer" an instance of Ground class into a variant of builder-based code that would generate exactly that instance. 
            // Invariants to preserve:
            // - Points are considered "First" when they lie before 0 on X-axis, plus the very first point which lies after 0.
            // - Points are considered "Last" when they lie after GameWorld.WorldLength on X-axis, plus the very last point which lies before GameWorld.WorldLength.
            // - All points in-between "First" and "Last" group are considered "Middle"
            // - "First" points should be horizontally represented by `OffLeft`.
            // - "Last" points should be horizontally represented by `OffRight`.
            // - "Middle" points should be horizontally represented by `OffPrev`.
            // - "Middle" points should be vertically represented by `OffFloor`.
            // - "First" and "Last" points should be vertically represented by `OffFloor`.
            // - All vertical and horizontal offsets must be expressed according to passed in parameters horzUnits and vertUnits.
            // - Where appropriate (i.e. current-Y == prev-Y), regardles of First/Last/Middle category, SegmentFlat should be used.
            // - No "optimizations" or loops - each source point must be represented by one and only one explicit `Segment` or `SegmentFlat` operator.
            // - No specific detection or action for "degenerate" Ground instances (i.e. with no points or a single point, or indetenrminate categorization of First/Last/Middle)
            //   I am happy for silent failure or indeterminate result.

        }

        public static string ReverseEngineer(Ground ground, Units horzUnits, Units vertUnits)
        {
            var sb = new System.Text.StringBuilder();
            var pts = ground.Points;

            if (pts.Count < 2)
                return "// insufficient points";

            sb.AppendLine("new GroundBuilder()");

            Vector2 prev = pts[0];

            for (int i = 1; i < pts.Count; i++)
            {
                var p = pts[i];

                bool isFirst =
                    p.X < 0 ||
                    (i == 1 && p.X >= 0);

                bool isLast =
                    p.X > GameWorld.WorldLength ||
                    (i == pts.Count - 1 && p.X <= GameWorld.WorldLength);

                bool isMiddle = !isFirst && !isLast;
                bool isFlat = Math.Abs(p.Y - prev.Y) < 0.0001f;

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
                    : ("OffPrev", // Middle
                        horzUnits == Units.Pct
                            ? ((p.X - prev.X) / GameWorld.WorldLength) * 100f
                            : (p.X - prev.X));

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
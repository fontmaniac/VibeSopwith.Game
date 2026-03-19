using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class Airplane : ICentered, ISimulated<Airplane.State>
    {
        public Body Body = null!;

        public record State(Vector2 Position, Vector2 Direction, Winding NormalDown, float Speed, bool launchingBomb, DateTime RollTime, DateTime BombTime);

        public State CurrentState = new State(Vector2.Zero, Vector2.UnitX, Winding.Clockwise, 0f, false, DateTime.MinValue, DateTime.MinValue);
        public float Speed { get => CurrentState.Speed; }

        public Vector2 Position { get => CurrentState.Position; }       // World position of the plane in meters. 
        public Vector2 Direction { get => CurrentState.Direction; }     // Direction where plane is facing.
        public Winding NormalDown { get => CurrentState.NormalDown; }   // Orientation of normal pointing towards bottom of the plane.
        public float Length => 3.46f;
        public float Height => 2.0f;

        public Vector2 MidPoint { get => Position + midPointOffset.Rotate(Direction.ToAngle()); }

        public bool Exploded = false;

        public void Place(Vector2 pos, Winding normalDown) => CurrentState = CurrentState with { Position = pos, NormalDown = normalDown };

        public void RemoveRigging(World collisionWorld)
        {
            collisionWorld.Remove(Body);
        }

        private Fixture[] bodyFixtures = null!;
        private Vector2 midPointOffset = Vector2.Zero;

        public void SetupRigging(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Aether.Vector2.Zero, 0f, BodyType.Dynamic);
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 500f;
            body.Inertia = 0f;

            this.Body = body;

            (bodyFixtures, midPointOffset) = RebuildFixtures(body, [], NormalDown);
        }

        private (Fixture[], Vector2) RebuildFixtures(Body body, Fixture[] fixtures, Winding normalDown)
        {
            foreach (var fixture in fixtures) 
                body.Remove(fixture);

            var ff = normalDown == Winding.Clockwise ? +1f : -1f;

            // Add fixture 0
            var vertices0 = new Aether.Vertices();
            vertices0.Add(new Aether.Vector2(0.0000f,  ff * 0f));
            vertices0.Add(new Aether.Vector2(0.2304f,  ff * 0f));
            vertices0.Add(new Aether.Vector2(0.3513f,  ff * 1.968f));
            vertices0.Add(new Aether.Vector2(-0.6837f, ff * 1.9793f));
            vertices0.Add(new Aether.Vector2(-0.763f,  ff * 1.5506f));
            vertices0.Add(new Aether.Vector2(-0.1681f, ff * 0.0359f));
            var shape0 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices0, 1.0f);
            var fixture0 = body.CreateFixture(shape0);
            fixture0.Friction = 0.5f;
            fixture0.Restitution = 0.0f;
            fixture0.CollisionCategories = Category.Cat1;
            fixture0.CollidesWith = Category.All;

            // Add fixture 1
            var vertices1 = new Aether.Vertices();
            vertices1.Add(new Aether.Vector2(1.1521f, ff * 1.1898f));
            vertices1.Add(new Aether.Vector2(1.1521f, ff * 1.035f));
            vertices1.Add(new Aether.Vector2(0.9821f, ff * 0.5779f));
            vertices1.Add(new Aether.Vector2(0.2569f, ff * 0.6138f));
            vertices1.Add(new Aether.Vector2(0.2606f, ff * 1.6412f));
            vertices1.Add(new Aether.Vector2(0.9764f, ff * 1.645f));
            var shape1 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices1, 1.0f);
            var fixture1 = body.CreateFixture(shape1);
            fixture1.Friction = 0.5f;
            fixture1.Restitution = 0.0f;
            fixture1.CollisionCategories = Category.Cat1;
            fixture1.CollidesWith = Category.All;

            // Add fixture 2
            var vertices2 = new Aether.Vertices();
            vertices2.Add(new Aether.Vector2(-0.763f,  ff * 1.5562f));
            vertices2.Add(new Aether.Vector2(-1.8943f, ff * 1.8868f));
            vertices2.Add(new Aether.Vector2(-2.2645f, ff * 1.8811f));
            vertices2.Add(new Aether.Vector2(-2.2664f, ff * 1.1861f));
            vertices2.Add(new Aether.Vector2(-2.0454f, ff * 0.9462f));
            vertices2.Add(new Aether.Vector2(-0.4457f, ff * 0.7517f));
            var shape2 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices2, 1.0f);
            var fixture2 = body.CreateFixture(shape2);
            fixture2.Friction = 0.5f;
            fixture2.Restitution = 0.0f;
            fixture2.CollisionCategories = Category.Cat1;
            fixture2.CollidesWith = Category.All;

            // Add fixture 3
            var vertices3 = new Aether.Vertices();
            vertices3.Add(new Aether.Vector2(-1.8074f, ff * 0.933f));
            vertices3.Add(new Aether.Vector2(-1.6261f, ff * 0.9103f));
            vertices3.Add(new Aether.Vector2(-1.6639f, ff * 0.4438f));
            vertices3.Add(new Aether.Vector2(-1.832f,  ff * 0.3041f));
            vertices3.Add(new Aether.Vector2(-2.0246f, ff * 0.3343f));
            vertices3.Add(new Aether.Vector2(-2.1417f, ff * 0.5458f));
            vertices3.Add(new Aether.Vector2(-2.0152f, ff * 0.7649f));
            var shape3 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices3, 1.0f);
            var fixture3 = body.CreateFixture(shape3);
            fixture3.Friction = 0.5f;
            fixture3.Restitution = 0.0f;
            fixture3.CollisionCategories = Category.Cat1;
            fixture3.CollidesWith = Category.All;

            Aether.Vector2 findMidPoint(IEnumerable<Aether.Vector2> vertices)
            {
                var acc = vertices.Aggregate(
                    seed: (minX: float.PositiveInfinity,
                           maxX: float.NegativeInfinity,
                           minY: float.PositiveInfinity,
                           maxY: float.NegativeInfinity),

                    func: (a, v) => (
                        minX: MathF.Min(a.minX, v.X),
                        maxX: MathF.Max(a.maxX, v.X),
                        minY: MathF.Min(a.minY, v.Y),
                        maxY: MathF.Max(a.maxY, v.Y)
                    )
                );

                return new Aether.Vector2(
                    (acc.minX + acc.maxX) * 0.5f,
                    (acc.minY + acc.maxY) * 0.5f
                );
            }

            var midPoint = findMidPoint(vertices0.Concat(vertices1).Concat(vertices2).Concat(vertices3));

            return (new[] { fixture0, fixture1, fixture2, fixture3 }, midPoint.ToXna());
        }

        private const float Acceleration = 0.05f; // meters per second^2
        private const float MaxSpeed = 0.4f; // meters per second
        private const float PitchAngle = 4.0f;
        private const float RollGracePeriod = 1f / 4f; // Time in seconds before subsequent roll input is accepted.
        private const float BombGracePeriod = 0.25f; // Time in seconds before subsequent bomb can be spawned.

        public enum ThrottleInput { Throttling, None, Reversing }
        public enum PitchInput { Forward, None, Backward }
        public enum RollInput { Roll, None }
        public enum BombInput { Active, Inactive }

        public ThrottleInput Throttle;
        public PitchInput Pitch;
        public RollInput Roll;
        public BombInput Bomb;

        public State ApplyInputs(GameTime gameTime)
        {
            var newSpeed = MathHelper.Clamp((
                Throttle == ThrottleInput.Throttling ? Speed + Acceleration :
                Throttle == ThrottleInput.Reversing ? Speed - Acceleration :
                Speed), 0f, MaxSpeed);

            var nowTime = DateTime.UtcNow;

            var (newBomb, newBombTime) = (Bomb == BombInput.Inactive || (nowTime - CurrentState.BombTime).TotalSeconds < BombGracePeriod) 
                ? (false, CurrentState.BombTime) 
                : (true, nowTime);

            var (newWinding, newRollTime) = (Roll == RollInput.None || (nowTime - CurrentState.RollTime).TotalSeconds < RollGracePeriod) ? (NormalDown, CurrentState.RollTime) :
                NormalDown == Winding.Clockwise
                ? (Winding.CounterClockwise, nowTime)
                : (Winding.Clockwise, nowTime);

            var rollFactor = newWinding == Winding.Clockwise ? +1f : -1f;

            var newDirection = Speed == 0f ? Direction : Vector2.TransformNormal(Direction,
                Pitch == PitchInput.Backward ? Matrix.CreateRotationZ(rollFactor * MathHelper.ToRadians(PitchAngle)) :
                Pitch == PitchInput.Forward  ? Matrix.CreateRotationZ(-rollFactor * MathHelper.ToRadians(PitchAngle)) :
                Matrix.Identity);

            var newPosition = Position + newDirection * newSpeed;

            return new State(newPosition, newDirection, newWinding, newSpeed, newBomb, newRollTime, newBombTime);
        }

        public void ClearInputs()
        {
            Throttle = ThrottleInput.None;
            Pitch = PitchInput.None;
            Roll = RollInput.None;
        }

        public void PreSimulationPrepare(State projected)
        {
            
        }

        public void PostSimulationUpdate(State projected)
        {
            if (NormalDown != projected.NormalDown)
            {
                // Executing a roll/flip.
                // Reflect position across horizontal midline.
                projected = projected with { Position = Position.ReflectPointAcrossLine(MidPoint, Direction) };

                // Rebuild Aether2D Body from flipped perspective.
                (bodyFixtures, midPointOffset) = RebuildFixtures(Body, bodyFixtures, projected.NormalDown);
            }


            CurrentState = projected;
            Body.Position = Position.ToAether();
            Body.Rotation = Direction.ToAngle();
        }
    }
}

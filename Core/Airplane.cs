using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using System.Diagnostics.Metrics;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class Airplane : ICentered
    {
        public Body Body = null!;

        public record State(Vector2 Position, Vector2 Direction, Winding NormalDown, float Speed, DateTime RollTime);

        public State CurrentState = new State(Vector2.Zero, Vector2.UnitX, Winding.Clockwise, 0f, DateTime.MinValue);
        public float Speed { get => CurrentState.Speed; }

        public Vector2 Position { get => CurrentState.Position; }       // World position of geometrical center of the plane in meters.
        public Vector2 Direction { get => CurrentState.Direction; }     // Direction where plane is facing.
        public Winding NormalDown { get => CurrentState.NormalDown; }   // Orientation of normal pointing towards bottom of the plane.
        public float Length => 3.46f;
        public float Height => 2.0f;

        public void Place(Vector2 pos, Winding normalDown) => CurrentState = CurrentState with { Position = pos, NormalDown = normalDown };

        public void SetupRigging(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Aether.Vector2.Zero, 0f, BodyType.Dynamic);
            body.Tag = this;
            body.FixedRotation = false;
            body.Mass = 1000f;
            body.Inertia = 0f;
            body.LinearDamping = 0f;
            body.AngularDamping = 0f;

            // Add fixture 0
            var vertices0 = new Aether.Vertices();
            vertices0.Add(new Aether.Vector2(-0.1284f, +0.9745f));
            vertices0.Add(new Aether.Vector2(0.8782f, +0.9802f));
            vertices0.Add(new Aether.Vector2(0.8593f, -0.8329f));
            vertices0.Add(new Aether.Vector2(0.6686f, -1f));
            vertices0.Add(new Aether.Vector2(0.5005f, -1f));
            vertices0.Add(new Aether.Vector2(0.3003f, -0.814f));
            vertices0.Add(new Aether.Vector2(-0.1228f, +0.5553f));
            var shape0 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices0, 1.0f);
            var fixture0 = body.CreateFixture(shape0);
            fixture0.Friction = 0.5f;
            fixture0.Restitution = 0.1f;

            // Add fixture 1
            var vertices1 = new Aether.Vertices();
            vertices1.Add(new Aether.Vector2(1.5714f, +0.6384f));
            vertices1.Add(new Aether.Vector2(1.713f, +0.1964f));
            vertices1.Add(new Aether.Vector2(1.7187f, +0.0416f));
            vertices1.Add(new Aether.Vector2(1.5732f, -0.4042f));
            vertices1.Add(new Aether.Vector2(0.8329f, -0.4004f));
            vertices1.Add(new Aether.Vector2(0.8197f, +0.6648f));
            var shape1 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices1, 1.0f);
            var fixture1 = body.CreateFixture(shape1);
            fixture1.Friction = 0.5f;
            fixture1.Restitution = 0.1f;

            // Add fixture 2
            var vertices2 = new Aether.Vertices();
            vertices2.Add(new Aether.Vector2(-0.0963f, +0.5553f));
            vertices2.Add(new Aether.Vector2(0.1473f, -0.2285f));
            vertices2.Add(new Aether.Vector2(-1.4429f, -0.0812f));
            vertices2.Add(new Aether.Vector2(-1.7224f, +0.2531f));
            vertices2.Add(new Aether.Vector2(-1.7243f, +0.8801f));
            vertices2.Add(new Aether.Vector2(-1.3315f, +0.8896f));
            vertices2.Add(new Aether.Vector2(-1.1124f, +0.5666f));
            var shape2 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices2, 1.0f);
            var fixture2 = body.CreateFixture(shape2);
            fixture2.Friction = 0.5f;
            fixture2.Restitution = 0.1f;

            // Add fixture 3
            var vertices3 = new Aether.Vertices();
            vertices3.Add(new Aether.Vector2(-1.069f, -0.1058f));
            vertices3.Add(new Aether.Vector2(-1.0822f,-0.5288f));
            vertices3.Add(new Aether.Vector2(-1.2578f, -0.695f));
            vertices3.Add(new Aether.Vector2(-1.4259f, -0.6912f));
            vertices3.Add(new Aether.Vector2(-1.5808f, -0.508f));
            vertices3.Add(new Aether.Vector2(-1.5581f, -0.3286f));
            vertices3.Add(new Aether.Vector2(-1.2333f, -0.0907f));
            var shape3 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices3, 1.0f);
            var fixture3 = body.CreateFixture(shape3);
            fixture3.Friction = 0.5f;
            fixture3.Restitution = 0.1f;

            this.Body = body;
        }

        private const float Acceleration = 0.05f; // meters per second^2
        private const float MaxSpeed = 0.4f; // meters per second
        private const float PitchAngle = 4.0f;
        private const float RollGracePeriod = 1f / 4f; // Time in seconds before subsequent roll input is accepted.

        public enum ThrottleInput { Throttling, None, Reversing }
        public enum PitchInput { Forward, None, Backward }
        public enum RollInput { Roll, None }

        public ThrottleInput Throttle;
        public PitchInput Pitch;
        public RollInput Roll;

        public State ApplyInputs(GameTime gameTime)
        {
            var newSpeed = MathHelper.Clamp((
                Throttle == ThrottleInput.Throttling ? Speed + Acceleration :
                Throttle == ThrottleInput.Reversing ? Speed - Acceleration :
                Speed), 0f, MaxSpeed);

            var nowTime = DateTime.UtcNow;
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

            return new State(newPosition, newDirection, newWinding, newSpeed, newRollTime);
        }

        public void ClearInputs()
        {
            Throttle = ThrottleInput.None;
            Pitch = PitchInput.None;
            Roll = RollInput.None;
        }


    }
}

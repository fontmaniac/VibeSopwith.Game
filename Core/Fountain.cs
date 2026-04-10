using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal class Fountain : IHasLocation, ICanRemoveRigging, IHasParts, IAmBehaving<Fountain.State>
    {
        public Body Body = null!;

        public record State(float NozzleAngle, NozzleMovement Movement);
        public State CurrentState;

        public Vector2 Position { get => WorldLocation.Position; }
        public Vector2 Direction { get => WorldLocation.Direction; }
        public BasisSpin Spin { get => WorldLocation.Spin; }
        public float Length => 4f;
        public float Height => 4f;

        private IBasis WorldLocation;

        public FountainNozzle Nozzle;

        public bool Exploded = false;

        public enum NozzleMovement { Left, Right, Stop };

        private const float NozzleMaxAngle = 45f;          // Degrees.
        private const float NozzleAngleChangeRate = 30f;   // Degree per second.

        private Dictionary<string, (float X, float Y)> refPoints = new Dictionary<string, (float X, float Y)>
        {
            { "nozzleMount", (0f, 0.875f) }
        };

        private (float X, float Y) GetRefPoint(string name)
        {
            var refPoint = refPoints[name];
            return (refPoint.X, Spin.ToFactor() * refPoint.Y);
        }

        public Fountain(IBasis worldLocation)
        {
            WorldLocation = worldLocation;

            var nozzleAngle = 0f;
            CurrentState = new State(nozzleAngle, NozzleMovement.Left);

            var nozzleDirection = () => Vector2.UnitY.RotateDeg(CurrentState.NozzleAngle * Spin.ToFactor());
            var nozzlePosition = () => GetRefPoint("nozzleMount").ToXna();

            Nozzle = new FountainNozzle(this, new LiveBasis(nozzlePosition, nozzleDirection, () => BasisSpin.Down));
        }

        public IBasis PickPart(object tag)
        {
            var fixture = tag as Fixture;
            if (fixture == null) return this;
            if (fixture.Body == Body) return this;
            if (fixture.Body == Nozzle.Body) return Nozzle;
            return this;
        }

        public void RemoveRigging(World collisionWorld)
        {
            Nozzle.RemoveRigging(collisionWorld);
            collisionWorld.Remove(Body);
            Body = null!;
        }

        public void SetupRigging(World collisionWorld, Func<object>? makeTag = null)
        {
            var body = collisionWorld.CreateBody(Position.ToAether(), 0f, BodyType.Static);
            body.Rotation = Direction.ToAngle();
            body.Tag = this;
            body.FixedRotation = false;
            body.Mass = 1000f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 0.0f;

            var ff = Spin.ToFactor();

            // Add fixture 0
            var vertices0 = new[]
            {
                (-1.8984f, ff * 0f),
                (-1.9023f, ff * 0.8711f),
                (1.9141f, ff * 0.8711f),
                (1.9063f, ff * 0f)
            };

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;
            // Add fixture 1
            var vertices1 = new[]
            {
                (-1.5313f, ff * 0.8672f),
                (-1.2695f, ff * 1.6992f),
                (-0.3242f, ff * 2.3281f),
                (0.3086f, ff * 2.3242f),
                (1.2813f, ff * 1.7109f),
                (1.5391f, ff * 0.875f)
            };

            var fixture1 = vertices1.ToPolygon(body);
            fixture1.Friction = 0.0f;
            fixture1.Restitution = 0.0f;

            this.Body = body;

            makeTag = makeTag ?? (() => this);
            Body.Tag = makeTag();

            Nozzle.SetupRigging(collisionWorld, makeTag);
        }

        public State DeriveState(GameTime gameTime)
        {
            var nowTime = DateTime.UtcNow;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var newAngle =
                CurrentState.Movement == NozzleMovement.Stop 
                ? CurrentState.NozzleAngle
                : CurrentState.NozzleAngle + (CurrentState.Movement == NozzleMovement.Left ? +1f : -1f) * NozzleAngleChangeRate * dt;
            var newMovement =
                CurrentState.Movement == NozzleMovement.Stop ? NozzleMovement.Stop :
                newAngle >= NozzleMaxAngle ? NozzleMovement.Right :
                newAngle <= -NozzleMaxAngle ? NozzleMovement.Left :
                CurrentState.Movement;

            return new(newAngle, newMovement);
        }

        public void PreSimulationPrepare(State projected)
        {
            if (Nozzle.Body == null) return;

            Nozzle.Body.Position = Nozzle.Position.ToAether();
            Nozzle.Body.Rotation = Nozzle.Direction.ToAngle();
        }

        public void PostSimulationUpdate(State projected)
        {
            CurrentState = projected;
        }
    }
}

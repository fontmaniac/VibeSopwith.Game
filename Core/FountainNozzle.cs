using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal class FountainNozzle : IHasLocation, ICanRemoveRigging
    {
        public Body Body = null!;

        public IBasis Parent { get; }
        public IBasis Local { get; }

        // Returned Basis is now "World" basis.
        public Vector2 Position { get => Parent.Position + Local.Position; }
        public Vector2 Direction { get => Parent.Direction.Rotate(Local.Direction.ToAngle()); }
        public BasisSpin Spin { get => Local.Spin == BasisSpin.Down ? Parent.Spin : Parent.Spin.Toggle(); }

        public float Length => 2f;
        public float Height => 2f;

        public FountainNozzle(IBasis parent, IBasis localBasis)
        {
            Parent = parent;
            Local = localBasis;
        }

        public void RemoveRigging(World collisionWorld)
        {
            collisionWorld.Remove(Body);
            Body = null!;
        }

        private Dictionary<string, (float X, float Y)> refPoints = new Dictionary<string, (float X, float Y)>
        {
            { "tip", (1.9f, 0.0f) }
        };

        private (float X, float Y) GetRefPoint(string name)
        {
            var refPoint = refPoints[name];
            return (refPoint.X, Spin.ToFactor() * refPoint.Y);
        }

        public void SetupRigging(World collisionWorld, Func<object>? makeTag = null)
        {
            var body = collisionWorld.CreateBody(Position.ToAether(), 0f, BodyType.Kinematic);
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
                (-0.0996f, ff * 0.3418f),
                (1.7227f, ff * 0.3418f),
                (1.9004f, ff * 0.0449f),
                (1.8984f, ff * -0.0508f),
                (1.7285f, ff * -0.3457f),
                (-0.0996f, ff * -0.3496f),
                (-0.0996f, ff * -0.3496f)
            };

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;

            this.Body = body;

            makeTag = makeTag ?? (() => this);
            Body.Tag = makeTag();
        }

        public ParticleSystem.Prototype SpawnParticleSystem(TimeSpan startTime)
        {
            var mid = Position.ToAether();
            var tip = Body.GetWorldPoint(GetRefPoint("tip").ToAether());
            var launchDirection = (tip - mid);
            var spawnPos = tip + launchDirection * 0.00f;

            var boundBasis = LiveBasis.Bind(new Basis(spawnPos.ToXna(), Direction, Spin), this);
            var result = new ParticleSystem.Prototype(boundBasis, 600f);

            return result;
        }

    }
}

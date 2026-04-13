using Microsoft.Xna.Framework;
using Nage.Strata.Abstractions.Spatial;
using Nage.Strata.Physics;
using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game.Core
{
    internal class FlakGunBarrel : IHasLocation, ICanRemoveRigging
    {
        public Body Body = null!;

        public IBasis Parent { get; }
        public IBasis Local { get; }

        // Returned Basis is now "World" basis.
        public Vector2 Position { get => Parent.Position + Local.Position; }
        public Vector2 Direction { get => Parent.Direction.Rotate(Local.Direction.ToAngle()); }
        public BasisSpin Spin { get => Local.Spin == BasisSpin.Down ? Parent.Spin : Parent.Spin.Toggle(); }

        public float Length => 8f;
        public float Height => 4f;

        public const float BulletSpeed = 21f;               // meters per second

        public FlakGunBarrel(IBasis parent, IBasis localBasis)
        {
            Parent = parent;
            Local = localBasis;
        }

        private Dictionary<string, (float X, float Y)> refPoints = new Dictionary<string, (float X, float Y)>
        {
            { "gun0", (0.0f, 1.0f) },
            { "gun1", (3.8271f, 1.0f) },
        };

        private (float X, float Y) GetRefPoint(string name)
        {
            var refPoint = refPoints[name];
            return (refPoint.X, Spin.ToFactor() * refPoint.Y);
        }

        public void RemoveRigging(World simWorld)
        {
            simWorld.Remove(Body);
            Body = null!;
        }

        public void SetupRigging(World simWorld, Func<object>? makeTag = null)
        {
            var body = simWorld.CreateBody(Position.ToAether(), 0f, BodyType.Kinematic);
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
                (-1.7939f, ff * 1.0195f),
                (-0.7031f, ff * 1.0205f),
                (0.8584f, ff * 0.6807f),
                (1.6436f, ff * 0.3057f),
                (1.6401f, ff * -0.2578f),
                (-1.5796f, ff * -0.2578f),
                (-1.7974f, ff * 0.2432f)
            };

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;

            // Add fixture 1
            var vertices1 = new[]
            {
                (-0.7031f, ff * 1.0195f),
                (-0.7031f, ff * 1.6177f),
                (-0.5137f, ff * 1.7739f),
                (0.7305f, ff * 1.7729f),
                (1.5771f, ff * 1.6167f),
                (3.8291f, ff * 1.2417f),    // Top of the barrel
                (3.8252f, ff * 0.6812f),    // Bottom of the barrel
                (0.8623f, ff * 0.6831f)
            };

            var fixture1 = vertices1.ToPolygon(body);
            fixture1.Friction = 0.0f;
            fixture1.Restitution = 0.0f;

            this.Body = body;

            makeTag = makeTag ?? (() => this);
            Body.Tag = makeTag();
        }

        public (Bullet? bullet, Explosion? muzzleFlash) SpawnBullet(TimeSpan startTime)
        {
            var gun0 = Body.GetWorldPoint(GetRefPoint("gun0").ToAether());
            var gun1 = Body.GetWorldPoint(GetRefPoint("gun1").ToAether());
            var launchDirection = gun1 - gun0;
            var spawnPos = gun1 + launchDirection * 0.05f;
            var velocityVector = Vector2.Normalize(launchDirection.ToXna()) * BulletSpeed;

            var boundBasis = LiveBasis.Bind(Basis.FixedPos(gun1.ToXna()), this);
            var muzzleFlash = new Explosion(Explosion.ExplosionVariant.Centered1, 0.5f, 0.5f, startTime, TimeSpan.FromSeconds(0.08), boundBasis);

            return (new Bullet(new Bullet.State(spawnPos.ToXna(), Vector2.Normalize(velocityVector), velocityVector), startTime), muzzleFlash);
        }
    }
}

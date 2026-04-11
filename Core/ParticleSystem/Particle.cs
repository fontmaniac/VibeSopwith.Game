using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core.ParticleSystem
{
    internal record struct Particle(Vector2 Position, Vector2 Velocity, float Length, float Height) : IHasLocation, ICanRemoveRigging, IAmBehaving<Unit>
    {
        public Vector2 Direction { get; private set; } = Vector2.Normalize(Velocity);
        public BasisSpin Spin { get; } = BasisSpin.Down;

        public TimeSpan Age = TimeSpan.Zero;

        public Body Body = null!;

        public void SetupRiggingCircle(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Position.ToAether(), Direction.ToAngle(), BodyType.Dynamic);
            body.LinearVelocity = Velocity.ToAether();
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 0.1f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 0.0f;
            body.IgnoreGravity = false;

            var fixture0 = body.CreateCircle(0.01f, 1.0f);
            fixture0.Friction = 0f;
            fixture0.Restitution = 0.05f;
            fixture0.CollisionCategories = GameWorld.WorldCollider.AddCategories("Particle");
            fixture0.CollidesWith = GameWorld.WorldCollider.GetAll() & ~GameWorld.WorldCollider.GetCategories("Particle");

            this.Body = body;
        }

        public void SetupRiggingDroplet(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Position.ToAether(), Direction.ToAngle(), BodyType.Dynamic);
            body.LinearVelocity = Velocity.ToAether();
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 0.0f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 0.0f;
            body.IgnoreGravity = false;

            var ff = Spin.ToFactor();
            var sfx = Length / 0.3f;
            var sfy = Height / 0.3f;

            // Add fixture 0
            var vertices0 = new[]
            {
                (-0.15f * sfx, ff * 0.0833f * sfy),
                (0.0169f * sfx, ff * 0.0841f * sfy),
                (0.1495f * sfx, ff * 0.0086f * sfy),
                (0.1497f * sfx, ff * -0.0161f * sfy),
                (0.0247f * sfx, ff * -0.0669f * sfy),
                (-0.15f * sfx, ff * -0.0674f * sfy)
            };

            var fixture0 = vertices0.ToPolygon(body, density:0f);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.05f;
            fixture0.CollisionCategories = GameWorld.WorldCollider.AddCategories("Particle");
            fixture0.CollidesWith = GameWorld.WorldCollider.GetAll() & ~GameWorld.WorldCollider.GetCategories("Particle");

            this.Body = body;
        }

        public void SetupRigging(World collisionWorld) => SetupRiggingDroplet(collisionWorld);

        public void AdvanceAge(TimeSpan dt) => Age = Age.Add(dt);

        public void RemoveRigging(World collisionWorld)
        {
            collisionWorld.Remove(Body);
            Body = null!;
        }

        public void PreSimulationPrepare(Unit _)
        {
            Body.LinearVelocity = Velocity.ToAether();

            if (Body.ContactList == null)
            {
                var refPos = Position.ToAether();
                Body.SetTransformIgnoreContacts(ref refPos, Direction.ToAngle());
            }
        }

        public void PostSimulationUpdate(Unit _)
        {
            var oldPos = Position;
            Position = Body.Position.ToXna();
            Velocity = Body.LinearVelocity.ToXna();

            var newPos = Position;
            Direction = Vector2.Normalize(newPos - oldPos);

        }

    }
}

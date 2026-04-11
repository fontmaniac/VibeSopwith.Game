using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Utils.ParticleSystem.Special
{
    internal record struct ParticleWaterDroplet(Vector2 Position, Vector2 Velocity, float BaseLength, float BaseHeight, TimeSpan MaxAge) : IHasLocation, ICanRemoveRigging<World>, IAmBehaving<Unit>, IParticle<World>
    {
        public float Length => BaseLength * MathHelper.Clamp(0.5f + Velocity.Length() * 0.01f, 0f, 2f);
        public float Height => BaseHeight;
        public Vector2 Direction { get; private set; } = Vector2.Normalize(Velocity);
        public BasisSpin Spin { get; } = BasisSpin.Down;

        public TimeSpan Age { get; private set; } = TimeSpan.Zero;
        public float AgePct => MaxAge == TimeSpan.Zero ? 1f : (float)Age.Ticks / MaxAge.Ticks;

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

            var fixture0 = body.CreateCircle(0.1f, 1.0f);
            fixture0.Friction = 0.5f;
            fixture0.Restitution = 0.1f;
            fixture0.CollisionCategories = GameWorld.WorldCollider.AddCategories("ParticleWaterDroplet");
            fixture0.CollidesWith = GameWorld.WorldCollider.GetAll() & ~GameWorld.WorldCollider.GetCategories("ParticleWaterDroplet", "Bullet");

            Body = body;
        }

        public void SetupRigging(World collisionWorld) => SetupRiggingCircle(collisionWorld);

        public void AdvanceAge(TimeSpan dt) => Age = Age.Add(dt);

        public void RemoveRigging(World collisionWorld)
        {
            if (Body != null) collisionWorld.Remove(Body);
            Body = null!;
        }

        public void PreSimulationPrepare(Unit _, GameTime g)
        {
            Body.LinearVelocity = Velocity.ToAether();
            var drag = Velocity * -0.01f;
            var extraGravity = Vector2.UnitY * -1.0f;
            Body.ApplyForce((drag + extraGravity).ToAether());

            if (Body.ContactList == null)
            {
                var refPos = Position.ToAether();
                Body.SetTransformIgnoreContacts(ref refPos, Direction.ToAngle());
            }
        }

        public void PostSimulationUpdate(Unit _, GameTime g)
        {
            var oldPos = Position;
            Position = Body.Position.ToXna();
            Velocity = Body.LinearVelocity.ToXna().RotateDeg((float)GameWorld.WorldSeed.NextDouble() * 1f - 0.5f);

            var newPos = Position;
            Direction = Vector2.Normalize(newPos - oldPos);

        }

    }
}

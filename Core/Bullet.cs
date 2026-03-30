using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal class Bullet : ICentered, ISimulated<Unit>
    {
        public Body Body = null!;
        public record State(Vector2 Position, Vector2 Direction, Vector2 Velocity);
        public State CurrentState;

        public Vector2 Position { get => CurrentState.Position; }
        public Vector2 Direction { get => CurrentState.Direction; }
        public BasisSpin Spin => BasisSpin.Down;
        public TimeSpan StartTime;

        public const float BulletLength = 0.05f;
        public const float BulletHeight = 0.02f;

        public float Length => BulletLength;
        public float Height => BulletHeight;

        private TimeSpan Duration = TimeSpan.FromSeconds(1.5);
        public bool IsExpired(TimeSpan gameTime) => StartTime + Duration < gameTime;

        public Bullet(State initialState, TimeSpan startTime)
        {
            CurrentState = initialState;
            StartTime = startTime;
        }

        public Bullet SetupRigging(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Position.ToAether(), Direction.ToAngle(), BodyType.Dynamic);
            body.LinearVelocity = CurrentState.Velocity.ToAether();
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 100f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 50.0f;
            body.IgnoreGravity = true;

            var fixture0 = body.CreateRectangle(BulletLength, BulletHeight, 1f, Vector2.Zero.ToAether());
            fixture0.Friction = 1f;
            fixture0.Restitution = 0.0f;
            fixture0.CollisionCategories = GameWorld.WorldCollider.AddCategories("Bullet");
            fixture0.CollidesWith = GameWorld.WorldCollider.GetAll() & ~GameWorld.WorldCollider.AddCategories("Bullet");

            this.Body = body;

            return this;
        }


        public void PreSimulationPrepare(Unit _)
        {
        }

        public void PostSimulationUpdate(Unit _)
        {
            CurrentState = CurrentState with
            {
                Position = Body.Position.ToXna(),
                Direction = Body.Rotation.ToNormal(),
            };
        }


    }
}

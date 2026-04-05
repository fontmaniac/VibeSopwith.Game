using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal class FlakGun : IHasLocation, ICanRemoveRigging, IAmBehaving<FlakGun.State>
    {
        public Body Body = null!;

        public record State(float BarrelAngle, Bullet? Bullet, BarrelMovement Movement, DateTime BulletTime);
        public State CurrentState;

        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public BasisSpin Spin { get; }
        public float Length => 4f;
        public float Height => 4f;

        public FlakGunBarrel Barrel;

        public bool Exploded = false;

        public enum BarrelMovement { Up, Down };

        private const float MinGunAngle = 30f;          // Degrees.
        private const float MaxGunAngle = 85f;          // Degrees.
        private const float GunAngleChangeRate = 60f;   // Degree per second.

        public FlakGun(Vector2 position, BasisSpin spin)
        {
            Position = position;
            Spin = spin;
            Direction = Vector2.UnitX * (spin == BasisSpin.Down ? +1f : -1f);

            CurrentState = new State((float)GameWorld.WorldSeed.NextDouble() * (MaxGunAngle-MinGunAngle)+MinGunAngle, null, BarrelMovement.Up, DateTime.MinValue);
            Barrel = new FlakGunBarrel(this, new LiveBasis(() => new Vector2(0f, 2f), () => Vector2.UnitX.RotateDeg(CurrentState.BarrelAngle * spin.ToFactor()), () => BasisSpin.Down));
        }

        public void RemoveRigging(World collisionWorld)
        {
            Barrel.RemoveRigging(collisionWorld);
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
                (-1.8906f, ff * 0f),
                (-1.875f, ff * 0.1758f),
                (-1.2578f, ff * 0.3945f),
                (1.2578f, ff * 0.4102f),
                (1.8945f, ff * 0.1602f),
                (1.9219f, ff * 0f)
            };

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;

            // Add fixture 1
            var vertices1 = new[]
            {
                (-1.2539f, ff * 0.3945f),
                (-1.2539f, ff * 0.7695f),
                (-0.8281f, ff * 2.5898f),
                (0.7422f, ff * 2.5938f),
                (1.2969f, ff * 0.7422f),
                (1.2891f, ff * 0.4102f)
            };

            var fixture1 = vertices1.ToPolygon(body);
            fixture1.Friction = 0.0f;
            fixture1.Restitution = 0.0f;

            this.Body = body;

            makeTag = makeTag ?? (() => this);
            Body.Tag = makeTag();

            Barrel.SetupRigging(collisionWorld, makeTag);
        }

        public State ApplyInputs(GameTime gameTime)
        {
            var nowTime = DateTime.UtcNow;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var newAngle = CurrentState.BarrelAngle + (CurrentState.Movement == BarrelMovement.Up ? +1f : -1f) * GunAngleChangeRate * dt;
            var newMovement =
                newAngle >= MaxGunAngle ? BarrelMovement.Down :
                newAngle <= MinGunAngle ? BarrelMovement.Up :
                CurrentState.Movement;

            var (newBullet, newBulletTime) = Exploded || ((nowTime - CurrentState.BulletTime).TotalSeconds < FlakGunBarrel.BulletGracePeriod)
                ? (null, CurrentState.BulletTime)
                : (Barrel.SpawnBullet(gameTime.TotalGameTime), nowTime);

            return new(newAngle, newBullet, newMovement, newBulletTime);
        }

        public void PreSimulationPrepare(State projected)
        {
            if (Barrel.Body == null) return;

            // State must be "accepted" in the PostSimulationUpdate, but for now it is too inconvenient - I have to rethink it.
            CurrentState = projected;
            Barrel.Body.Rotation = Barrel.Direction.ToAngle();
        }

        public void PostSimulationUpdate(State projected)
        {
            
        }
    }
}

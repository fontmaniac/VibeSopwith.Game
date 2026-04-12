using Microsoft.Xna.Framework;
using Nage.Strata.Abstractions.Behavioral;
using Nage.Strata.Abstractions.Spatial;
using Nage.Strata.Physics;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal class Baloon : IHasLocation, IAmBehaving<Vector2>, ICanRemoveRigging
    {
        public Body Body = null!;

        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; }
        public BasisSpin Spin { get; }
        public float Length => 6f;
        public float Height => 6f;

        public bool Exploded = false;

        private Vector2 barrageCenter;

        private const float RPM = 4f;

        public Baloon(Vector2 position, BasisSpin spin, Vector2 barrageCenter)
        {
            Position = position;
            Spin = spin;
            Direction = Vector2.UnitX * (spin == BasisSpin.Down ? +1f : -1f);
            this.barrageCenter = barrageCenter;
        }

        public void RemoveRigging(World simWorld)
        {
            simWorld.Remove(Body);
            Body = null!;
        }

        public void SetupRigging(World simWorld, Func<object>? makeTag = null)
        {
            var body = simWorld.CreateBody(Position.ToAether(), 0f, BodyType.Kinematic);
            body.Rotation = Direction.ToAngle();
            body.Tag = this;
            body.FixedRotation = false;
            body.Mass = 500f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 0.0f;

            var ff = Spin.ToFactor();

            // Add fixture 0
            var vertices0 = new[]
            {
                (0.7383f, ff * 2.8594f),
                (1.9512f, ff * 2.0977f),
                (2.502f, ff * 1.1484f),
                (2.5781f, ff * 0.0234f),
                (1.6992f, ff * -1.5f),
                (0.6504f, ff * -1.8984f),
                (0.1641f, ff * -1.9336f),
                (0.0352f, ff * 2.8535f)
            };

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;

            // Add fixture 1
            var vertices1 = new[]
            {
                (-0.6973f, ff * 2.8594f),
                (0.0293f, ff * 2.8535f),
                (0.1582f, ff * -1.9336f),
                (-0.6973f, ff * -1.8574f),
                (-1.6875f, ff * -1.3301f),
                (-2.4727f, ff * 0.0469f),
                (-2.4434f, ff * 1.1484f),
                (-1.834f, ff * 2.2148f)
            };

            var fixture1 = vertices1.ToPolygon(body);
            fixture1.Friction = 0.0f;
            fixture1.Restitution = 0.0f;

            // Add fixture 2
            var vertices2 = new[]
            {
                (-1.0898f, ff * -1.6348f),
                (0.9785f, ff * -1.6992f),
                (1.2773f, ff * -2.9941f),
                (-1.1309f, ff * -3f)
            };

            var fixture2 = vertices2.ToPolygon(body);
            fixture2.Friction = 0.0f;
            fixture2.Restitution = 0.0f;
            this.Body = body;

            makeTag = makeTag ?? (() => this);
            Body.Tag = makeTag();
        }

        public Vector2 DeriveState(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var barrage = Position - barrageCenter;
            var degreesPerSecond = 360f * RPM / 60f;

            return barrageCenter + barrage.RotateDeg(-degreesPerSecond * dt); ;
        }

        public void PreSimulationPrepare(Vector2 projectedPosition, GameTime gameTime)
        {
            if (Body == null) return;
            Body.Position = Position.ToAether();
        }

        public void PostSimulationUpdate(Vector2 projectedPosition, GameTime gameTime)
        {
            Position = projectedPosition;
        }
    }
}

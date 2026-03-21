using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal class Bomb : ICentered, ISimulated<Unit>
    {
        public Body Body = null!;
        public record State(Vector2 Position, Vector2 Direction, Vector2 Velocity);
        public State CurrentState;

        public Vector2 Position { get => CurrentState.Position; }
        public Vector2 Direction { get => CurrentState.Direction; }
        public Winding NormalDown => Winding.Clockwise;

        public const float BombLength = 1.5f;
        public const float BombHeight = 1.0f;

        public float Length => BombLength;
        public float Height => BombHeight;

        public Bomb(State initialState)
        {
            CurrentState = initialState;
        }

        public Bomb SetupRigging(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Position.ToAether(), Direction.ToAngle(), BodyType.Dynamic);
            body.LinearVelocity = (CurrentState.Velocity * 60f).ToAether();
            body.Tag = this;
            body.FixedRotation = false;
            body.Mass = 100f;
            body.Inertia = 0f;
            body.LinearDamping = 0.0f;
            body.AngularDamping = 5.0f;

            // Add fixture 0
            var fixture0 = new[]
            {
                (-0.24f, 0.5f),
                (0.3900f, 0.5f),
                (0.75f, 0.03f),
                (0.75f, -0.03f),
                (0.3900f, -0.5f),
                (-0.24f, -0.5f)
            }.ToPolygon(body);

            fixture0.Friction = 0.2f;
            fixture0.Restitution = 0.0f;
            fixture0.CollisionCategories = Category.Cat10;
            fixture0.CollidesWith = Category.All;// & ~Category.Cat10;

            // Add fixture 1
            var fixture1 = new[]
            {
                (-0.24f, 0.2625f),
                (-0.69f, 0.4447f),
                (-0.75f, 0.1087f),
                (-0.75f, -0.066f),
                (-0.69f, -0.4087f),
                (-0.24f, -0.1764f)
            }.ToPolygon(body, 0.2f);

            fixture1.Friction = 0.2f;
            fixture1.Restitution = 0.0f;
            fixture1.CollisionCategories = Category.Cat10;
            fixture1.CollidesWith = Category.All & ~Category.Cat10;

            this.Body = body;

            return this;
        }

        public void PreSimulationPrepare(Unit _)
        {
            var velo = Body.LinearVelocity.ToXna();
            var drag = velo * -0.15f;
            var tailPoint = Body.GetWorldPoint(new Vector2(-0.75f, 0f).ToAether());

            // Aether documentation about ApplyForce is incorrect.
            // When 'point' parameter is ommitted the force is applied not to Center-of-Mass, but to Origin [0, 0].
            // In this particular case application of drag to tailPoint doesn't stabilize the bomb on its own
            // and needs to be combined with relatively high AngularDamping setting.
            // AngularDamping = 5.0f seems to produce believable "lock-in" of Direction into Velocity.
            Body.ApplyForce(drag.ToAether(), tailPoint);
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

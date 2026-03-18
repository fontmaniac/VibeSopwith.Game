using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class Bomb : ICentered, ISimulated<Bomb.State>
    {
        public Body Body = null!;
        public record State(Vector2 Position, Vector2 Direction, Vector2 Velocity);
        public State CurrentState;

        public Vector2 Position { get => CurrentState.Position; }
        public Vector2 Direction { get => CurrentState.Direction; }
        public Winding NormalDown => Winding.Clockwise;
        public float Length => 1.5f;
        public float Height => 1.0f;

        public Bomb(State initialState)
        {
            CurrentState = initialState;
        }

        public void SetupRigging(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Position.ToAether(), Direction.ToAngle(), BodyType.Dynamic);
            body.LinearVelocity = (CurrentState.Velocity * 60f).ToAether();
            body.Tag = this;
            body.FixedRotation = false;
            body.Mass = 100f;
            body.Inertia = 0f;
            body.LinearDamping = 0f;
            body.AngularDamping = 8.0f;

            // Add fixture 0
            var vertices0 = new Aether.Vertices();
            vertices0.Add(new Aether.Vector2(-0.75f,  0.5f));
            vertices0.Add(new Aether.Vector2(0.3775f, 0.5f));
            vertices0.Add(new Aether.Vector2(0.7484f, 0.0301f));
            vertices0.Add(new Aether.Vector2(0.7492f, -0.0184f));
            vertices0.Add(new Aether.Vector2(0.3889f, -0.5f));
            vertices0.Add(new Aether.Vector2(-0.75f,  -0.5f));
            var shape0 = new nkast.Aether.Physics2D.Collision.Shapes.PolygonShape(vertices0, 1.0f);
            var fixture0 = body.CreateFixture(shape0);
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;
            fixture0.CollisionCategories = Category.Cat10;
            fixture0.CollidesWith = Category.All;

            this.Body = body;
        }

        public void PreSimulationPrepare(State projected)
        {
            
        }

        public void PostSimulationUpdate(State projected)
        {
            CurrentState = CurrentState with
            {
                Position = Body.Position.ToXna(),
                Direction = Body.Rotation.ToNormal(),
            };
        }
    }
}

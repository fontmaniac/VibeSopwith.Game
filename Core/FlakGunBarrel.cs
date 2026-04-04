using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;


namespace VibeSopwith.Game.Core
{
    internal class FlakGunBarrel : IHasLocation, ICanRemoveRigging
    {
        public Body Body = null!;

        public IBasis Parent { get; }
        // Returned Basis is now "World" basis.
        public Vector2 Position { get => Parent.Position + _localPosition; }
        public Vector2 Direction { get => Parent.Direction.Rotate(_localDirection.ToAngle()); }
        // Spin is inherited from Parent at all times.
        public BasisSpin Spin { get => Parent.Spin; }
        public float Length => 8f;
        public float Height => 4f;

        private Vector2 _localPosition;
        private Vector2 _localDirection;

        public FlakGunBarrel(IBasis parent, Vector2 localPosition, Vector2 localDirection)
        {
            Parent = parent;
            _localPosition = localPosition;
            _localDirection = localDirection;
        }

        public void RemoveRigging(World collisionWorld)
        {
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


            // Insert fixtures here

            this.Body = body;

            makeTag = makeTag ?? (() => this);
            Body.Tag = makeTag();
        }

    }
}

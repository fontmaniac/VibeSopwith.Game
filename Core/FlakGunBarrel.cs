using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;


namespace VibeSopwith.Game.Core
{
    internal class FlakGunBarrel : IHasLocation, ICanRemoveRigging
    {
        public Body Body = null!;

        public IBasis Parent { get; }
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public BasisSpin Spin { get; } = BasisSpin.Down;    // Since it is relative to parent, it is always default (Down). 
        public float Length => 8f;
        public float Height => 4f;

        public FlakGunBarrel(IBasis parent, Vector2 position)
        {
            Parent = parent;
            Position = position;
            // This renders correctly but doesn't fit my reasoning model. So, if the base is spin-UP then direction of barrel is locally-opposite to world-direction of base?
            // My intuition doesn't fit.
            Direction = parent.Direction * (parent.Spin == BasisSpin.Down ? +1f : -1f);
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

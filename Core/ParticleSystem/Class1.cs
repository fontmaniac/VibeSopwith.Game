using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game.Core.ParticleSystem
{
    internal class APrototype : IBasis, ICanRemoveRigging, IAmBehaving<bool>
    {
        World CollisionWorld = null!;

        public IBasis WorldLocation { get; }
        public Vector2 Position { get => WorldLocation.Position; }
        public Vector2 Direction { get => WorldLocation.Direction; }
        public BasisSpin Spin { get => WorldLocation.Spin; }

        public bool IsEmitting { get; private set; } = false;
        public readonly float EmissionRate = 0f;

        public APrototype(IBasis WorldPosition, float emissionRate)
        {
            WorldLocation = WorldPosition;
            EmissionRate = emissionRate;
        }

        public void RemoveRigging(World collisionWorld)
        {
            CollisionWorld = null!;
        }

        public void SetupRigging(World collisionWorld)
        {
            CollisionWorld = collisionWorld;
        }

        public bool DeriveState(bool emit, GameTime gameTime)
        {
            return false;
        }

        public void PreSimulationPrepare(bool emitting)
        {
        }

        public void PostSimulationUpdate(bool emitting)
        {
            IsEmitting = emitting;
        }

    }
}

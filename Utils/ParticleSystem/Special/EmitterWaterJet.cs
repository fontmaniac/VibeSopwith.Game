using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Utils.ParticleSystem.Special
{
    internal class EmitterWaterJet : EmitterSingleSource<ParticleWaterDroplet, World>
    {
        public readonly float InitialVelocity;
        public readonly float MidAge;
        public readonly float DeltaAge;

        public EmitterWaterJet(IBasis worldLocation, float emissionRate, float initialVelocity, float midAge, float deltaAge) : base(worldLocation, emissionRate)
        {
            InitialVelocity = initialVelocity;
            MidAge = midAge;
            DeltaAge = deltaAge;

            MakeParticle = MakeDroplet;
        }

        private ParticleWaterDroplet MakeDroplet(GameTime gameTime, int batchSize, int batchIndex)
        {
            var linearFactor = InitialVelocity * ((float)GameWorld.WorldSeed.NextDouble() * 0.2f - 0.1f + 1f);
            var angleFactor = (float)GameWorld.WorldSeed.NextDouble() * 1f - 0.5f;
            var velocity = Direction.RotateDeg(angleFactor) * linearFactor;

            var ageFactor = (float)GameWorld.WorldSeed.NextDouble() * 2f * DeltaAge - DeltaAge;

            return new ParticleWaterDroplet(Position, velocity, 0.6f, 0.6f, TimeSpan.FromSeconds(MidAge + ageFactor));
        }
    }
}

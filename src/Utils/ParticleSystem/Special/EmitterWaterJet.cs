using Microsoft.Xna.Framework;
using Nage.Strata.Abstractions.Infra;
using Nage.Strata.Abstractions.Spatial;
using Nage.Strata.Physics;
using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game.Utils.ParticleSystem.Special
{
    internal class EmitterWaterJet : EmitterSingleSource<ParticleWaterDroplet, World>
    {
        public readonly float InitialVelocity;
        public readonly float MidAge;
        public readonly float DeltaAge;
        public readonly float SprayConeDegrees;

        public EmitterWaterJet(IBasis worldLocation, float emissionRate, float initialVelocity, float midAge, float deltaAge, float sprayConeDegrees) : base(worldLocation, emissionRate)
        {
            InitialVelocity = initialVelocity;
            MidAge = midAge;
            DeltaAge = deltaAge;

            MakeParticle = MakeDroplet;
            SprayConeDegrees = sprayConeDegrees;
        }

        private ParticleWaterDroplet MakeDroplet(GameTime gameTime, int batchSize, int batchIndex)
        {
            var linearFactor = InitialVelocity * ((float)Globs.World.Seed.NextDouble() * 0.2f - 0.1f + 1f);
            var angleFactor = (float)Globs.World.Seed.NextDouble() * SprayConeDegrees - SprayConeDegrees / 2f;
            var velocity = Source.Direction.RotateDeg(angleFactor) * linearFactor;

            var ageFactor = (float)Globs.World.Seed.NextDouble() * 2f * DeltaAge - DeltaAge;

            return new ParticleWaterDroplet(Source.Position, velocity, 0.6f, 0.6f, TimeSpan.FromSeconds(MidAge + ageFactor));
        }
    }
}

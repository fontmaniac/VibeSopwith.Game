using Microsoft.Xna.Framework;
using Nage.Strata.Abstractions.Behavioral;
using Nage.Strata.Types;

namespace VibeSopwith.Game.Utils.ParticleSystem
{
    public interface IParticleSystem<TSimWorld> : ICanRemoveRigging<TSimWorld>, IAmBehaving<Unit>
    {
        IParticleSystem<TSimWorld> SetupRigging(TSimWorld simWorld);
        void EmitParticles(GameTime gameTime);
        bool IsExpired { get; }
    }

    public interface IParticle<TSimWorld>
    {
        TimeSpan Age { get; }
        TimeSpan MaxAge { get; }
        void AdvanceAge(TimeSpan dt);

        void SetupRigging(TSimWorld simWorld);
    }
}

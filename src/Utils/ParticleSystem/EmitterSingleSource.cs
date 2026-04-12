using Microsoft.Xna.Framework;
using Nage.Strata.Types;

namespace VibeSopwith.Game.Utils.ParticleSystem
{
    internal class EmitterSingleSource<TParticle, TSimWorld> 
        : IParticleSystem<TSimWorld> 
        where TParticle 
            : ICanRemoveRigging<TSimWorld>
            , IAmBehaving<Unit>
            , IParticle<TSimWorld>
    {
        private TSimWorld SimWorld = default(TSimWorld)!;

        protected IBasis Source { get; }
        protected Func<GameTime, int, int, TParticle> MakeParticle = null!;

        private readonly float _emissionRate;
        private TimeSpan? _emissionStart = null;

        private AutoGrowArray<TParticle> _particles = null!;
        private int _totalParticlesEmitted = 0;

        public bool IsExpired { get => _emissionStart != null && _particles.Length == 0; }

        public ReadOnlySpan<TParticle> Particles => _particles.ReadOnlyItems;

        public EmitterSingleSource(IBasis worldLocation, float emissionRate)
        {
            Source = worldLocation;
            _emissionRate = emissionRate;
            _particles = new AutoGrowArray<TParticle>((int)_emissionRate);    // Enough for one second.
        }

        public void RemoveRigging(TSimWorld simWorld)
        {
            for (var i = 0; i < _particles.Length; ++i)
                _particles[i].RemoveRigging(SimWorld);
            SimWorld = default(TSimWorld)!;
        }

        public IParticleSystem<TSimWorld> SetupRigging(TSimWorld simWorld)
        {
            SimWorld = simWorld;
            return this;
        }

        public void EmitParticles(GameTime gameTime)
        {
            _emissionStart = _emissionStart == null ? gameTime.TotalGameTime - gameTime.ElapsedGameTime : _emissionStart;
            var dt = (gameTime.TotalGameTime - _emissionStart.Value).TotalSeconds;
            var particlesToEmit = (int)(dt * _emissionRate - _totalParticlesEmitted);

            for (var i = 0; i < particlesToEmit; ++i)
            {
                var particle = MakeParticle(gameTime, particlesToEmit, i);
                particle.SetupRigging(SimWorld);
                _particles.Add(particle);
                _totalParticlesEmitted++;
            }
        }

        public void PreSimulationPrepare(Unit _, GameTime gameTime)
        {
            for (var i = 0; i < _particles.Length; ++i)
                _particles[i].PreSimulationPrepare(Unit.Value, gameTime);
        }

        public void PostSimulationUpdate(Unit _, GameTime gameTime)
        {
            for (var i = 0; i < _particles.Length;)
            {
                ref var particle = ref _particles[i];
                particle.PostSimulationUpdate(Unit.Value, gameTime);
                particle.AdvanceAge(gameTime.ElapsedGameTime);
                if (particle.Age <= particle.MaxAge)
                {
                    i++;
                    continue;
                }

                particle.RemoveRigging(SimWorld);
                _particles.RemoveAt(i);

            }
        }

    }
}

using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Utils.ParticleSystem
{
    internal class EmitterSingleSource<TParticle, TSimWorld> : IBasis, IParticleSystem<TSimWorld> where TParticle : ICanRemoveRigging<TSimWorld>, IAmBehaving<Unit>, IParticle<TSimWorld>
    {
        TSimWorld CollisionWorld = default(TSimWorld)!;

        public IBasis WorldLocation { get; }
        public Vector2 Position { get => WorldLocation.Position; }
        public Vector2 Direction { get => WorldLocation.Direction; }
        public BasisSpin Spin { get => WorldLocation.Spin; }

        protected Func<GameTime, int, int, TParticle> MakeParticle = null!;

        private readonly float _emissionRate;
        private TimeSpan? _emissionStart = null;

        private AutoGrowArray<TParticle> _particles = null!;
        private int _totalParticlesEmitted = 0;

        public bool IsExpired { get => _emissionStart != null && _particles.Length == 0; }

        public ReadOnlySpan<TParticle> Particles => _particles.ReadOnlyItems;

        public EmitterSingleSource(IBasis worldLocation, float emissionRate)
        {
            WorldLocation = worldLocation;
            _emissionRate = emissionRate;
            _particles = new AutoGrowArray<TParticle>((int)_emissionRate);    // Enough for one second.
        }

        public void RemoveRigging(TSimWorld collisionWorld)
        {
            for (var i = 0; i < _particles.Length; ++i)
                _particles[i].RemoveRigging(CollisionWorld);
            CollisionWorld = default(TSimWorld)!;
        }

        public IParticleSystem<TSimWorld> SetupRigging(TSimWorld collisionWorld)
        {
            CollisionWorld = collisionWorld;
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
                particle.SetupRigging(CollisionWorld);
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

                particle.RemoveRigging(CollisionWorld);
                _particles.RemoveAt(i);

            }
        }

    }
}

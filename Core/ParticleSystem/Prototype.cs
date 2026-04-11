using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core.ParticleSystem
{
    internal class Prototype : IBasis, ICanRemoveRigging, IAmBehaving<Unit>
    {
        World CollisionWorld = null!;

        public IBasis WorldLocation { get; }
        public Vector2 Position { get => WorldLocation.Position; }
        public Vector2 Direction { get => WorldLocation.Direction; }
        public BasisSpin Spin { get => WorldLocation.Spin; }

        public readonly float EmissionRate;
        private TimeSpan? _emissionStart = null;

        private Particle[] _particles = null!;
        private int _particlesUsed = 0;
        private int _totalParticlesEmitted = 0;

        public bool IsExpired { get => _emissionStart != null && _particlesUsed == 0; }

        public ReadOnlySpan<Particle> Particles => _particles.AsSpan(0, _particlesUsed);

        public Prototype(IBasis WorldPosition, float emissionRate)
        {
            WorldLocation = WorldPosition;
            EmissionRate = emissionRate;
            _particles = new Particle[Math.Max(16, (int)EmissionRate)];    // Enough for one second.
        }

        public void RemoveRigging(World collisionWorld)
        {
            CollisionWorld = null!;
        }

        public Prototype SetupRigging(World collisionWorld)
        {
            CollisionWorld = collisionWorld;
            return this;
        }

        public void RemoveParticles(GameTime gameTime)
        {
            var age = TimeSpan.FromSeconds(5);

            for (var i = 0; i < _particlesUsed; )
            {
                ref var particle = ref _particles[i];
                particle.AdvanceAge(gameTime.ElapsedGameTime);
                if (particle.Age <= age) 
                { 
                    i++;  
                    continue; 
                }

                particle.RemoveRigging(CollisionWorld);
                particle = _particles[--_particlesUsed]; // Copy the last particle here and forget about it.
            }
        }

        private void AddParticle(Particle particle)
        {
            if (_particlesUsed == _particles.Length)
                Array.Resize(ref _particles, _particles.Length + Math.Max(16, (int)EmissionRate)); // Add another second worth of capacity.

            _particles[_particlesUsed++] = particle;
        }


        public void EmitParticles(GameTime gameTime)
        {
            _emissionStart = _emissionStart == null ? gameTime.TotalGameTime - gameTime.ElapsedGameTime : _emissionStart;
            var dt = (gameTime.TotalGameTime - _emissionStart.Value).TotalSeconds;
            var particlesToEmit = (int)(dt * EmissionRate - _totalParticlesEmitted);

            for (var i = 0; i < particlesToEmit; ++i)
            {
                var randomFactor = 24f * (((float)GameWorld.WorldSeed.NextDouble() * 0.2f - 0.1f) + 1f);
                var velocity = Direction.RotateDeg((float)GameWorld.WorldSeed.NextDouble() * 1f - 0.5f) * randomFactor;
                var particle = new Particle(Position, velocity, 0.6f, 0.6f);
                particle.SetupRigging(CollisionWorld);
                AddParticle(particle);
                _totalParticlesEmitted++;
            }
        }

        public void PreSimulationPrepare(Unit _)
        {
            for (var i = 0; i < _particlesUsed; ++i)
                _particles[i].PreSimulationPrepare(Unit.Value);
        }

        public void PostSimulationUpdate(Unit _)
        {
            for (var i = 0; i < _particlesUsed; ++i)
                _particles[i].PostSimulationUpdate(Unit.Value);
        }

    }
}

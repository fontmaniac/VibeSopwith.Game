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

        private AutoGrowArray<Particle> _particles = null!;
        private int _totalParticlesEmitted = 0;

        public bool IsExpired { get => _emissionStart != null && _particles.Length == 0; }

        public ReadOnlySpan<Particle> Particles => _particles.ReadOnlyItems;

        public Prototype(IBasis WorldPosition, float emissionRate)
        {
            WorldLocation = WorldPosition;
            EmissionRate = emissionRate;
            _particles = new AutoGrowArray<Particle>((int)EmissionRate);    // Enough for one second.
        }

        public void RemoveRigging(World collisionWorld)
        {
            for (var i = 0; i < _particles.Length; ++i)
                _particles[i].RemoveRigging(CollisionWorld);
            CollisionWorld = null!;
        }

        public Prototype SetupRigging(World collisionWorld)
        {
            CollisionWorld = collisionWorld;
            return this;
        }

        public void EmitParticles(GameTime gameTime)
        {
            _emissionStart = _emissionStart == null ? gameTime.TotalGameTime - gameTime.ElapsedGameTime : _emissionStart;
            var dt = (gameTime.TotalGameTime - _emissionStart.Value).TotalSeconds;
            var particlesToEmit = (int)(dt * EmissionRate - _totalParticlesEmitted);

            for (var i = 0; i < particlesToEmit; ++i)
            {
                var linearFactor = 54f * (((float)GameWorld.WorldSeed.NextDouble() * 0.2f - 0.1f) + 1f);
                var angleFactor = (float)GameWorld.WorldSeed.NextDouble() * 1f - 0.5f;
                var velocity = Direction.RotateDeg(angleFactor) * linearFactor;

                var ageFactor = (float)GameWorld.WorldSeed.NextDouble() * 4f - 2f;

                var particle = new Particle(Position, velocity, 0.6f, 0.6f, TimeSpan.FromSeconds(3f + ageFactor));
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

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

        public IList<Particle> Particles = new List<Particle>();
        private int _totalParticlesEmitted = 0;

        public bool IsExpired { get => _emissionStart != null && Particles.Count == 0; }

        public Prototype(IBasis WorldPosition, float emissionRate)
        {
            WorldLocation = WorldPosition;
            EmissionRate = emissionRate;
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

            var indicesToRemove = new List<int>();
            for (var i = 0; i < Particles.Count; ++i)
            {
                var particle = Particles[i];
                particle.AdvanceAge((float)gameTime.ElapsedGameTime.TotalSeconds);
                if (particle.Age > age)
                {
                    particle.RemoveRigging(CollisionWorld);
                    indicesToRemove.Add(i);
                }
                Particles[i] = particle;
            }

            foreach (var i in indicesToRemove.AsEnumerable().Reverse())
                Particles.RemoveAt(i);
        }

        public void EmitParticles(GameTime gameTime)
        {
            _emissionStart = _emissionStart == null ? gameTime.TotalGameTime - gameTime.ElapsedGameTime : _emissionStart;
            var dt = (gameTime.TotalGameTime - _emissionStart.Value).TotalSeconds;
            var particlesToEmit = (int)(dt * EmissionRate - _totalParticlesEmitted);

            for (var i = 0; i < particlesToEmit; ++i)
            {
                var randomFactor = 26f * (((float)GameWorld.WorldSeed.NextDouble() * 0.2f - 0.1f) + 1f);
                var velocity = Vector2.Normalize(Direction.RotateDeg((float)GameWorld.WorldSeed.NextDouble() * 1f - 0.5f)) * randomFactor;
                var particle = new Particle(Position, velocity, 0.6f, 0.6f);
                particle.SetupRigging(CollisionWorld);
                Particles.Add(particle);
                _totalParticlesEmitted++;
            }
        }

        public void PreSimulationPrepare(Unit _)
        {
            foreach (var particle in Particles)
                particle.PreSimulationPrepare(Unit.Value);
        }

        public void PostSimulationUpdate(Unit _)
        {
            Particles = Particles.Select(particle =>
            {
                particle.PostSimulationUpdate(Unit.Value);
                return particle;
            })
            .ToList();
                
        }

    }
}

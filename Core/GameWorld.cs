using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class GameWorld 
    {
        public const int WorldLength = 1000;
        public const int WorldHeight = 50;

        public static readonly Random WorldSeed = new Random(12345);

        public readonly Ground Ground;
        public Airplane Plane = null!;
        private readonly List<Explosion> explosions = new List<Explosion>();
        private Explosion? planeExplosion = null; 
        public IEnumerable<Explosion> Explosions 
        { 
            get
            { 
                if (planeExplosion != null) yield return planeExplosion;
                foreach (var exp in explosions) yield return exp;
            } 
        }

        public readonly List<Bomb> Bombs = new List<Bomb>();

        private readonly World collisionWorld;

        public GameWorld()
        {
            //Ground = Ground.MakeFlat(0.25f); 
            Ground = Ground.MakeRandom();

            collisionWorld = new World()
            {
                Gravity = Aether.Vector2.Zero
            };

            Ground.SetupRigging(collisionWorld);

            MakeNewPlane();
        }

        private void MakeNewPlane()
        {
            Plane = new Airplane();
            Plane.Place(new Vector2(WorldLength / 2f, WorldHeight / 2f), Winding.Clockwise);
            Plane.SetupRigging(collisionWorld);
            Plane.Body.Position = Plane.Position.ToAether();
            Plane.Body.Rotation = Plane.Direction.ToAngle();
        }

        public void Simulate(GameTime gameTime)
        {
            if (Plane.Exploded)
            {
                if (planeExplosion?.IsExpired(gameTime.TotalGameTime) == true)
                {
                    planeExplosion = null;
                    MakeNewPlane();
                }
                return;
            }

            var planeProjected = Plane.ApplyInputs(gameTime);
            
            // Spawn a bomb, if requested.
            if (planeProjected.launchingBomb)
            {
                // Bomb spawned half-height off the plane Position in direction of NormalDown, with initial Direction equal to Plane's.
                var bomb = new Bomb(new Bomb.State(Vector2.Zero, Plane.Direction, Vector2.Zero));
                var spawnPos = Plane.Position + Plane.Direction.Rotate(float.Pi / 2f * (Plane.NormalDown == Winding.Clockwise ? -1f : +1f)) * bomb.Height * 0.5f;
                bomb.CurrentState = bomb.CurrentState with { Position = spawnPos };
                Bombs.Add(bomb);
            }

            Plane.PreSimulationPrepare(planeProjected);

            collisionWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            Plane.PostSimulationUpdate(planeProjected);
            Plane.ClearInputs();

            var expiredExplosions = explosions.Where(e => e.IsExpired(gameTime.TotalGameTime));
            foreach (var ee in expiredExplosions)
                explosions.Remove(ee);

            foreach (var ct in collisionWorld.ContactList)
            {
                if (ct.IsTouching)
                {
                    ct.GetWorldManifold(out var normal, out var points);
                    var contactPoint = points[0]; // first contact point
                    Console.WriteLine($"Collision detected at {contactPoint.X} - {contactPoint.Y}. Total points {ct.Manifold.PointCount}");

                    // Set plane to Exploded
                    Plane.Exploded = true;

                    // Remove world body
                    Plane.RemoveRigging(collisionWorld);

                    // Add explosion.
                    planeExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
                    planeExplosion.RootPosition = new Vector2(contactPoint.X, contactPoint.Y);
                    return;
                }
            }
        }

    }
}

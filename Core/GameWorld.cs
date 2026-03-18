using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
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
                Gravity = (-Vector2.UnitY * 10f).ToAether(),
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
                var bomb = new Bomb(new Bomb.State(Vector2.Zero, Plane.Direction, Plane.Direction * Plane.Speed));
                var spawnPos = Plane.Position + Plane.Direction.Rotate(float.Pi / 2f * (Plane.NormalDown == Winding.Clockwise ? -1f : +1f)) * bomb.Height * 0.6f;
                bomb.CurrentState = bomb.CurrentState with { Position = spawnPos };
                bomb.SetupRigging(collisionWorld);
                Bombs.Add(bomb);
            }

            Plane.PreSimulationPrepare(planeProjected);

            collisionWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            Plane.PostSimulationUpdate(planeProjected);
            Plane.ClearInputs();

            foreach (var bomb in Bombs)
                bomb.PostSimulationUpdate(bomb.CurrentState);

            var expiredExplosions = explosions.Where(e => e.IsExpired(gameTime.TotalGameTime)).ToArray();
            foreach (var ee in expiredExplosions)
                explosions.Remove(ee);

            var postCheckActions = new List<Action>();

            for (Contact ct = collisionWorld.ContactList; ct != null; ct = ct.Next)
            {
                if (ct.IsTouching && ((ct.FixtureA.Body.Tag == Plane && ct.FixtureB.Body.Tag == Ground) || (ct.FixtureA.Body.Tag == Ground && ct.FixtureB.Body.Tag == Plane)))
                {
                    ct.GetWorldManifold(out var normal, out var points);
                    var contactPoint = points[0]; // first contact point
                    Console.WriteLine($"Plane-Ground collision detected at {contactPoint.X} - {contactPoint.Y}. Total points {ct.Manifold.PointCount}");

                    // Set plane to Exploded
                    Plane.Exploded = true;

                    // Remove world body
                    postCheckActions.Add(() => { Plane.RemoveRigging(collisionWorld); });

                    // Add explosion.
                    planeExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
                    planeExplosion.RootPosition = new Vector2(contactPoint.X, contactPoint.Y);
                }
                else if (ct.IsTouching && ((ct.FixtureA.Body.Tag is Bomb && ct.FixtureB.Body.Tag == Ground) || (ct.FixtureA.Body.Tag == Ground && ct.FixtureB.Body.Tag is Bomb)))
                {
                    ct.GetWorldManifold(out var normal, out var points);
                    var contactPoint = points[0]; // first contact point
                    Console.WriteLine($"Bomb-Ground collision detected at {contactPoint.X} - {contactPoint.Y}. Total points {ct.Manifold.PointCount}");

                    // Add explosion.
                    var bombExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
                    bombExplosion.RootPosition = new Vector2(contactPoint.X, contactPoint.Y);
                    explosions.Add(bombExplosion);

                    var bomb = Bombs.FirstOrDefault(b => (ct.FixtureA.Body.Tag as Bomb ?? ct.FixtureB.Body.Tag as Bomb) == b);
                    if (bomb != null)
                    {
                        postCheckActions.Add(() => { collisionWorld.Remove(bomb.Body); });
                        Bombs.Remove(bomb);
                    }
                }

                if (ct.Next == collisionWorld.ContactList)
                    break;
            }

            foreach (var postAction in postCheckActions) postAction();
        }

    }
}

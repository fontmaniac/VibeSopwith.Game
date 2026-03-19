using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game.Core
{
    internal class GameWorld 
    {
        public const int WorldLength = 1000;
        public const int WorldHeight = 50;

        public static readonly Random WorldSeed = new Random(12345);

        public readonly Ground Ground;
        public Airplane Plane;
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
            collisionWorld = new World()
            {
                Gravity = (-Vector2.UnitY * 10f).ToAether(),
            };

            Ground = Ground.MakeFlat(0.1f); 
            //Ground = Ground.MakeRandom();
            Ground.SetupRigging(collisionWorld);
            Plane = MakeNewPlane();
        }

        private Airplane MakeNewPlane()
        {
            var plane = new Airplane();
            plane.Place(new Vector2(WorldLength / 2f, WorldHeight * 0.9f), Winding.Clockwise);
            plane.SetupRigging(collisionWorld);
            plane.Body.Position = plane.Position.ToAether();
            plane.Body.Rotation = plane.Direction.ToAngle();
            return plane;
        }

        private void ExecutePlaneCollision(Contact ct, GameTime gameTime, Stack<Action> postCheckActions)
        {
            ct.GetWorldManifold(out var normal, out var points);
            var contactPoint = points[0]; // first contact point
            Console.WriteLine($"Plane-Ground collision detected at {contactPoint.X} - {contactPoint.Y}. Total points {ct.Manifold.PointCount}");

            if (Plane.Exploded) return;
            // Remove world body
            postCheckActions.Push(() => { Plane.RemoveRigging(collisionWorld); });

            Plane.Exploded = true;

            // Add explosion.
            planeExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
            planeExplosion.RootPosition = new Vector2(contactPoint.X, contactPoint.Y);
        }

        private void ExecuteBombGroundExplosion(Contact ct, GameTime gameTime, Stack<Action> postCheckActions)
        {
            ct.GetWorldManifold(out var normal, out var points);
            var contactPoint = points[0]; // first contact point
            Console.WriteLine($"Bomb-Ground collision detected at {contactPoint.X} - {contactPoint.Y}. Total points {ct.Manifold.PointCount}");

            var bomb = Bombs.FirstOrDefault(b => (ct.FixtureA.Body.Tag as Bomb ?? ct.FixtureB.Body.Tag as Bomb) == b);
            if (bomb == null) return;
            // Remove world body
            postCheckActions.Push(() => { collisionWorld.Remove(bomb.Body); });
            Bombs.Remove(bomb);

            // Add explosion.
            var bombExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
            bombExplosion.RootPosition = new Vector2(contactPoint.X, contactPoint.Y);
            explosions.Add(bombExplosion);

        }

        private static bool IsCollision(Contact ct, Func<Fixture, bool> check1, Func<Fixture, bool> check2)
        {
            if (!ct.IsTouching) return false;
            if (!((check1(ct.FixtureA) && check2(ct.FixtureB)) || (check1(ct.FixtureB) && check2(ct.FixtureA)))) return false;
            return true;
        }


        public void Simulate(GameTime gameTime)
        {
            var doBeforeSimulation = () =>
            {
                if (Plane.Exploded)
                {
                    if (planeExplosion?.IsExpired(gameTime.TotalGameTime) == true)
                    {
                        planeExplosion = null;
                        Plane = MakeNewPlane();
                    }

                    return () => { };
                }
                else
                {
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

                    return () =>
                    {
                        Plane.PostSimulationUpdate(planeProjected);
                        Plane.ClearInputs();
                    };
                }
            };

            // Do plane-dependent things before simulation
            var planePostSimulation = doBeforeSimulation();

            foreach (var bomb in Bombs)
                bomb.PreSimulationPrepare(Unit.Value);

            // Simulate
            collisionWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            // Update plane state if not exploded
            planePostSimulation();

            // Update bomb states
            foreach (var bomb in Bombs)
                bomb.PostSimulationUpdate(Unit.Value);

            // Get rid of expired explosions
            var expiredExplosions = explosions.Where(e => e.IsExpired(gameTime.TotalGameTime)).ToArray();
            foreach (var ee in expiredExplosions)
                explosions.Remove(ee);

            // Collision check round.
            var postCheckActions = new Stack<Action>(); // Push here actions which manipulate collisionWorld, to avoid corruption mid-iteration.

            for (Contact ct = collisionWorld.ContactList.Next; ct != collisionWorld.ContactList; ct = ct.Next)
            {
                if (IsCollision(ct, f => f.Body.Tag is Airplane, f => f.Body.Tag is Ground))
                    ExecutePlaneCollision(ct, gameTime, postCheckActions);
                else if (IsCollision(ct, f => f.Body.Tag is Bomb, f => f.Body.Tag is Ground))
                    ExecuteBombGroundExplosion(ct, gameTime, postCheckActions);
            }

            foreach (var postAction in postCheckActions) postAction();
        }

    }
}

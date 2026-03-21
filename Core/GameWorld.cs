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

        private nkast.Aether.Physics2D.Common.Vector2 PrintContactLog(Contact ct, string contactType)
        {
            ct.GetWorldManifold(out var normal, out var points);
            var contactPoint = points[0]; // first contact point
            Console.WriteLine($"{contactType} collision detected at {contactPoint.X} - {contactPoint.Y}. Total points {ct.Manifold.PointCount}");
            return contactPoint;
        }

        private void ExecutePlaneGroundCollision(Contact ct, GameTime gameTime, Stack<Action> postCheckActions)
        {
            var cp = PrintContactLog(ct, "Plane-Ground");

            if (Plane.Exploded) return;
            
            // Remove world body
            postCheckActions.Push(() => { Plane.RemoveRigging(collisionWorld); });

            Plane.Exploded = true;

            // Add explosion.
            planeExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime) { RootPosition = new Vector2(cp.X, cp.Y) };
        }

        private void ExecuteBombGroundExplosion(Contact ct, GameTime gameTime, Stack<Action> postCheckActions)
        {
            var cp = PrintContactLog(ct, "Bomb-Ground");

            var bomb = Bombs.FirstOrDefault(b => (ct.FixtureA.Body.Tag as Bomb ?? ct.FixtureB.Body.Tag as Bomb) == b);
            if (bomb == null) return;
            // Remove world body
            postCheckActions.Push(() => { collisionWorld.Remove(bomb.Body); });
            Bombs.Remove(bomb);

            // Add explosion.
            var bombExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
            bombExplosion.RootPosition = new Vector2(cp.X, cp.Y);
            explosions.Add(bombExplosion);

        }

        private void ExecuteBombPlaneExplosion(Contact ct, GameTime gameTime, Stack<Action> postCheckActions)
        {
            var cp = PrintContactLog(ct, "Bomb-Plane");

            var bomb = Bombs.FirstOrDefault(b => (ct.FixtureA.Body.Tag as Bomb ?? ct.FixtureB.Body.Tag as Bomb) == b);
            if (bomb == null) return;
            // Remove world Bomb body
            postCheckActions.Push(() => { collisionWorld.Remove(bomb.Body); });
            Bombs.Remove(bomb);

            if (Plane.Exploded) return;
            // Remove world Plane body
            postCheckActions.Push(() => { Plane.RemoveRigging(collisionWorld); });

            Plane.Exploded = true;

            // Add explosions - Plane & bomb together.
            planeExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
            planeExplosion.RootPosition = new Vector2(cp.X, cp.Y);
            var bombExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
            bombExplosion.RootPosition = new Vector2(cp.X, cp.Y);
            explosions.Add(bombExplosion);
        }


        private void ExecuteBombBombExplosion(Contact ct, GameTime gameTime, Stack<Action> postCheckActions)
        {
            var cp = PrintContactLog(ct, "Bomb-Bomb");

            var bomb1 = Bombs.FirstOrDefault(b => (ct.FixtureA.Body.Tag as Bomb) == b);
            var bomb2 = Bombs.FirstOrDefault(b => (ct.FixtureB.Body.Tag as Bomb) == b);
            if (bomb1 == null || bomb2 == null) return;
            // Remove world bodies
            postCheckActions.Push(() => { collisionWorld.Remove(bomb1.Body); });
            postCheckActions.Push(() => { collisionWorld.Remove(bomb2.Body); });
            Bombs.Remove(bomb1);
            Bombs.Remove(bomb2);

            // Add explosion.
            var bombExplosion = new Explosion(16f, 16f, gameTime.TotalGameTime);
            bombExplosion.RootPosition = new Vector2(cp.X, cp.Y);
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

                    if (planeProjected.Bomb != null)
                    {
                        var bomb = planeProjected.Bomb;
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
                    ExecutePlaneGroundCollision(ct, gameTime, postCheckActions);
                else if (IsCollision(ct, f => f.Body.Tag is Bomb, f => f.Body.Tag is Ground))
                    ExecuteBombGroundExplosion(ct, gameTime, postCheckActions);
                else if (IsCollision(ct, f => f.Body.Tag is Bomb, f => f.Body.Tag is Bomb))
                    ExecuteBombBombExplosion(ct, gameTime, postCheckActions);
                else if (IsCollision(ct, f => f.Body.Tag is Airplane, f => f.Body.Tag is Bomb))
                    ExecuteBombPlaneExplosion(ct, gameTime, postCheckActions);
            }

            foreach (var postAction in postCheckActions) postAction();
        }

    }
}

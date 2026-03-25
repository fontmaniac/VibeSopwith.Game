using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using VibeSopwith.Game.Utils;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class GameWorld 
    {
        public const int WorldLength = 600;
        public const int WorldHeight = 50;

        public static readonly Random WorldSeed = new Random(12345);

        public readonly Ground Ground;
        public Airplane Plane;
        private readonly List<Explosion> explosions = new List<Explosion>();
        private Explosion? planeExplosion = null; 
        public IEnumerable<Explosion> GetExplosions()
        { 
            if (planeExplosion != null) yield return planeExplosion;
            foreach (var exp in explosions) yield return exp;
        }

        public readonly List<Bomb> Bombs = new List<Bomb>();
        public readonly List<Bullet> Bullets = new List<Bullet>();

        public readonly List<StaticBuilding> Buildings = new List<StaticBuilding>();

        private readonly World collisionWorld;

        public GameWorld()
        {
            collisionWorld = new World()
            {
                Gravity = (-Vector2.UnitY * 10f).ToAether(),
            };

            //Ground = Ground.MakeFlat(0.1f); 
            //Ground = Ground.MakeRandom();
            //Ground = Ground.MakeCustom();

            (Ground, Buildings) = Ground.MakeWithPlatforms();

            Ground.SetupRigging(collisionWorld);
            foreach (var building in Buildings)
                building.SetupRigging(collisionWorld);
            Plane = MakeNewPlane();
        }

        private Airplane MakeNewPlane()
        {
            var plane = new Airplane();
            plane.Place(new Vector2(WorldLength / 2f, WorldHeight * 0.9f), BasisSpin.Down);
            plane.SetupRigging(collisionWorld);
            plane.Body.Position = plane.Position.ToAether();
            plane.Body.Rotation = plane.Direction.ToAngle();
            return plane;
        }

        private static Explosion MakeBigExplosion(GameTime gameTime, Aether.Vector2 pt) =>
            new Explosion(16f, 16f, gameTime.TotalGameTime) { RootPosition = pt.ToXna() };

        private static Explosion MakeSmallExplosion(GameTime gameTime, Aether.Vector2 pt) =>
            new Explosion(1f, 1f, gameTime.TotalGameTime) { RootPosition = pt.ToXna() };

        private record struct CollisionContext(Aether.Vector2 cp, GameTime gameTime, Stack<Action> postCheckActions);

        private void ExecuteCollision(CollisionContext ctx, Airplane plane, Ground ground)
        {
            ctx.postCheckActions.Push(() => { plane.RemoveRigging(collisionWorld); });
            plane.Exploded = true;
            planeExplosion = MakeBigExplosion(ctx.gameTime, ctx.cp);
        }

        private void ExecuteExplosion(CollisionContext ctx, Bomb bomb, Ground ground)
        {
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bomb.Body); });
            Bombs.Remove(bomb);
            explosions.Add(MakeBigExplosion(ctx.gameTime, ctx.cp));
        }

        private void ExecuteExplosion(CollisionContext ctx, Bullet bullet, Ground ground)
        {
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bullet.Body); });
            Bullets.Remove(bullet);
            explosions.Add(MakeSmallExplosion(ctx.gameTime, ctx.cp));
        }

        private void ExecuteExplosion(CollisionContext ctx, Bomb bomb, Airplane plane)
        {
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bomb.Body); });
            Bombs.Remove(bomb);
            ctx.postCheckActions.Push(() => { Plane.RemoveRigging(collisionWorld); });
            Plane.Exploded = true;
            planeExplosion = MakeBigExplosion(ctx.gameTime, ctx.cp);
            explosions.Add(MakeBigExplosion(ctx.gameTime, ctx.cp));
        }

        private void ExecuteExplosion(CollisionContext ctx, Bomb bomb1, Bomb bomb2)
        {
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bomb1.Body); });
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bomb2.Body); });
            Bombs.Remove(bomb1);
            Bombs.Remove(bomb2);
            explosions.Add(MakeBigExplosion(ctx.gameTime, ctx.cp));
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
                        Bombs.Add(planeProjected.Bomb.SetupRigging(collisionWorld));

                    if (planeProjected.Bullet != null)
                        Bullets.Add(planeProjected.Bullet.SetupRigging(collisionWorld));

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

            foreach (var bullet in Bullets)
                bullet.PreSimulationPrepare(Unit.Value);

            // Simulate
            collisionWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            // Update plane state if not exploded
            planePostSimulation();

            // Update bomb states
            foreach (var bomb in Bombs)
                bomb.PostSimulationUpdate(Unit.Value);

            // Update bullet states
            foreach (var bullet in Bullets)
                bullet.PostSimulationUpdate(Unit.Value);

            // Get rid of expired bullets
            var expiredBullets = Bullets.Where(e => e.IsExpired(gameTime.TotalGameTime)).ToArray();
            foreach (var eb in expiredBullets)
                Bullets.Remove(eb);

            // Get rid of expired explosions
            var expiredExplosions = explosions.Where(e => e.IsExpired(gameTime.TotalGameTime)).ToArray();
            foreach (var ee in expiredExplosions)
                explosions.Remove(ee);

            // Collision check round.
            var postCheckActions = new Stack<Action>(); // Push here actions which manipulate collisionWorld, to avoid corruption mid-iteration.
            var makeCtx = (Aether.Vector2 cp) => new CollisionContext(cp, gameTime, postCheckActions);

            for (Contact ct = collisionWorld.ContactList.Next; ct != collisionWorld.ContactList; ct = ct.Next)
            {
                var bombAlive   = (Bomb bomb)      => Bombs.Exists(b => bomb == b);
                var bulletAlive = (Bullet bullet)  => Bullets.Exists(b => bullet == b);
                var groundAlive = (Ground ground)  => true;
                var planeAlive  = (Airplane plane) => !plane.Exploded;

                var _ = 
                    Physics.OnCollision(ct, "Plane-Ground",  planeAlive,  groundAlive, (cp, p, g)   => ExecuteCollision(makeCtx(cp), p,  g))  ||
                    Physics.OnCollision(ct, "Bomb-Ground",   bombAlive,   groundAlive, (cp, b, g)   => ExecuteExplosion(makeCtx(cp), b,  g))  ||
                    Physics.OnCollision(ct, "Bomb-Bomb",     bombAlive,   bombAlive,   (cp, b1, b2) => ExecuteExplosion(makeCtx(cp), b1, b2)) ||
                    Physics.OnCollision(ct, "Bomb-Plane",    bombAlive,   planeAlive,  (cp, b, p)   => ExecuteExplosion(makeCtx(cp), b,  p))  ||
                    Physics.OnCollision(ct, "Bullet-Ground", bulletAlive, groundAlive, (cp, b, g)   => ExecuteExplosion(makeCtx(cp), b,  g))  ||
                    false;
            }

            foreach (var postAction in postCheckActions) postAction();
        }

    }
}

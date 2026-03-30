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
        public static readonly Collider<string> WorldCollider = new Collider<string>();

        public readonly Ground Ground;
        public readonly Ceiling Ceiling;
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

        public readonly List<StaticBuilding> Buildings;
        public readonly List<Ground.Runway> Runways;
        public readonly List<Autopilot.Approach> Approaches = new List<Autopilot.Approach>();

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

            Ceiling = new Ceiling();
            (Ground, Buildings, Runways) = Ground.MakeWithBuildings();

            Ground.SetupRigging(collisionWorld);
            Ceiling.SetupRigging(collisionWorld);
            foreach (var building in Buildings)
                building.SetupRigging(collisionWorld);
            Plane = MakeNewPlane();

            Autopilot.Approach makeApproach(Ground.Runway rw, float farX, float rwEnd, float rwLevel, float df)
            {
                var preTouchZone = new Autopilot.ApproachZone(rwEnd+5f*df, rwEnd-15f*df, rwLevel+2.5f, rwLevel+7.5f, (-Vector2.UnitX*df).Rotate(MathHelper.ToRadians(+29f*df)), (-Vector2.UnitX*df).Rotate(MathHelper.ToRadians(+15f*df)));
                var finalZone = new Autopilot.ApproachZone(rwEnd+35f*df, rwEnd+25f*df, rwLevel+10f, rwLevel+20f, (-Vector2.UnitX*df).Rotate(MathHelper.ToRadians(+30f*df)), (-Vector2.UnitX*df).Rotate(MathHelper.ToRadians(-30f*df)));
                var preFinalZone = new Autopilot.ApproachZone(rwEnd+95f*df, farX, rwLevel+10f, rwLevel+20f, (Vector2.UnitX*df).Rotate(MathHelper.ToRadians(-45f*df)), (Vector2.UnitX*df).Rotate(MathHelper.ToRadians(+45f*df)));
                var approach = new Autopilot.Approach(rw, preFinalZone, finalZone, preTouchZone, 34f, 20f);

                return approach;
            }

            // For each runaway construct an approach
            foreach (var rw in Runways)
            {
                var eastApproach = makeApproach(rw, 550, rw.End, rw.Level, +1f);
                var westApproach = makeApproach(rw, 50, rw.Start, rw.Level, -1f);

                Approaches.Add(eastApproach);
                Approaches.Add(westApproach);
            }
        }

        private Airplane MakeNewPlane()
        {
            var runway = Runways[0];
            var spin = runway.End > runway.Start ? BasisSpin.Down : BasisSpin.Up;
            var parkingPos = runway.End > runway.Start ? runway.Start + runway.ParkingOffset : runway.Start - runway.ParkingOffset;
            var plane = new Airplane(new Vector2(parkingPos, runway.Level), spin);
            plane.CheckAndSetLandingMode(runway);

            plane.SetupRigging(collisionWorld);
            plane.Body.Position = plane.Position.ToAether();
            plane.Body.Rotation = plane.Direction.ToAngle();
            return plane;
        }

        private static Explosion MakeBasedExplosion(GameTime gameTime, Aether.Vector2 pt) =>
            new Explosion(Explosion.ExplosionVariant.Based1, 16f, 16f, gameTime.TotalGameTime, TimeSpan.FromSeconds(2)) { RootPosition = pt.ToXna() };

        private static Explosion MakeBigExplosion(GameTime gameTime, Aether.Vector2 pt) =>
            new Explosion(Explosion.ExplosionVariant.Centered1, 10f, 10f, gameTime.TotalGameTime, TimeSpan.FromSeconds(1.5)) { RootPosition = pt.ToXna() };

        private static Explosion MakeSmallExplosion(GameTime gameTime, Aether.Vector2 pt) =>
            new Explosion(Explosion.ExplosionVariant.Centered1, 1f, 1f, gameTime.TotalGameTime, TimeSpan.FromSeconds(1)) { RootPosition = pt.ToXna() };

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

        private void ExecuteExplosion(CollisionContext ctx, Bomb bomb, StaticBuilding building)
        {
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bomb.Body); });
            ctx.postCheckActions.Push(() => { building.RemoveRigging(collisionWorld); });
            building.Exploded = true;
            Bombs.Remove(bomb);
            explosions.Add(MakeBasedExplosion(ctx.gameTime, building.Position.ToAether()));
        }

        private void ExecuteCollision(CollisionContext ctx, Airplane plane, StaticBuilding building)
        {
            ctx.postCheckActions.Push(() => { plane.RemoveRigging(collisionWorld); });
            plane.Exploded = true;
            planeExplosion = MakeBigExplosion(ctx.gameTime, ctx.cp);
            ctx.postCheckActions.Push(() => { building.RemoveRigging(collisionWorld); });
            building.Exploded = true;
            explosions.Add(MakeBasedExplosion(ctx.gameTime, building.Position.ToAether()));
        }

        private void ExecuteExplosion(CollisionContext ctx, Bullet bullet, StaticBuilding building)
        {
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bullet.Body); });
            Bullets.Remove(bullet);
            explosions.Add(MakeSmallExplosion(ctx.gameTime, ctx.cp));

            building.Hits += 1;
            if (building.Hits < 5) return;

            ctx.postCheckActions.Push(() => { building.RemoveRigging(collisionWorld); });
            building.Exploded = true;
            explosions.Add(MakeBasedExplosion(ctx.gameTime, building.Position.ToAether()));
        }

        private void ExecuteBounce(CollisionContext ctx, Airplane plane, Ceiling ceiling)
        {
            var newY = ctx.cp.Y - plane.Length + 0.9f;
            // Position plane facing direct down and with minimal speed.
            plane.PostSimulationUpdate(plane.CurrentState with 
            { 
                Direction = -Vector2.UnitY, 
                Speed = Airplane.MinSpeed, 
                Position = new Vector2(ctx.cp.X, newY) ,
            });
        }


        public void Simulate(GameTime gameTime, float ups)
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
                    if (Plane.CurrentState.AutoLanding != null)
                    {
                        // Compute new ApproachPhase & set new inputs.
                        var (phase, input) = Plane.Transition(Plane.CurrentState.AutoLanding, ups);
                        if (phase == Autopilot.ApproachPhase.Fail)
                        {
                            Plane.CurrentState = Plane.CurrentState with { AutoLanding = null };
                        }
                        else
                        {
                            Plane.Input = input;
                            Plane.CurrentState = Plane.CurrentState with { AutoLanding = phase };
                        }
                    }

                    Plane.CheckAndSetLandingMode(Runways[0]);
                    var planeProjected = Plane.ApplyInputs(Plane.Input, () => Autopilot.InitiateAutoLanding(Plane, Approaches), gameTime);

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
                var bombAlive     = (Bomb bomb)      => Bombs.Exists(b => bomb == b);
                var bulletAlive   = (Bullet bullet)  => Bullets.Exists(b => bullet == b);
                var groundAlive   = (Ground ground)  => true;
                var planeAlive    = (Airplane plane) => !plane.Exploded;
                var buildingAlive = (StaticBuilding building) => !building.Exploded;
                var ceilingAlive  = (Ceiling ceiling) => true;


                var _ = 
                    Physics.OnCollision(ct, "Plane-Ground",    planeAlive,  groundAlive,   (cp, p, g)   => ExecuteCollision(makeCtx(cp), p,  g))  ||
                    Physics.OnCollision(ct, "Bomb-Ground",     bombAlive,   groundAlive,   (cp, b, g)   => ExecuteExplosion(makeCtx(cp), b,  g))  ||
                    Physics.OnCollision(ct, "Bomb-Bomb",       bombAlive,   bombAlive,     (cp, b1, b2) => ExecuteExplosion(makeCtx(cp), b1, b2)) ||
                    Physics.OnCollision(ct, "Bomb-Plane",      bombAlive,   planeAlive,    (cp, b, p)   => ExecuteExplosion(makeCtx(cp), b,  p))  ||
                    Physics.OnCollision(ct, "Bullet-Ground",   bulletAlive, groundAlive,   (cp, b, g)   => ExecuteExplosion(makeCtx(cp), b,  g))  ||
                    Physics.OnCollision(ct, "Bomb-Building",   bombAlive,   buildingAlive, (cp, b, bb)  => ExecuteExplosion(makeCtx(cp), b,  bb)) ||
                    Physics.OnCollision(ct, "Plane-Building",  planeAlive,  buildingAlive, (cp, p, b)   => ExecuteCollision(makeCtx(cp), p,  b)) ||
                    Physics.OnCollision(ct, "Bullet-Building", bulletAlive, buildingAlive, (cp, b, bb)  => ExecuteExplosion(makeCtx(cp), b,  bb)) ||
                    Physics.OnCollision(ct, "Plane-Ceiling",   planeAlive,  ceilingAlive,  (cp, p, c)   => ExecuteBounce   (makeCtx(cp), p,  c)) ||
                    false;
            }

            foreach (var postAction in postCheckActions) postAction();
        }

    }
}

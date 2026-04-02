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

        public readonly List<FlakGun> FlakGuns;
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
            (Ground, Buildings, FlakGuns, Runways) = Ground.MakeWithBuildings();

            Ground.SetupRigging(collisionWorld, () => new object[] { Ground, MayDieByBomb(Ground) });
            Ceiling.SetupRigging(collisionWorld);

            foreach (var building in Buildings)
                building.SetupRigging(collisionWorld, () => MayDieByBomb(building));

            foreach (var flakGun in FlakGuns)
                flakGun.SetupRigging(collisionWorld, () => MayDieByBomb(flakGun));

            Plane = MakeNewPlane();

            Autopilot.Approach makeApproach(Ground.Runway rw, float farX, float rwEnd, float rwLevel, float df)
            {
                var preTouchZone = new Autopilot.ApproachZone(rwEnd + 5f * df, rwEnd - 15f * df, rwLevel + 2.5f, rwLevel + 7.5f, (-Vector2.UnitX * df).Rotate(MathHelper.ToRadians(+29f * df)), (-Vector2.UnitX * df).Rotate(MathHelper.ToRadians(+15f * df)));
                var finalZone = new Autopilot.ApproachZone(rwEnd + 35f * df, rwEnd + 25f * df, rwLevel + 10f, rwLevel + 20f, (-Vector2.UnitX * df).Rotate(MathHelper.ToRadians(+30f * df)), (-Vector2.UnitX * df).Rotate(MathHelper.ToRadians(-30f * df)));
                var preFinalZone = new Autopilot.ApproachZone(rwEnd + 95f * df, farX, rwLevel + 10f, rwLevel + 20f, (Vector2.UnitX * df).Rotate(MathHelper.ToRadians(-45f * df)), (Vector2.UnitX * df).Rotate(MathHelper.ToRadians(+45f * df)));
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

            plane.SetupRigging(collisionWorld, () => new object[] { plane, MayDieByBomb(plane) });
            plane.Body.Position = plane.Position.ToAether();
            plane.Body.Rotation = plane.Direction.ToAngle();
            return plane;
        }

        private ICanDieByBomb<Unit> MayDieByBomb(StaticBuilding building) =>
            new CanDieByBomb<Unit>(
                "Building",
                () => building.Exploded,
                () => building.Exploded = true,
                (_) => building.RemoveRigging(collisionWorld),
                (gameTime, _) => { explosions.Add(MakeBasedExplosion(gameTime, building.Position.ToAether())); return Unit.Value; });

        private ICanDieByBomb<Unit> MayDieByBomb(FlakGun flakGun) =>
            new CanDieByBomb<Unit>(
                "FlakGun",
                () => flakGun.Exploded,
                () => flakGun.Exploded = true,
                (_) => flakGun.RemoveRigging(collisionWorld),
                (gameTime, _) => { explosions.Add(MakeBasedExplosion(gameTime, flakGun.Position.ToAether())); return Unit.Value; });

        private ICanDieByBomb<Unit> MayDieByBomb(Bomb bomb) =>
            new CanDieByBomb<Unit>(
                "Bomb",
                () => !Bombs.Exists(b => bomb == b),
                () => Bombs.Remove(bomb),
                (_) => collisionWorld.Remove(bomb.Body),
                (gameTime, cp) => { explosions.Add(MakeBigExplosion(gameTime, cp.ToAether())); return Unit.Value; });

        private ICanDieByBomb<Unit> MayDieByBomb(Airplane plane) =>
            new CanDieByBomb<Unit>(
                "Plane",
                () => plane.Exploded,
                () => plane.Exploded = true,
                (_) => plane.RemoveRigging(collisionWorld),
                (gameTime, cp) =>
                {
                    planeExplosion = MakeBigExplosion(gameTime, cp.ToAether());
                    explosions.Add(MakeBigExplosion(gameTime, cp.ToAether()));
                    return Unit.Value;
                });

        private ICanDieByBomb<Ground.XRange> MayDieByBomb(Ground ground) =>
            new CanDieByBomb<Ground.XRange>(
                "Ground",
                () => false,
                () => { },
                (xr) => ground.ReRigRange(xr),
                (gameTime, cp) => 
                {
                    explosions.Add(MakeBigExplosion(gameTime, cp.ToAether()));
                    return ground.PlaceDent(cp.X, 1.0f, 0.5f, segmentsPerMeter: 8);
                });


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
            var affectedRange = ground.PlaceDent(ctx.cp.X, 1.5f, 0.8f, segmentsPerMeter: 8);
            ctx.postCheckActions.Push(() => { ground.ReRigRange(affectedRange); });
        }

        private void ExecuteExplosion(CollisionContext ctx, Bullet bullet, Ground ground)
        {
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bullet.Body); });
            Bullets.Remove(bullet);
            explosions.Add(MakeSmallExplosion(ctx.gameTime, ctx.cp));
        }

        private void ExecuteExplosion<T>(CollisionContext ctx, Bomb bomb, ICanDieByBomb<T> dead)
        {
            ctx.postCheckActions.Push(() => { collisionWorld.Remove(bomb.Body); });
            Bombs.Remove(bomb);
            dead.SetExploded();
            var payload = dead.MakeExplosion(ctx.gameTime, ctx.cp.ToXna());
            ctx.postCheckActions.Push(() => { dead.RemoveRigging(payload); });
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
                        Bombs.Add(planeProjected.Bomb.SetupRigging(collisionWorld, () => new object[] { planeProjected.Bomb, MayDieByBomb(planeProjected.Bomb) }));

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
                var bombAlive     = (Bomb bomb)       => Bombs.Exists(b => bomb == b);
                var bulletAlive   = (Bullet bullet)   => Bullets.Exists(b => bullet == b);
                var groundAlive   = (Ground ground)   => true;
                var planeAlive    = (Airplane plane)  => !plane.Exploded;
                var buildingAlive = (StaticBuilding building) => !building.Exploded;
                var ceilingAlive  = (Ceiling ceiling) => true;
                var byBombAlive1  = (ICanDieByBomb<Unit> byBomb) => !byBomb.IsExploded();
                var byBombAlive2  = (ICanDieByBomb<Ground.XRange> byBomb) => !byBomb.IsExploded();

                var _ = 
                    Physics.OnCollision(ct, "Plane-Ground",    planeAlive,  groundAlive,    (cp, p, g)   => ExecuteCollision(makeCtx(cp), p,  g))  ||
                    Physics.OnCollision(ct, "Bullet-Ground",   bulletAlive, groundAlive,    (cp, b, g)   => ExecuteExplosion(makeCtx(cp), b,  g))  ||
                    Physics.OnCollision(ct, "Plane-Building",  planeAlive,  buildingAlive,  (cp, p, b)   => ExecuteCollision(makeCtx(cp), p,  b))  ||
                    Physics.OnCollision(ct, "Bullet-Building", bulletAlive, buildingAlive,  (cp, b, bb)  => ExecuteExplosion(makeCtx(cp), b,  bb)) ||
                    Physics.OnCollision(ct, "Plane-Ceiling",   planeAlive,  ceilingAlive,   (cp, p, c)   => ExecuteBounce   (makeCtx(cp), p,  c))  ||
                    Physics.OnCollision(ct, "Bomb-{0}",        bombAlive,   byBombAlive1,   (cp, b, bb)  => ExecuteExplosion(makeCtx(cp), b,  bb)) ||
                    Physics.OnCollision(ct, "Bomb-{0}",        bombAlive,   byBombAlive2,   (cp, b, bb)  => ExecuteExplosion(makeCtx(cp), b,  bb)) ||
                    false;
            }

            foreach (var postAction in postCheckActions) postAction();
        }

    }
}

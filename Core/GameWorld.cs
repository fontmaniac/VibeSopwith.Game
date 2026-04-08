using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;
using System;
using VibeSopwith.Game.Utils;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class GameWorld
    {
        public const int WorldLength = 600;
        public const int WorldHeight = 60;

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

        public readonly List<Baloon> Baloons;
        public readonly List<FlakGun> FlakGuns;
        public readonly List<StaticBuilding> Buildings;
        public readonly List<Ground.Runway> Runways;
        public readonly List<Autopilot.Approach> Approaches = new List<Autopilot.Approach>();

        private readonly World collisionWorld;

        public GameWorld()
        {
            // Poppets are unshakeable pillars of the world.
            RaisePoppets();

            collisionWorld = new World()
            {
                Gravity = (-Vector2.UnitY * 10f).ToAether(),
            };

            //Ground = Ground.MakeFlat(0.1f); 
            //Ground = Ground.MakeRandom();
            //Ground = Ground.MakeCustom();

            Ceiling = new Ceiling();
            (Ground, Buildings, FlakGuns, Runways) = Ground.MakeWithBuildings();

            Baloons = new List<Baloon>();
            Baloons.Add(new Baloon(new Vector2(380, 35), BasisSpin.Down, new Vector2(385, 35)));
            Baloons.Add(new Baloon(new Vector2(555, 25), BasisSpin.Up, new Vector2(558, 25)));


            Ground.SetupRigging(collisionWorld, () => new object[] { Ground, MayDieByBomb(Ground), MayDieByBullet(Ground), MayDieByPlane(Ground) });
            Ceiling.SetupRigging(collisionWorld);

            foreach (var baloon in Baloons)
                baloon.SetupRigging(collisionWorld, () => new object[] { baloon, MayDieByProjectile(baloon, "Baloon", pBaloon, 10) with { ExecuteEffect = Caps.ExecuteEffect((gameTime, _) => explosions.Add(MakeBigExplosion(gameTime, baloon.Position.ToAether())))}});

            foreach (var building in Buildings)
                building.SetupRigging(collisionWorld, () => new object[] { building, MayDieByProjectile(building, "Building", pBuilding, 5) });

            foreach (var flakGun in FlakGuns)
                flakGun.SetupRigging(collisionWorld, () => new object[] { flakGun, MayDieByProjectile(flakGun, "FlakGun", pFlakGun, 5) });

            Plane = MakeNewPlane();

            Autopilot.Approach makeApproach(Cardinal direction, Ground.Runway rw, float farX, float rwEnd, float rwLevel, float df)
            {
                var preTouchZone = new Autopilot.ApproachZone(rwEnd + 5f * df, direction, 20f, rwLevel + 2.5f, rwLevel + 7.5f, new Autopilot.Funnel((-Vector2.UnitX * df).RotateDeg(+22f * df), +7f * df, -7f * df));
                var finalZone0 = new Autopilot.ApproachZone(rwEnd + 35f * df, direction, 10f, rwLevel + 10f, rwLevel + 20f, new Autopilot.Funnel((-Vector2.UnitX * df), -30f, +30f));
                var preFinalZone0 = new Autopilot.ApproachZone(rwEnd + 95f * df, direction.Toggle(), MathF.Abs(rwEnd + 95f * df - farX), rwLevel + 10f, rwLevel + 20f, new Autopilot.Funnel((Vector2.UnitX * df), -45f, +45f));
                var approach = new Autopilot.Approach(direction, rw, Autopilot.Approach.Node.Of(preFinalZone0), Autopilot.Approach.Node.Of(finalZone0), preTouchZone, 34f, 20f);

                return approach;
            }

            Autopilot.Approach makeLeftApproach(Cardinal direction, Ground.Runway rw, float farX, float rwEnd, float rwLevel)
            {
                var preTouchZone = new Autopilot.ApproachZone(rwEnd + 5f, direction, 20f, rwLevel + 2.5f, rwLevel + 7.5f, new Autopilot.Funnel((-Vector2.UnitX).RotateDeg(+22f), +7f, -7f));

                var finalZone0 = new Autopilot.ApproachZone(rwEnd + 35f, direction, 10f, rwLevel + 10f, rwLevel + 20f, new Autopilot.Funnel((-Vector2.UnitX), -30f, +30f));
                var finalZone1 = new Autopilot.ApproachZone(rwEnd + 95f, direction, 10f, rwLevel + 08f, rwLevel + 22f, new Autopilot.Funnel((-Vector2.UnitX), -45f, +45f));
                var finalZone2 = new Autopilot.ApproachZone(rwEnd + 155f, direction, 10f, rwLevel + 08f, rwLevel + 22f, new Autopilot.Funnel((-Vector2.UnitX), -45f, +45f));
                var finalZone3 = new Autopilot.ApproachZone(rwEnd + 215f, direction, 10f, rwLevel + 12f, rwLevel + 18f, new Autopilot.Funnel((-Vector2.UnitX), -45f, +45f));

                var preFinalZone0 = new Autopilot.ApproachZone(rwEnd + 95f, direction.Toggle(), 10f, rwLevel + 10f, rwLevel + 20f, new Autopilot.Funnel((Vector2.UnitX), -45f, +45f));
                var preFinalZone1 = new Autopilot.ApproachZone(rwEnd + 155f, direction.Toggle(), 10f, rwLevel + 10f, rwLevel + 20f, new Autopilot.Funnel((Vector2.UnitX), -75f, +75f));
                var preFinalZone2 = new Autopilot.ApproachZone(rwEnd + 245f, direction.Toggle(), 10f, rwLevel + 10f, rwLevel + 20f, new Autopilot.Funnel((Vector2.UnitX), -75f, +75f));

                var approach = new Autopilot.Approach(
                    direction, rw,
                    Autopilot.Approach.Node.Of(preFinalZone0).WithNext(preFinalZone1).WithNext(preFinalZone2),
                    Autopilot.Approach.Node.Of(finalZone3).WithNext(finalZone2).WithNext(finalZone1).WithNext(finalZone0),
                    preTouchZone, 34f, 20f);

                return approach;
            }

            // For each runaway construct an approach
            foreach (var rw in Runways)
            {
                var eastApproach = makeLeftApproach(Cardinal.Left,  rw, 550, rw.End, rw.Level);
                var westApproach = makeApproach(Cardinal.Right, rw, 50, rw.Start, rw.Level, -1f);

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

            plane.SetupRigging(collisionWorld, () => new object[] { plane, MayDieByBomb(plane), MayDieByBullet(plane) });
            plane.Body.Position = plane.Position.ToAether();
            plane.Body.Rotation = plane.Direction.ToAngle();
            return plane;
        }

        #region Object Capabilities

        private Poppet<Ground> pGround = null!;
        private Poppet<Bomb> pBomb = null!;
        private Poppet<Bullet> pBullet = null!;
        private Poppet<Airplane> pPlane = null!;
        private Poppet<StaticBuilding> pBuilding = null!;
        private Poppet<FlakGun> pFlakGun = null!;
        private Poppet<Baloon> pBaloon = null!;
        private Poppet<Ceiling> pCeiling = null!;

        private ICanDie<Unit> howPlaneDies(Airplane plane) => 
            Caps.JustDie(pPlane,  plane,  collisionWorld, Caps.ExecuteEffect((gt, cp) => planeExplosion = MakeBigExplosion(gt, plane.MidPoint.ToAether())));
        private ICanDie<Unit> howBombDies(Bomb bomb) => 
            Caps.JustDie(pBomb,   bomb,   collisionWorld, Caps.NoEffect());
        private ICanDie<Unit> howBulletDies(Bullet bullet, IBasis bindTarget) => 
            Caps.JustDie(pBullet, bullet, collisionWorld, Caps.ExecuteEffect((gt, cp) => explosions.Add(MakeSmallExplosion(gt, LiveBasis.Bind(Basis.FixedPos(cp), bindTarget)))));

        private void RaisePoppets()
        {
            Poppet.Make(out pGround);
            Poppet.Make(out pCeiling);
            Poppet.Make(out pBomb,      (bomb)     => Bombs.Exists(b => bomb == b),     bomb     => Bombs.Remove(bomb));
            Poppet.Make(out pBullet,    (bullet)   => Bullets.Exists(b => bullet == b), bullet   => Bullets.Remove(bullet));
            Poppet.Make(out pPlane,     (plane)    => !plane.Exploded,                  plane    => plane.Exploded = true);
            Poppet.Make(out pBuilding,  (building) => !building.Exploded,               building => building.Exploded = true);
            Poppet.Make(out pFlakGun,   (flakGun)  => !flakGun.Exploded,                flakGun  => flakGun.Exploded = true);
            Poppet.Make(out pBaloon,    (baloon)   => !baloon.Exploded,                 baloon   => baloon.Exploded = true);
        }

        private CanDieByProjectile<Unit> MayDieByProjectile<T>(T target, string name, Poppet<T> poppet, int hits) where T : IHasLocation, ICanRemoveRigging =>
            new CanDieByProjectile<Unit>(
                name,
                poppet.Embrace(target),
                Caps.AbsorbHits(hits),
                Caps.BindTargetPart(target),
                Caps.RemoveRigging(target, collisionWorld),
                Caps.ExecuteEffect((gameTime, _) => explosions.Add(MakeBasedExplosion(gameTime, target.Position.ToAether()))));

        private CanDieByBombOrBullet<Unit> MayDieByBombOrBullet(Bomb bomb) =>
            new CanDieByBombOrBullet<Unit>(
                "Bomb",
                pBomb.Embrace(bomb),
                Caps.AbsorbHits(1),
                Caps.BindTargetSelf(bomb),
                Caps.RemoveRigging(bomb, collisionWorld),
                Caps.ExecuteEffect((gameTime, cp) => explosions.Add(MakeBigExplosion(gameTime, cp.ToAether()))));

        private ICanDieByBomb<Unit> MayDieByBomb(Airplane plane) =>
            new CanDieByBomb<Unit>(
                "Plane",
                pPlane.Embrace(plane),
                Caps.RemoveRigging(plane, collisionWorld),
                Caps.ExecuteEffect((gameTime, cp) =>
                {
                    planeExplosion = MakeBigExplosion(gameTime, plane.MidPoint.ToAether());
                    explosions.Add(MakeBigExplosion(gameTime, cp.ToAether()));
                }));

        private ICanDieByBullet<Unit> MayDieByBullet(Airplane plane) =>
            new CanDieByBullet<Unit>(
                "Plane",
                pPlane.Embrace(plane),
                Caps.AbsorbHits(5),
                Caps.BindTargetSelf(plane),
                Caps.RemoveRigging(plane, collisionWorld),
                Caps.ExecuteEffect((gameTime, cp) => { planeExplosion = MakeBigExplosion(gameTime, plane.MidPoint.ToAether()); }));

        private ICanDieByBomb<Ground.XRange> MayDieByBomb(Ground ground) =>
            new CanDieByBomb<Ground.XRange>(
                "Ground",
                pGround.Embrace(ground),
                (xr) => ground.ReRigRange(xr),
                (gameTime, cp) => 
                {
                    explosions.Add(MakeBigExplosion(gameTime, cp.ToAether()));
                    return ground.PlaceDent(cp.X, 1.0f, 0.5f, segmentsPerMeter: 8);
                });

        private ICanDieByPlane<Ground.XRange> MayDieByPlane(Ground ground) =>
            new CanDieByPlane<Ground.XRange>(
                "Ground",
                pGround.Embrace(ground),
                (xr) => ground.ReRigRange(xr),
                (gameTime, cp) => ground.PlaceDent(cp.X, 1.5f, 0.8f, segmentsPerMeter: 8));

        private ICanDieByBullet<Unit> MayDieByBullet(Ground ground) =>
            new CanDieByBullet<Unit>(
                "Ground",
                pGround.Embrace(ground),
                Caps.ImperviousToHits(),
                Caps.BindToWorld(),
                Caps.DoNothing(),
                Caps.NoEffect());

        #endregion

        private static Explosion MakeBasedExplosion(GameTime gameTime, Aether.Vector2 pt) =>
            new Explosion(Explosion.ExplosionVariant.Based1, 16f, 16f, gameTime.TotalGameTime, TimeSpan.FromSeconds(2), Basis.FixedPos(pt.ToXna()));

        private static Explosion MakeBigExplosion(GameTime gameTime, Aether.Vector2 pt) =>
            new Explosion(Explosion.ExplosionVariant.Centered1, 10f, 10f, gameTime.TotalGameTime, TimeSpan.FromSeconds(1.5), Basis.FixedPos(pt.ToXna()));

        private static Explosion MakeSmallExplosion(GameTime gameTime, IBasis boundBasis) =>
            new Explosion(Explosion.ExplosionVariant.Centered1, 1f, 1f, gameTime.TotalGameTime, TimeSpan.FromSeconds(1), boundBasis);

        private record struct CollisionContext(Aether.Vector2 cp, Fixture fixA, Fixture fixB, GameTime gameTime, Stack<Action> postCheckActions);

        private void ExecuteBounce(CollisionContext ctx, Airplane plane, Ceiling ceiling)
        {
            var newY = ctx.cp.Y - plane.Length + 0.9f;
            // Position plane facing direct down and with minimal speed.
            plane.PostSimulationUpdate(plane.CurrentState with
            {
                Direction = -Vector2.UnitY,
                Speed = Airplane.MinSpeed,
                Position = new Vector2(ctx.cp.X, newY),
            });
        }

        private void Kill<T>(CollisionContext ctx, ICanDie<T> target)
        {
            target.Poppet.Kill();
            var payload = target.ExecuteEffect(ctx.gameTime, ctx.cp.ToXna());
            ctx.postCheckActions.Push(() => { target.RefreshRigging(payload); });
        }

        private void ExecuteCollision<T>(CollisionContext ctx, Airplane plane, ICanDieByPlane<T> target)
        {
            Kill(ctx, howPlaneDies(plane));
            Kill(ctx, target);
        }

        private void ExecuteExplosion<T>(CollisionContext ctx, Bomb bomb, ICanDieByBomb<T> target)
        {
            Kill(ctx, howBombDies(bomb));
            Kill(ctx, target);
        }

        private void ExecuteExplosion<T>(CollisionContext ctx, Bullet bullet, ICanDieByBullet<T> target)
        {

            Kill(ctx, howBulletDies(bullet, target.ExplosionBindTarget(ctx.fixB)));
            if (!target.HitOnce()) return;
            Kill(ctx, target);
        }

        private record struct SimulationContext(GameTime gameTime, float ups, Airplane.InputStack airplaneInputs);

        private interface IActor
        {
            Action DoBeforeSimulation(SimulationContext ctx);
        }

        private interface IPerishable
        {
            bool RemoveIfExpired(SimulationContext ctx);
        }

        private record struct Actor(Func<SimulationContext, Action> doBeforeSimulation) : IActor
        {
            public Action DoBeforeSimulation(SimulationContext ctx) => doBeforeSimulation(ctx);
        }

        private record struct Perishable(Func<SimulationContext, bool> removeIfExpired) : IPerishable
        {
            public bool RemoveIfExpired(SimulationContext ctx) => removeIfExpired(ctx);
        }

        private IPerishable GetPerishable(Func<SimulationContext, bool> removeIfExpired) => new Perishable(removeIfExpired);

        private IEnumerable<IActor> EnumerateActors()
        {
            yield return GetPlaneActor();
            foreach (var bomb in Bombs) yield return GetUnitActor(bomb);
            foreach (var bullet in Bullets) yield return GetUnitActor(bullet);  
            foreach (var flakGun in FlakGuns) yield return GetFlakActor(flakGun);
            foreach (var baloon in Baloons) yield return GetBaloonActor(baloon);
        }

        private IEnumerable<IPerishable> EnumeratePerishables()
        {
            foreach (var bullet in Bullets) yield return GetPerishable((ctx) => { if (bullet.IsExpired(ctx.gameTime.TotalGameTime)) return Bullets.Remove(bullet); return false; });
            foreach (var explosion in explosions) yield return GetPerishable((ctx) => { if (explosion.IsExpired(ctx.gameTime.TotalGameTime)) return explosions.Remove(explosion); return false; });
            foreach (var baloon in Baloons) yield return GetPerishable((ctx) => { if (baloon.Exploded) return Baloons.Remove(baloon); return false; });
        }

        private IActor GetUnitActor(IAmBehaving<Unit> actor) => new Actor((ctx) => 
        {
            actor.PreSimulationPrepare(Unit.Value);
            return () => actor.PostSimulationUpdate(Unit.Value);
        });

        private IActor GetActor<T>(T actor, Func<SimulationContext, Action> doBeforeSimulation) => new Actor(doBeforeSimulation);

        private IActor GetFlakActor(FlakGun flakGun) => GetActor(flakGun, (ctx) => 
        {
            var projected = flakGun.ApplyInputs(ctx.gameTime);

            if (projected.Bullet != null)
                Bullets.Add(projected.Bullet.SetupRigging(collisionWorld));

            if (projected.MuzzleFlash != null)
                explosions.Add(projected.MuzzleFlash);

            flakGun.PreSimulationPrepare(projected);

            return () => flakGun.PostSimulationUpdate(projected);
        });

        private IActor GetBaloonActor(Baloon baloon) => GetActor(baloon, (ctx) => 
        {
            var projected = baloon.ApplyInputs(ctx.gameTime);
            baloon.PreSimulationPrepare(projected);

            return () => baloon.PostSimulationUpdate(projected);
        });

        private IActor GetPlaneActor() => GetActor(Plane, (ctx) => 
        {
            var airplaneInputs = ctx.airplaneInputs;

            // 1. Execute "Plane.WhenDestroyed" behavior
            if (Plane.Exploded)
            {
                if (planeExplosion?.IsExpired(ctx.gameTime.TotalGameTime) == true)
                {
                    planeExplosion = null;
                    Plane = MakeNewPlane();
                }

                return () => { };
            }
            else
            {
                // Otherwise,
                // 2. execute "Plane.WhenAutoLanding" behavior
                if (Plane.CurrentState.AutoLanding != null)
                {
                    // Compute new ApproachPhase & set new inputs.
                    var (phase, input) = Autopilot.Transition(Plane, Plane.CurrentState.AutoLanding, ctx.ups);
                    if (phase == Autopilot.ApproachPhase.Fail)
                    {
                        Plane.CurrentState = Plane.CurrentState with { AutoLanding = null };
                    }
                    else
                    {
                        // Store Synthesized inputs back in the Plane instance, potentially overriding User inputs stored at previous Update.
                        airplaneInputs.Autopilot = input;
                        Plane.CurrentState = Plane.CurrentState with { AutoLanding = phase };
                    }
                }

                // 3. Execute "Plane.Ordinary" behavior
                Plane.CheckAndSetLandingMode(Runways[0]);

                // 4. Compute "Plane.Projected" state by calling Plane.ApplyInputs, passing in:
                // - AutoLanding init "factory" - in case inputs indicate the need to initiate.
                var planeProjected = Plane.ApplyInputs(airplaneInputs, () => Autopilot.InitiateAutoLanding(Plane, Approaches), ctx.gameTime);

                // 5. Modify necessary bits of Aether instrumentation before simulation step. 
                Plane.PreSimulationPrepare(planeProjected);

                // 6. Prepare a closure to execute after physics simulation step.
                return () =>
                {
                    // 7. Project simulated physicals (positions, velocities, etc.) back to Plane "world" instance.
                    Plane.PostSimulationUpdate(planeProjected);

                    // Ideally #8 must be here
                    // 8. Act upon finalized Plane state.
                    {
                        var planeState = Plane.CurrentState;
                        if (planeState.Bomb != null)
                            Bombs.Add(planeState.Bomb.SetupRigging(collisionWorld, () => new object[] { planeState.Bomb, MayDieByBombOrBullet(planeState.Bomb) }));

                        if (planeState.Bullet != null)
                            Bullets.Add(planeState.Bullet.SetupRigging(collisionWorld));
                    }
                };
            }
        });


        public void Simulate(GameTime gameTime, float ups, Airplane.InputStack airplaneInputs)
        {
            var ctx = new SimulationContext(gameTime, ups, airplaneInputs);

            // Execute pre-physics-simulation behaviors.
            var postSimulationActions = 
                EnumerateActors()
                .Select(actor => actor.DoBeforeSimulation(ctx))
                .ToArray();

            //------------------------------------------------------------------//
            // Simulate Physics.                                                //
            collisionWorld.Step((float)gameTime.ElapsedGameTime.TotalSeconds);  //
            //                                                                  //
            //------------------------------------------------------------------//

            // Execute post-physics-simulation behaviors.
            foreach (var postSimAction in postSimulationActions)
                postSimAction();

            // "Expire" behaviors
            foreach (var perishable in EnumeratePerishables().ToArray())
                perishable.RemoveIfExpired(ctx);

            // Collision check round - not a "behavior", a structural step.
            var postCheckActions = new Stack<Action>(); // Push here actions which manipulate collisionWorld, to avoid corruption mid-iteration.
            var makeCtx = (Aether.Vector2 cp, Fixture a, Fixture b) => new CollisionContext(cp, a, b, gameTime, postCheckActions);

            for (Contact ct = collisionWorld.ContactList.Next; ct != collisionWorld.ContactList; ct = ct.Next)
            {
                var _ = 
                    Physics.OnCollision(ct, "Plane-Ceiling", pPlane.IsAlive,  pCeiling.IsAlive,                                 (cp, fa, fb, p, c) => ExecuteBounce   (makeCtx(cp, fa, fb), p,  c)) ||
                    Physics.OnCollision(ct, "Plane-{0}",     pPlane.IsAlive,  Caps.CheckAlive<ICanDieByPlane<Unit>>(),          (cp, fa, fb, p, t) => ExecuteCollision(makeCtx(cp, fa, fb), p,  t)) ||
                    Physics.OnCollision(ct, "Plane-{0}",     pPlane.IsAlive,  Caps.CheckAlive<ICanDieByPlane<Ground.XRange>>(), (cp, fa, fb, p, t) => ExecuteCollision(makeCtx(cp, fa, fb), p,  t)) ||
                    Physics.OnCollision(ct, "Bullet-{0}",    pBullet.IsAlive, Caps.CheckAlive<ICanDieByBullet<Unit>>(),         (cp, fa, fb, b, t) => ExecuteExplosion(makeCtx(cp, fa, fb), b,  t)) ||
                    Physics.OnCollision(ct, "Bomb-{0}",      pBomb.IsAlive,   Caps.CheckAlive<ICanDieByBomb<Unit>>(),           (cp, fa, fb, b, t) => ExecuteExplosion(makeCtx(cp, fa, fb), b,  t)) ||
                    Physics.OnCollision(ct, "Bomb-{0}",      pBomb.IsAlive,   Caps.CheckAlive<ICanDieByBomb<Ground.XRange>>(),  (cp, fa, fb, b, t) => ExecuteExplosion(makeCtx(cp, fa, fb), b,  t)) ||
                    false;
            }

            foreach (var postAction in postCheckActions) postAction();
        }

    }
}

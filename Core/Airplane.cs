using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class Airplane : IHasLocation, IAmBehaving<Airplane.State>, ICanRemoveRigging
    {
        public Body Body = null!;

        public abstract record EigenState()
        {
            public sealed record ControlledFlight() : EigenState;
            public sealed record AutoLanding(Autopilot.ApproachPhase Phase) : EigenState;
            public sealed record Destroyed(Explosion Explosion) : EigenState;
        }

        public record State(Basis Location, float Speed, Bomb? Bomb, Bullet? Bullet, DateTime BombTime, DateTime BulletTime, DateTime RollTime, EigenState EigenState);

        public State CurrentState;
        public float Speed { get => CurrentState.Speed; }

        public Vector2 Position { get => CurrentState.Location.Position; }       // World position of the plane in meters. 
        public Vector2 Direction { get => CurrentState.Location.Direction; }     // Direction where plane is facing.
        public BasisSpin Spin { get => CurrentState.Location.Spin; }             // Orientation of basis spin. Up is Y-up.
        public float Length => 3.46f;
        public float Height => 2.0f;

        public Vector2 MidPoint { get => Position + midPointOffset.Rotate(Direction.ToAngle()); }

        public bool IsExploded { get => CurrentState.EigenState is EigenState.Destroyed; } 
        public Explosion SetDestroyed(GameTime gt) 
        { 
            var explosion = new Explosion(Explosion.ExplosionVariant.Centered1, 10f, 10f, gt.TotalGameTime, TimeSpan.FromSeconds(1.5), Basis.FixedPos(MidPoint));
            CurrentState = CurrentState with { EigenState = new EigenState.Destroyed(explosion) }; 
            return explosion; 
        }

        public void SetControlledFlight() => CurrentState = CurrentState with { EigenState = new Airplane.EigenState.ControlledFlight() };
        public void SetAutoLandingPhase(Autopilot.ApproachPhase phase) => CurrentState = CurrentState with { EigenState = new EigenState.AutoLanding(phase) };

        public bool Landing = false;

        public Dial SpeedDial;
        public Dial AltDial;

        public Airplane(Vector2 pos, BasisSpin spin)
        {
            CurrentState = new State(new Basis(pos, Vector2.UnitX * spin.ToFactor(), spin), 0f, null, null, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, new EigenState.ControlledFlight());

            SpeedDial = new Dial("Spd\r\nm/s", 0, 40f, 20, new[] { 0f, 10f, 20f, 30f }, () => this.Speed);
            AltDial = new Dial("Alt,m", 0, 60f, 12, new[] { 0f, 15f, 30f, 45f }, () => this.Position.Y);
        }

        public void RemoveRigging(World collisionWorld)
        {
            collisionWorld.Remove(Body);
        }

        private Fixture[] bodyFixtures = null!;
        private Vector2 midPointOffset = Vector2.Zero;

        public void SetupRigging(World collisionWorld, Func<object>? makeTag = null)
        {
            var body = collisionWorld.CreateBody(Aether.Vector2.Zero, 0f, BodyType.Dynamic);
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 500f;
            body.Inertia = 0f;

            this.Body = body;

            (bodyFixtures, midPointOffset) = RebuildFixtures(body, []);

            makeTag = makeTag ?? (() => this);
            Body.Tag = makeTag();
        }

        private (Fixture[], Vector2) RebuildFixtures(Body body, Fixture[] fixtures)
        {
            foreach (var fixture in fixtures) 
                body.Remove(fixture);

            var ff = Spin.ToFactor();

            Fixture tagFixture(object tag, Fixture fix)
            {
                fix.Tag = tag;
                fix.Friction = 0.0f;
                fix.Restitution = 0.0f;
                fix.CollisionCategories = GameWorld.WorldCollider.AddCategories("Airplane");
                fix.CollidesWith = GameWorld.WorldCollider.GetAll();
                return fix;
            }

            // Add fixture 0
            var vertices0 = new[]
            {
                (-0.7025f, ff * 1.9925f),
                (0.335f, ff * 1.995f),
                (0.295f, ff * 0.175f),
                (0.1225f, ff * 0f),
                (-0.135f, ff * 0f),
                (-0.2875f, ff * 0.15f),
                (-0.715f, ff * 1.565f)
            };

            var fixture0 = tagFixture("Wing", vertices0.ToPolygon(body));

            // Add fixture 1
            var vertices1 = new[]
            {
                (0.245f, ff * 1.6325f),
                (1f, ff * 1.6275f),
                (1.1525f, ff * 1.195f),
                (1.1525f, ff * 1.0175f),
                (1.0025f, ff * 0.5875f),
                (0.2325f, ff * 0.5925f)
            };

            var fixture1 = tagFixture("Cowling", vertices1.ToPolygon(body));

            // Add fixture 2
            var vertices2 = new[]
            {
                (-2.3025f, ff * 1.8775f),
                (-1.88f, ff * 1.8775f),
                (-1.6925f, ff * 1.56f),
                (-0.675f, ff * 1.5625f),
                (-0.4325f, ff * 0.76f),
                (-2.0125f, ff * 0.8925f),
                (-2.3025f, ff * 1.2075f)
            };

            var fixture2 = tagFixture("Tail", vertices2.ToPolygon(body));

            // Add fixture 3
            var vertices3 = new[]
            {
                (-1.83f, ff * 0.8975f),
                (-1.6325f, ff * 0.8825f),
                (-1.6475f, ff * 0.4525f),
                (-1.8225f, ff * 0.2975f),
                (-1.9925f, ff * 0.3f),
                (-2.1525f, ff * 0.4575f),
                (-2.14f, ff * 0.685f)
            };

            var fixture3 = tagFixture("TWheel", vertices3.ToPolygon(body));

            return (new[] { fixture0, fixture1, fixture2, fixture3 }, refPoints["midPoint"].ToXna() * ff);
        }

        private Dictionary<string, (float X, float Y)> refPoints = new Dictionary<string, (float X, float Y)>
        {
            { "gun0", (0.64f, 1.1f) },
            { "gun1", (1.15f, 1.1f) },
            { "midPoint", (0.0f, 1.1f) },
            { "origin", (0.0f, 0.0f) },
        };

        private (float X, float Y) GetRefPoint(string name)
        {
            var refPoint = refPoints[name];
            return (refPoint.X, Spin.ToFactor() * refPoint.Y);
        }

        private Bomb SpawnBomb()
        {
            var mp = Body.GetWorldPoint(GetRefPoint("midPoint").ToAether());
            var origin = Body.GetWorldPoint(GetRefPoint("origin").ToAether());
            var launchDirection = origin - mp;
            var spawnPos = origin + launchDirection * Bomb.BombHeight * 0.55f;
            var velocityVector = 
                Direction * Speed   // From Plane's flight.
                + Vector2.Normalize(launchDirection.ToXna()) * BombLaunchSpeed; // From Bomb's launcher.

            return new Bomb(new Bomb.State(spawnPos.ToXna(), Direction, velocityVector));
        }

        private Bullet SpawnBullet(TimeSpan startTime)
        {
            var gun0 = Body.GetWorldPoint(GetRefPoint("gun0").ToAether());
            var gun1 = Body.GetWorldPoint(GetRefPoint("gun1").ToAether());
            var launchDirection = gun1 - gun0;
            var spawnPos = gun1 + launchDirection * 1.5f;
            var velocityVector =
                Direction * Speed   // From Plane's flight.
                + Vector2.Normalize(launchDirection.ToXna()) * BulletSpeed;

            return new Bullet(new Bullet.State(spawnPos.ToXna(), Vector2.Normalize(velocityVector), velocityVector), startTime);
        }

        // Control Constants
        private const float Acceleration = 36f;             // meters per second^2
        private const float MaxSpeed = 36f;                 // meters per second
        public  const float MinSpeed = 5.94f;               // meters per second
        public  const float MaxLandingSpeed = 15f;          // meters per second
        public  const float MaxAutoLandingSpeed = 13.8f;    // meters per second
        public  const float CruiseSpeed = 24f;              // meters per second
        private const float MaxLandingAngle = 30f;          // Degrees
        private const float LandingProximityMax = 0.25f;    // Meters
        private const float LandingProximityMin = 0.05f;    // Meters
        public  const float PitchAngle = 240f;              // Degrees per second
        private const float RollGracePeriod = 1f / 4f;      // Time in seconds before subsequent roll input is accepted.
        private const float BombGracePeriod = 0.25f;        // Time in seconds before subsequent bomb can be spawned.
        private const float BombLaunchSpeed = 6f;           // meters per second
        private const float BulletGracePeriod = 1f / 8f;    // Time in seconds before subsequent bullet can be spawned.
        private const float BulletSpeed = 21f;              // meters per second
        private const float AccelerationGravityFactor = 0.5f;
        private const float AccelerationReverseFactor = 1.2f;
        private const float AccelerationReversePastMinFactor = 0.1f;

        public enum ThrottleInput { Throttling, None, Reversing }
        public enum PitchInput { Forward, None, Backward }
        public enum RollInput { Roll, None }
        public enum BombInput { Active, Inactive }
        public enum GunInput { Active, Inactive }
        public enum AutoLandToggle { Active, Inactive }

        public record struct Inputs(
            ThrottleInput Throttle,
            PitchInput Pitch,
            RollInput Roll,
            BombInput BombLaunch,
            GunInput GunFire,
            AutoLandToggle AutoLand)
        { 
            public static Inputs Clean() => new Inputs(ThrottleInput.None, PitchInput.None, RollInput.None, BombInput.Inactive, GunInput.Inactive, AutoLandToggle.Inactive);
        }

        public record struct InputStack(Inputs? User = null, Inputs? Autopilot = null);

        public State ApplyInputs(InputStack inputStack, Func<Autopilot.ApproachPhase> initiateAutoland, GameTime gameTime)
        {
            var input = inputStack.Autopilot ?? inputStack.User ?? Inputs.Clean();

            var nowTime = DateTime.UtcNow;
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            var (newSpin, newRollTime) = (Landing || input.Roll == RollInput.None || (nowTime - CurrentState.RollTime).TotalSeconds < RollGracePeriod) ? (Spin, CurrentState.RollTime) :
                Spin == BasisSpin.Down
                ? (BasisSpin.Up, nowTime)
                : (BasisSpin.Down, nowTime);

            var accReverseFactor =
                Speed > MinSpeed ? AccelerationReverseFactor : AccelerationReversePastMinFactor;

            var newSpeedRaw =
                input.Throttle == ThrottleInput.Throttling ? Speed + Acceleration * dt :
                input.Throttle == ThrottleInput.Reversing ? Speed - Acceleration * dt * accReverseFactor :
                Speed;

            var lowSpeedLimit =
                input.Throttle == ThrottleInput.Throttling ? 0f :                                         // Throttling - low limit irrelevant
                !Landing && input.Throttle == ThrottleInput.Reversing && newSpeedRaw < MinSpeed ? Speed : // Reversing and slipped below MinSpeed - stay at minimum, except when Landing
                0f;

            var gravityAcceleration = Acceleration * -Direction.Y * AccelerationGravityFactor * dt;
            newSpeedRaw = gravityAcceleration < 0 && newSpeedRaw <= MinSpeed ? newSpeedRaw : newSpeedRaw + gravityAcceleration;
            var newSpeed = MathHelper.Clamp(newSpeedRaw, lowSpeedLimit, MaxSpeed);

            var (newBomb, newBombTime) = (Landing || input.BombLaunch == BombInput.Inactive || (nowTime - CurrentState.BombTime).TotalSeconds < BombGracePeriod)
                ? (null, CurrentState.BombTime)
                : (SpawnBomb(), nowTime);

            var (newBullet, newBulletTime) = (input.GunFire == GunInput.Inactive || (nowTime - CurrentState.BulletTime).TotalSeconds < BulletGracePeriod)
                ? (null, CurrentState.BulletTime)
                : (SpawnBullet(gameTime.TotalGameTime), nowTime);

            var rollFactor = newSpin == BasisSpin.Down ? +1f : -1f;

            var newDirection = Speed == 0f ? Direction : Vector2.TransformNormal(Direction,
                input.Pitch == PitchInput.Backward ? Matrix.CreateRotationZ(rollFactor * MathHelper.ToRadians(PitchAngle * Speed * 2f * dt / 60f)) :
                !Landing && input.Pitch == PitchInput.Forward ? Matrix.CreateRotationZ(-rollFactor * MathHelper.ToRadians(PitchAngle * Speed * 2f * dt / 60f)) :
                Matrix.Identity);

            var newPosition = Position + newDirection * newSpeed * dt;

            EigenState newEigenState =
                CurrentState.EigenState switch
                {
                    EigenState.AutoLanding aland => aland,
                    EigenState.ControlledFlight cflight => !Landing && input.AutoLand == AutoLandToggle.Active
                        ? new EigenState.AutoLanding(initiateAutoland())   // Initiate autolanding
                        : cflight,                                         // Keep flying,
                    EigenState.Destroyed dead => dead,
                    _ => throw new NotSupportedException("Unsupported Airplane EigenState")
                };

            return new State(new Basis(newPosition, newDirection, newSpin), newSpeed, newBomb, newBullet, newBombTime, newBulletTime, newRollTime, newEigenState);
        }

        public bool CheckAndSetLandingMode(Ground.Runway runway)
        {
            bool checkLanding()
            {
                var b1 = Math.Min(runway.Start, runway.End);
                var b2 = Math.Max(runway.Start, runway.End);
                if (Position.X < b1 || Position.X > b2) return false;   // Not within range
                if (Position.Y > runway.Level + LandingProximityMax) return false; // Above threshold
                if (Speed > MaxLandingSpeed) return false;

                var angle = Spin == BasisSpin.Down 
                    ? MathHelper.ToDegrees(Direction.ToAngle()) 
                    : MathHelper.ToDegrees((-Direction).ToAngle());

                var landingAngle = Spin == BasisSpin.Down
                    ? angle <= 0f && angle > -MaxLandingAngle
                    : angle >= 0f && angle < +MaxLandingAngle;
                if (!landingAngle) return false;

                return true;
            }

            var maybeLanding = checkLanding();
            if (!maybeLanding)
            {
                Landing = false;
                return false;
            }
            else if (Landing == false)
            {
                Landing = true;
                CurrentState = CurrentState with 
                { 
                    Location = CurrentState.Location with
                    {
                        Position = Position with { Y = runway.Level + LandingProximityMin },
                        Direction = Vector2.UnitX * Spin.ToFactor()
                    }
                        
                };
            }
            return true;
        }

        public void PreSimulationPrepare(State projected)
        {
            
        }

        public void PostSimulationUpdate(State projected)
        {
            var wasNormalDown = Spin; 

            CurrentState = projected;

            if (wasNormalDown != Spin)
            {
                // Executing a roll/flip.
                // Reflect position across horizontal midline.
                CurrentState = CurrentState with { Location = CurrentState.Location with { Position = Position.ReflectPointAcrossLine(MidPoint, Direction) } };

                // Rebuild Aether2D Body from flipped perspective.
                (bodyFixtures, midPointOffset) = RebuildFixtures(Body, bodyFixtures);
            }

            Body.Position = Position.ToAether();
            Body.Rotation = Direction.ToAngle();
            // For dynamic body LinearVelocity seems to be updating with forced Position change, causing incorrect collision detection.
            // Forcibly resetting it to zero mitigates the issue.
            // Correct approach would be to let Aether fully simulate dynamic body.
            Body.LinearVelocity = Aether.Vector2.Zero;
        }
    }
}

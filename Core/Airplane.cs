using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using VibeSopwith.Game.Utils;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class Airplane : ICentered, ISimulated<Airplane.State>
    {
        public Body Body = null!;

        public record State(Vector2 Position, Vector2 Direction, BasisSpin Spin, float Speed, Bomb? Bomb, Bullet? Bullet, DateTime RollTime, DateTime BombTime, DateTime BulletTime);

        public State CurrentState;
        public float Speed { get => CurrentState.Speed; }

        public Vector2 Position { get => CurrentState.Position; }       // World position of the plane in meters. 
        public Vector2 Direction { get => CurrentState.Direction; }     // Direction where plane is facing.
        public BasisSpin Spin { get => CurrentState.Spin; }             // Orientation of basis spin. Up is Y-up.
        private float FlipFactor { get => Spin == BasisSpin.Down ? +1f : -1f; }
        public float Length => 3.46f;
        public float Height => 2.0f;

        public Vector2 MidPoint { get => Position + midPointOffset.Rotate(Direction.ToAngle()); }

        public bool Exploded = false;

        public Airplane(Vector2 pos, BasisSpin spin)
        {
            CurrentState = new State(pos, Vector2.UnitX, spin, 0f, null, null, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
            CurrentState = CurrentState with { Direction = Direction * FlipFactor };
        }

        public void RemoveRigging(World collisionWorld)
        {
            collisionWorld.Remove(Body);
        }

        private Fixture[] bodyFixtures = null!;
        private Vector2 midPointOffset = Vector2.Zero;

        public void SetupRigging(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Aether.Vector2.Zero, 0f, BodyType.Dynamic);
            body.Tag = this;
            body.FixedRotation = true;
            body.Mass = 500f;
            body.Inertia = 0f;

            this.Body = body;

            (bodyFixtures, midPointOffset) = RebuildFixtures(body, []);
        }

        private (Fixture[], Vector2) RebuildFixtures(Body body, Fixture[] fixtures)
        {
            foreach (var fixture in fixtures) 
                body.Remove(fixture);

            var ff = FlipFactor;

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

            var fixture0 = vertices0.ToPolygon(body);
            fixture0.Tag = "Wing";
            fixture0.Friction = 0.0f;
            fixture0.Restitution = 0.0f;
            fixture0.CollisionCategories = Category.Cat1;
            fixture0.CollidesWith = Category.All;

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

            var fixture1 = vertices1.ToPolygon(body);
            fixture1.Tag = "Cowling";
            fixture1.Friction = 0.0f;
            fixture1.Restitution = 0.0f;
            fixture1.CollisionCategories = Category.Cat1;
            fixture1.CollidesWith = Category.All;

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

            var fixture2 = vertices2.ToPolygon(body);
            fixture2.Tag = "Tail";
            fixture2.Friction = 0.0f;
            fixture2.Restitution = 0.0f;
            fixture2.CollisionCategories = Category.Cat1;
            fixture2.CollidesWith = Category.All;

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

            var fixture3 = vertices3.ToPolygon(body);
            fixture3.Tag = "TWheel";
            fixture3.Friction = 0.0f;
            fixture3.Restitution = 0.0f;
            fixture3.CollisionCategories = Category.Cat1;
            fixture3.CollidesWith = Category.All;

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
            return (refPoint.X, FlipFactor * refPoint.Y);
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
            var spawnPos = gun1 + launchDirection * 0.1f;
            var velocityVector =
                Direction * Speed   // From Plane's flight.
                + Vector2.Normalize(launchDirection.ToXna()) * BulletSpeed;

            return new Bullet(new Bullet.State(spawnPos.ToXna(), Vector2.Normalize(velocityVector), velocityVector), startTime);
        }

        private const float Acceleration = 0.01f;       // meters per second^2
        private const float MaxSpeed = 0.5f;            // meters per second
        private const float MinSpeed = 0.099f;          // meters per second
        private const float MaxLandingSpeed = 0.25f;    // meters per second
        private const float MaxLandingAngle = 30f;      // Degrees
        private const float LandingProximityMax = 0.25f;// Meters
        private const float LandingProximityMin = 0.05f;// Meters
        private const float PitchAngle = 4.0f;          // Degrees
        private const float RollGracePeriod = 1f / 4f;  // Time in seconds before subsequent roll input is accepted.
        private const float BombGracePeriod = 0.25f;    // Time in seconds before subsequent bomb can be spawned.
        private const float BombLaunchSpeed = 0.1f;     // meters per second
        private const float BulletGracePeriod = 1f / 8f;   // Time in seconds before subsequent bullet can be spawned.
        private const float BulletSpeed = 0.35f;           // meters per second

        public enum ThrottleInput { Throttling, None, Reversing }
        public enum PitchInput { Forward, None, Backward }
        public enum RollInput { Roll, None }
        public enum BombInput { Active, Inactive }
        public enum GunInput { Active, Inactive }

        public ThrottleInput Throttle;
        public PitchInput Pitch;
        public RollInput Roll;
        public BombInput BombLaunch;
        public GunInput GunFire;

        public bool Landing = false;

        public State ApplyInputs(GameTime gameTime)
        {
            var newSpeedRaw =
                Throttle == ThrottleInput.Throttling ? Speed + Acceleration :
                Throttle == ThrottleInput.Reversing ? Speed - Acceleration :
                Speed;
            var lowSpeedLimit =
                Throttle == ThrottleInput.Throttling ? 0f :                                         // Throttling - low limit irrelevant
                !Landing && Throttle == ThrottleInput.Reversing && newSpeedRaw < MinSpeed ? Speed : // Reversing and slipped below MinSpeed - stay at minimum
                0f; 
            var newSpeed = MathHelper.Clamp(newSpeedRaw, lowSpeedLimit, MaxSpeed);

            var nowTime = DateTime.UtcNow;

            var (newBomb, newBombTime) = (Landing || BombLaunch == BombInput.Inactive || (nowTime - CurrentState.BombTime).TotalSeconds < BombGracePeriod) 
                ? (null, CurrentState.BombTime) 
                : (SpawnBomb(), nowTime);

            var (newBullet, newBulletTime) = (GunFire == GunInput.Inactive || (nowTime - CurrentState.BulletTime).TotalSeconds < BulletGracePeriod)
                ? (null, CurrentState.BulletTime)
                : (SpawnBullet(gameTime.TotalGameTime), nowTime);

            var (newSpin, newRollTime) = (Landing || Roll == RollInput.None || (nowTime - CurrentState.RollTime).TotalSeconds < RollGracePeriod) ? (Spin, CurrentState.RollTime) :
                Spin == BasisSpin.Down
                ? (BasisSpin.Up, nowTime)
                : (BasisSpin.Down, nowTime);

            var rollFactor = newSpin == BasisSpin.Down ? +1f : -1f;

            var newDirection = Speed == 0f ? Direction : Vector2.TransformNormal(Direction,
                Pitch == PitchInput.Backward ? Matrix.CreateRotationZ(rollFactor * MathHelper.ToRadians(PitchAngle * Speed * 2f)) :
                !Landing && Pitch == PitchInput.Forward ? Matrix.CreateRotationZ(-rollFactor * MathHelper.ToRadians(PitchAngle * Speed * 2f)) :
                Matrix.Identity);

            var newPosition = Position + newDirection * newSpeed;

            return new State(newPosition, newDirection, newSpin, newSpeed, newBomb, newBullet, newRollTime, newBombTime, newBulletTime);
        }

        public void ClearInputs()
        {
            Throttle = ThrottleInput.None;
            Pitch = PitchInput.None;
            Roll = RollInput.None;
            BombLaunch = BombInput.Inactive;
            GunFire = GunInput.Inactive;
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
                CurrentState = CurrentState with { Position = Position with { Y = runway.Level + LandingProximityMin }, Direction = Spin == BasisSpin.Down ? Vector2.UnitX : -Vector2.UnitX };
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
                CurrentState = CurrentState with { Position = Position.ReflectPointAcrossLine(MidPoint, Direction) };

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

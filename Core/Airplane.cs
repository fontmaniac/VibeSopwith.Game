using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Core
{
    internal class Airplane : ICentered
    {
        public record State(Vector2 Position, Vector2 Direction, Winding NormalDown, float Speed, DateTime RollTime);

        public State CurrentState = new State(Vector2.Zero, Vector2.UnitX, Winding.Clockwise, 0f, DateTime.MinValue);
        public float Speed { get => CurrentState.Speed; }

        public Vector2 Position { get => CurrentState.Position; }       // World position of geometrical center of the plane in meters.
        public Vector2 Direction { get => CurrentState.Direction; }     // Direction where plane is facing.
        public Winding NormalDown { get => CurrentState.NormalDown; }   // Orientation of normal pointing towards bottom of the plane.
        public float Length => 3.46f;
        public float Height => 2.0f;

        public void Place(Vector2 pos, Winding normalDown) => CurrentState = CurrentState with { Position = pos, NormalDown = normalDown };

        private const float Acceleration = 0.1f; // meters per second^2
        private const float MaxSpeed = 0.5f; // meters per second
        private const float PitchAngle = 4.0f;
        private const float RollGracePeriod = 1f / 4f; // Time in seconds before subsequent roll input is accepted.

        public enum ThrottleInput { Throttling, None, Reversing }
        public enum PitchInput { Forward, None, Backward }
        public enum RollInput { Roll, None }

        public ThrottleInput Throttle;
        public PitchInput Pitch;
        public RollInput Roll;

        private DateTime _lastRoll = DateTime.MinValue;

        public State ApplyInputs(GameTime gameTime)
        {
            var newSpeed = MathHelper.Clamp((
                Throttle == ThrottleInput.Throttling ? Speed + Acceleration :
                Throttle == ThrottleInput.Reversing ? Speed - Acceleration :
                Speed), 0f, MaxSpeed);

            var nowTime = DateTime.UtcNow;
            var (newWinding, newRollTime) = (Roll == RollInput.None || (nowTime - CurrentState.RollTime).TotalSeconds < RollGracePeriod) ? (NormalDown, CurrentState.RollTime) :
                NormalDown == Winding.Clockwise
                ? (Winding.CounterClockwise, nowTime)
                : (Winding.Clockwise, nowTime);

            var rollFactor = newWinding == Winding.Clockwise ? +1f : -1f;

            var newDirection = Speed == 0f ? Direction : Vector2.TransformNormal(Direction,
                Pitch == PitchInput.Backward ? Matrix.CreateRotationZ(rollFactor * MathHelper.ToRadians(PitchAngle)) :
                Pitch == PitchInput.Forward  ? Matrix.CreateRotationZ(rollFactor * MathHelper.ToRadians(PitchAngle)) :
                Matrix.Identity);

            var newPosition = Position + newDirection * newSpeed;

            return new State(newPosition, newDirection, newWinding, newSpeed, newRollTime);
        }

        public void ClearInputs()
        {
            Throttle = ThrottleInput.None;
            Pitch = PitchInput.None;
            Roll = RollInput.None;
        }

    }
}

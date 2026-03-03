using Microsoft.Xna.Framework;

namespace VibeSopwith.Core
{
    internal class Plane : ICentered
    {
        public record State(Vector2 Position, Vector2 Direction, Winding NormalDown, float Speed);

        public State CurrentState = new State(Vector2.Zero, Vector2.UnitX, Winding.Clockwise, 0f);
        public float Speed { get => CurrentState.Speed; }

        public Vector2 Position { get => CurrentState.Position; }   // World position of geometrical center of the plane in meters.
        public Vector2 Direction { get => CurrentState.Direction; } // Direction where plane is facing.
        public Winding NormalDown { get => CurrentState.NormalDown; } // Orientation of normal pointing towards bottom of the plane.
        public float Length => 3.46f;
        public float Height => 2.0f;

        public void Place(Vector2 pos, Winding normalDown) => CurrentState = CurrentState with { Position = pos, NormalDown = normalDown };
    }
}

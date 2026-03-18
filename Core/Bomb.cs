using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game.Core
{
    internal class Bomb : ICentered
    {
        public Body Body = null!;
        public record State(Vector2 Position, Vector2 Direction, Vector2 Velocity);
        public State CurrentState;

        public Vector2 Position { get => CurrentState.Position; }
        public Vector2 Direction { get => CurrentState.Direction; }
        public Winding NormalDown => Winding.Clockwise;
        public float Length => 1.5f;
        public float Height => 1.0f;

        public Bomb(State initialState)
        {
            CurrentState = initialState;
        }

    }
}

using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Core
{
    internal class Explosion
    {
        public Vector2 RootPosition { get; set; } = Vector2.Zero;   // World position of explosion "root" - point in the middle of the bottom edge of the texture.
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

        public float Length;
        public float Height;

        public TimeSpan StartTime;

        public Explosion(float length, float height, TimeSpan startTime)
        {
            Length = length;
            Height = height;
            StartTime = startTime;
        }

        public bool IsExpired(TimeSpan gameTime) => StartTime + Duration < gameTime; 
    }
}

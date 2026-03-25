using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Core
{
    internal class Explosion
    {
        public Vector2 RootPosition { get; set; } = Vector2.Zero;   // World position of explosion "root" - point in the middle of the bottom edge of the texture.
        public TimeSpan Duration { get; set; }

        public enum ExplosionVariant { Based1, Centered1, BluePlasma }
        public ExplosionVariant Variant { get; }

        public float Length;
        public float Height;

        public TimeSpan StartTime;

        public Explosion(ExplosionVariant variant, float length, float height, TimeSpan startTime, TimeSpan duration)
        {
            Length = length;
            Height = height;
            StartTime = startTime;
            Variant = variant;
            Duration = duration;
        }

        public bool IsExpired(TimeSpan gameTime) => StartTime + Duration < gameTime; 
    }
}

using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Core
{
    internal class Explosion : IHasLocation
    {
        public TimeSpan Duration { get; set; }

        public enum ExplosionVariant { Based1, Centered1, BluePlasma }
        public ExplosionVariant Variant { get; }

        public IBasis World { get; }
        public Vector2 Position { get => World.Position; }
        public Vector2 Direction { get => World.Direction; }
        public BasisSpin Spin { get => World.Spin; }

        public float Length { get; }
        public float Height { get; }

        public TimeSpan StartTime;

        public Explosion(ExplosionVariant variant, float length, float height, TimeSpan startTime, TimeSpan duration, IBasis WorldPosition)
        {
            Length = length;
            Height = height;
            StartTime = startTime;
            Variant = variant;
            Duration = duration;
            World = WorldPosition;
        }

        public bool IsExpired(TimeSpan gameTime) => StartTime + Duration < gameTime; 
    }
}

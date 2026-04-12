using Microsoft.Xna.Framework;
using Nage.Strata.Abstractions.Spatial;

namespace VibeSopwith.Game.Core
{
    internal class Explosion : IHasLocation
    {
        public TimeSpan Duration { get; set; }

        public enum ExplosionVariant { Based1, Centered1, BluePlasma }
        public ExplosionVariant Variant { get; }

        public IBasis WorldLocation { get; }
        public Vector2 Position { get => WorldLocation.Position; }
        public Vector2 Direction { get => WorldLocation.Direction; }
        public BasisSpin Spin { get => WorldLocation.Spin; }

        public float Length { get; }
        public float Height { get; }

        public TimeSpan StartTime;

        public Explosion(ExplosionVariant variant, float length, float height, TimeSpan startTime, TimeSpan duration, IBasis worldLocation)
        {
            Length = length;
            Height = height;
            StartTime = startTime;
            Variant = variant;
            Duration = duration;
            WorldLocation = worldLocation;
        }

        public bool IsExpired(TimeSpan gameTime) => StartTime + Duration < gameTime; 
    }
}

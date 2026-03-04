using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Core
{
    internal class Ground 
    {
        public readonly List<Vector2> Points;

        private Ground(List<Vector2> points)
        {
            Points = points;
        }

        public static Ground MakeRandom()
        {
            var points = new List<Vector2>();

            for (int i = 0; i <= 20; i++) 
            {
                var x = GameWorld.WorldLength * (i / 20.0f); 
                var y = (float)(GameWorld.WorldSeed.NextDouble() * (GameWorld.WorldHeight * 0.5) + (GameWorld.WorldHeight * 0.25));
                points.Add(new Vector2(x, y));
            }

            return new Ground(points);
        }

        public static Ground MakeFlat(float heightPercentage)
        {
            var points = new List<Vector2>();

            for (int i = 0; i <= 20; i++) 
            {
                var x = GameWorld.WorldLength * (i / 20.0f); 
                var y = (GameWorld.WorldHeight * heightPercentage);
                points.Add(new Vector2(x, y));
            }

            return new Ground(points);
        }
    }
}
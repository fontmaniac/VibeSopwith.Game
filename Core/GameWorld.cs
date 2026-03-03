namespace VibeSopwith.Core
{
    internal class GameWorld 
    {
        public const int WorldLength = 2000;
        public const int WorldHeight = 100;

        public static readonly Random WorldSeed = new Random(12345);

        public readonly Ground Ground;

        public GameWorld()
        {
            //Ground = Ground.MakeFlat(0.25f); 
            Ground = Ground.MakeRandom();
        }
    }
}

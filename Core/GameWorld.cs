using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Core
{
    internal class GameWorld 
    {
        public const int WorldLength = 1000;
        public const int WorldHeight = 50;

        public static readonly Random WorldSeed = new Random(12345);

        public readonly Ground Ground;
        public readonly Airplane Plane;

        public GameWorld()
        {
            //Ground = Ground.MakeFlat(0.25f); 
            Ground = Ground.MakeRandom();
            Plane = new Airplane();
            Plane.Place(new Vector2(WorldLength / 2f, WorldHeight / 2f), Winding.Clockwise);

        }
    }
}

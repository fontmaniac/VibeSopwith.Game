using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class GameWorld 
    {
        public const int WorldLength = 1000;
        public const int WorldHeight = 50;

        public static readonly Random WorldSeed = new Random(12345);

        public readonly Ground Ground;
        public readonly Airplane Plane;

        public readonly Explosion TestExplosion;

        private readonly World collisionWorld;

        public GameWorld()
        {
            //Ground = Ground.MakeFlat(0.25f); 
            Ground = Ground.MakeRandom();
            Plane = new Airplane();
            Plane.Place(new Vector2(WorldLength / 2f, WorldHeight / 2f), Winding.Clockwise);

            TestExplosion = new Explosion(16f, 16f, TimeSpan.FromSeconds(2));
            TestExplosion.RootPosition = Plane.Position;

            collisionWorld = new World()
            {
                Gravity = Aether.Vector2.Zero
            };

            Ground.SetupRigging(collisionWorld);
        }

        public void Simulate(GameTime gameTime)
        {
            var planeProjected = Plane.ApplyInputs(gameTime);

            Plane.CurrentState = planeProjected;

            Plane.ClearInputs();
        }

    }
}

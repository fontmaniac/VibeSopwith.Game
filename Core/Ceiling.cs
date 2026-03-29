using nkast.Aether.Physics2D.Dynamics;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class Ceiling
    {
        public Body Body = null!;

        public void SetupRigging(World collisionWorld)
        {
            var body = collisionWorld.CreateBody(Aether.Vector2.Zero, 0f, BodyType.Static);
            body.Tag = this;
            body.IgnoreGravity = true;
            body.Mass = float.MaxValue;

            var fixture = body.CreateRectangle(GameWorld.WorldLength+10f, 2f, 1f, new Aether.Vector2(GameWorld.WorldLength/2f, GameWorld.WorldHeight+2f));
            fixture.CollisionCategories = Category.Cat2;

            Body = body;
        }
    }
}

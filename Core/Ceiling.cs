using nkast.Aether.Physics2D.Dynamics;
using Aether = nkast.Aether.Physics2D.Common;

namespace VibeSopwith.Game.Core
{
    internal class Ceiling
    {
        public Body Body = null!;

        public void SetupRigging(World simWorld)
        {
            var body = simWorld.CreateBody(Aether.Vector2.Zero, 0f, BodyType.Static);
            body.Tag = this;
            body.IgnoreGravity = true;
            body.Mass = float.MaxValue;

            var fixture = body.CreateRectangle(GameWorld.WorldLength+10f, 2f, 1f, new Aether.Vector2(GameWorld.WorldLength/2f, GameWorld.WorldHeight + 2f));
            fixture.CollisionCategories = GameWorld.WorldCollider.AddCategories("Ceiling");
            fixture.CollidesWith = GameWorld.WorldCollider.GetAll() & ~GameWorld.WorldCollider.AddCategories("Bomb", "Bullet");

            Body = body;
        }
    }
}

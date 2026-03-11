using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VibeSopwith.Game.Graphics
{
    internal static class Animation
    {
        public interface IPhase
        {
            
        }

        public interface ISequence
        {
            GameTime StartTime { get; }

        }

        public static void Draw(this ISequence sequence, GameTime gameTime, SpriteBatch spriteBatch)
        {
            
        }
    }
}

using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Core
{
    public class UpsCounter
    {
        private int _frameCounter = 0;
        private double _elapsedTime = 0;

        public int UPS { get; private set; }

        public void Update(GameTime gameTime)
        {
            _elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter++;

            if (_elapsedTime >= 1.0)
            {
                UPS = _frameCounter;
                _frameCounter = 0;
                _elapsedTime = 0;
            }
        }
    }
}

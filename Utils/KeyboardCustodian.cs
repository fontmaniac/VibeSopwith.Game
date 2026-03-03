using Microsoft.Xna.Framework.Input;

namespace VibeSopwith.Utils
{
    internal class KeyboardCustodian
    {
        public KeyboardState PreviousState { get; private set; }
        public KeyboardState CurrentState { get; private set; }

        public KeyboardCustodian()
        {
            PreviousState = CurrentState = Keyboard.GetState();
        }

        public bool IsKeyPressed(Keys key)
        {
            return CurrentState.IsKeyDown(key) && !PreviousState.IsKeyDown(key);
        }

        public bool IsKeyDown(Keys key)
        {
            return CurrentState.IsKeyDown(key);
        }

        public void Process(Action<KeyboardCustodian> kc)
        {
            CurrentState = Keyboard.GetState();
            kc(this);
            PreviousState = CurrentState;
        }
    }
}

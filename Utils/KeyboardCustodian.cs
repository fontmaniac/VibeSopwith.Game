using Microsoft.Xna.Framework.Input;

namespace VibeSopwith.Game.Utils
{
    internal class KeyboardCustodian
    {
        public interface Interface
        {
            public bool IsKeyPressed(Keys key);
            public bool IsKeyDown(Keys key);
        }

        private record TheInterface(KeyboardCustodian custodian) : Interface
        {
            public bool IsKeyDown(Keys key) => custodian.IsKeyDown(key);
            public bool IsKeyPressed(Keys key) => custodian.IsKeyPressed(key);
        }

        private TheInterface _proxy;

        private KeyboardState PreviousState { get; set; }
        private KeyboardState CurrentState { get; set; }

        public KeyboardCustodian()
        {
            PreviousState = CurrentState = Keyboard.GetState();
            _proxy = new TheInterface(this);
        }

        private bool IsKeyPressed(Keys key)
        {
            return CurrentState.IsKeyDown(key) && !PreviousState.IsKeyDown(key);
        }

        private bool IsKeyDown(Keys key)
        {
            return CurrentState.IsKeyDown(key);
        }

        public TInputs Process<TInputs>(Func<KeyboardCustodian.Interface, TInputs> kc)
        {
            CurrentState = Keyboard.GetState();
            var collectedInputs = kc(_proxy);
            PreviousState = CurrentState;
            return collectedInputs;
        }
    }
}

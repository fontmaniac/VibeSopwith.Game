using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VibeSopwith.Game.Core; 
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game
{
    public class TheGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private KeyboardCustodian _keyboardCustodian = new KeyboardCustodian();

        public static SpriteBatch SpriteBatch = null!;
        private BasicEffect _basicEffect = null!;

        private Components.WorldRender _worldRender = null!;
        private readonly Core.GameWorld _world;

        public TheGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1600; 
            _graphics.PreferredBackBufferHeight = 1000;
            _graphics.SynchronizeWithVerticalRetrace = true;

            Content.RootDirectory = "Content";

            this.Window.AllowUserResizing = true;
            this.IsMouseVisible = true;

            _world = new Core.GameWorld();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
           
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            _basicEffect = new BasicEffect(GraphicsDevice);
            
            _worldRender = new Components.WorldRender(this);
            _worldRender.LoadContent();
        }

        protected override void UnloadContent()
        {
            _basicEffect?.Dispose();
            SpriteBatch?.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _keyboardCustodian.Process(kc =>
            {
                // F11 - Fullscreen toggle
                if (kc.IsKeyPressed(Keys.F11))
                {
                    _graphics.IsFullScreen = !_graphics.IsFullScreen;
                    _graphics.ApplyChanges();
                }

                // ESC - Exit
                if (kc.IsKeyPressed(Keys.Escape))
                    this.Exit();

                _world.Plane.Throttle =
                    (kc.IsKeyDown(Keys.A) && !kc.IsKeyDown(Keys.D)) ? Airplane.ThrottleInput.Reversing :
                    (!kc.IsKeyDown(Keys.A) && kc.IsKeyDown(Keys.D)) ? Airplane.ThrottleInput.Throttling :
                    Airplane.ThrottleInput.None;

                _world.Plane.Pitch =
                    (kc.IsKeyDown(Keys.W) && !kc.IsKeyDown(Keys.S)) ? Airplane.PitchInput.Backward :
                    (!kc.IsKeyDown(Keys.W) && kc.IsKeyDown(Keys.S)) ? Airplane.PitchInput.Forward :
                    Airplane.PitchInput.None;

                _world.Plane.Roll =
                    kc.IsKeyDown(Keys.X) ? Airplane.RollInput.Roll : Airplane.RollInput.None;

            });

            _world.Simulate(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            var scale = (float)GraphicsDevice.Viewport.Height / Core.GameWorld.WorldHeight;
            var viewportWidthInWorldUnits = GraphicsDevice.Viewport.Width / scale;
            var minCameraX = viewportWidthInWorldUnits / 2f;
            var maxCameraX = Core.GameWorld.WorldLength - viewportWidthInWorldUnits / 2f;

            var cameraPositionX = MathHelper.Clamp(_world.Plane.Position.X, minCameraX, maxCameraX);

            _worldRender.Draw(_world, gameTime, cameraPositionX - minCameraX); 
        }
    }
}
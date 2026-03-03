using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using VibeSopwith.Utils;
using VibeSopwith.Core; // Added for GameWorld constants
using VibeSopwith.Components; // Added for WorldRender

namespace VibeSopwith
{
    public class TheGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private KeyboardCustodian _keyboardCustodian = new KeyboardCustodian();

        public static SpriteBatch SpriteBatch = null!;
        private BasicEffect _basicEffect = null!;

        private Components.WorldRender _worldRender = null!;
        private readonly Core.GameWorld _world;

        // Camera State
        private float _cameraPositionX = 0f;
        private const float ScrollSpeed = 500f; // World units per second

        public TheGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1600; // Default resolution
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

                // Handle horizontal scrolling input
                var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _cameraPositionX +=
                    kc.IsKeyDown(Keys.A) ? -ScrollSpeed * deltaTime :
                    kc.IsKeyDown(Keys.D) ? +ScrollSpeed * deltaTime :
                    0f;

                // Clamp CameraPositionX
                var scale = (float)GraphicsDevice.Viewport.Height / Core.GameWorld.WorldHeight;
                var viewportWidthInWorldUnits = GraphicsDevice.Viewport.Width / scale;
                var maxCameraX = Core.GameWorld.WorldLength - viewportWidthInWorldUnits;

                _cameraPositionX = MathHelper.Clamp(_cameraPositionX, 0f, Math.Max(0f, maxCameraX)); // Ensure maxCameraX isn't negative for small worlds
            });
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
            
            _worldRender.Draw(_world, gameTime, _cameraPositionX); // Pass CameraPositionX
        }
    }
}
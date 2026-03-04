using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;
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
        private Components.Dashboard _dashboard = null!;
        private Components.AirplaneGizmoRender _gizmo = null!;

        private readonly Core.GameWorld _world;

        public const float PlaneGizmoWidth = 4f;
        public const float PlaneGizmoHeight = 4f;

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

            MlemPlatform.Current = new MlemPlatform.DesktopFna(a => TextInputEXT.TextInput += a);
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

            _dashboard = new Components.Dashboard(this);
            _dashboard.LoadContent();

            _gizmo = new Components.AirplaneGizmoRender(this);
            _gizmo.LoadContent();
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

                // Adjust throttle if flipped.
                _world.Plane.Throttle =
                    _world.Plane.CurrentState.NormalDown == Winding.Clockwise ? _world.Plane.Throttle :
                    _world.Plane.Throttle == Airplane.ThrottleInput.Throttling ? Airplane.ThrottleInput.Reversing :
                    _world.Plane.Throttle == Airplane.ThrottleInput.Reversing ? Airplane.ThrottleInput.Throttling :
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

        // This is almost cosmetic, as SpriteBatch ignores viewport settings completely.
        private void DrawInViewport(Viewport vp, Viewport full, Action draw)
        {
            GraphicsDevice.Viewport = vp;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            draw();
            GraphicsDevice.Viewport = full;
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(Color.Black);

            var full = GraphicsDevice.Viewport;
            var bnd = full.Height - 80;

            var mainViewport = new Viewport(0, 0, full.Width, bnd);
            var dashViewport = new Viewport(0, bnd, 200, 80);
            var gizmoViewport = new Viewport(200, bnd, 80, 80);

            DrawInViewport(gizmoViewport, full, () =>
            {
                _gizmo.PreparedDraw(_world.Plane);
            });

            DrawInViewport(mainViewport, full, () =>
            {
                var scale = (float)GraphicsDevice.Viewport.Height / Core.GameWorld.WorldHeight;
                var viewportWidthInWorldUnits = GraphicsDevice.Viewport.Width / scale;
                var minCameraX = viewportWidthInWorldUnits / 2f;
                var maxCameraX = Core.GameWorld.WorldLength - viewportWidthInWorldUnits / 2f;

                var cameraPositionX = MathHelper.Clamp(_world.Plane.Position.X, minCameraX, maxCameraX);

                _worldRender.Draw(_world, gameTime, cameraPositionX - minCameraX);
            });

            DrawInViewport(dashViewport, full, () =>
            {
                _dashboard.Draw(_world.Plane, dashViewport, gameTime);
            });

            DrawInViewport(gizmoViewport, full, () =>
            {
                _gizmo.Draw(gameTime);
            });
        }
    }
}
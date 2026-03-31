using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;
using VibeSopwith.Game.Components;
using VibeSopwith.Game.Core; 
using VibeSopwith.Game.Utils;

namespace VibeSopwith.Game
{
    public class TheGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private KeyboardCustodian _keyboardCustodian = new KeyboardCustodian();

        public static SpriteBatch SpriteBatch = null!;
        public static BasicEffect BasicEffect = null!;
        public static PrimitivesRender Primitives = null!;

        private Components.WorldRender _worldRender = null!;
        private Components.Dashboard _dashboard = null!;
        private Components.AirplaneGizmoRender _gizmo = null!;

        private readonly Core.GameWorld _world;
        public readonly static UpsCounter UPS = new UpsCounter();

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
            BasicEffect = new BasicEffect(GraphicsDevice);

            Primitives = new PrimitivesRender(this);
            Primitives.LoadContent();

            _worldRender = new Components.WorldRender(this);
            _worldRender.LoadContent();

            _dashboard = new Components.Dashboard(this);
            _dashboard.LoadContent();

            _gizmo = new Components.AirplaneGizmoRender(this);
            _gizmo.LoadContent();
        }

        protected override void UnloadContent()
        {
            BasicEffect?.Dispose();
            SpriteBatch?.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UPS.Update(gameTime);

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

                if (kc.IsKeyPressed(Keys.F12))
                    Console.WriteLine($"Updates per second: {UPS.UPS}");

                var throttle =
                    (kc.IsKeyDown(Keys.A) && !kc.IsKeyDown(Keys.D)) ? Airplane.ThrottleInput.Reversing :
                    (!kc.IsKeyDown(Keys.A) && kc.IsKeyDown(Keys.D)) ? Airplane.ThrottleInput.Throttling :
                    Airplane.ThrottleInput.None;

                // Adjust throttle if flipped.
                throttle =
                    _world.Plane.CurrentState.Spin == BasisSpin.Down ? throttle :
                    throttle == Airplane.ThrottleInput.Throttling ? Airplane.ThrottleInput.Reversing :
                    throttle == Airplane.ThrottleInput.Reversing ? Airplane.ThrottleInput.Throttling :
                    Airplane.ThrottleInput.None;

                var pitch =
                    (kc.IsKeyDown(Keys.W) && !kc.IsKeyDown(Keys.S)) ? Airplane.PitchInput.Backward :
                    (!kc.IsKeyDown(Keys.W) && kc.IsKeyDown(Keys.S)) ? Airplane.PitchInput.Forward :
                    Airplane.PitchInput.None;

                var roll =
                    kc.IsKeyDown(Keys.X) ? Airplane.RollInput.Roll : Airplane.RollInput.None;

                var bombLaunch =
                    kc.IsKeyDown(Keys.B) ? Airplane.BombInput.Active : Airplane.BombInput.Inactive;

                var gunFire =
                    kc.IsKeyDown(Keys.Space) ? Airplane.GunInput.Active : Airplane.GunInput.Inactive;

                var autoLand =
                    kc.IsKeyDown(Keys.H) && _world.Plane.CurrentState.AutoLanding == null ? Airplane.AutoLandToggle.Active : Airplane.AutoLandToggle.Inactive;

                _world.Plane.Input = _world.Plane.Input with
                {
                    Throttle = throttle,
                    Pitch = pitch,
                    Roll = roll,
                    BombLaunch = bombLaunch,
                    GunFire = gunFire,
                    AutoLand = autoLand,
                };
            });

            _world.Simulate(gameTime, UPS.UPS);
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

            var full = GraphicsDevice.Viewport;
            var bnd = full.Height - 120;

            var mainViewport = new Viewport(0, 0, full.Width, bnd);
            var dashViewport = new Viewport(0, bnd, 200, 120);
            var gizmoViewport = new Viewport(200, bnd, 120, 120);
            var minimapViewport = new Viewport(full.Width / 2 + 5, bnd + 20, full.Width / 2 - 10, 120 - 40);

            GraphicsDevice.Clear(Color.Black);

            DrawInViewport(mainViewport, full, () =>
            {
                var scale = (float)GraphicsDevice.Viewport.Height / Core.GameWorld.WorldHeight;
                var viewportWidthInWorldUnits = GraphicsDevice.Viewport.Width / scale;
                var minCameraX = viewportWidthInWorldUnits / 2f;
                var maxCameraX = Core.GameWorld.WorldLength - viewportWidthInWorldUnits / 2f;

                var cameraPositionX = MathHelper.Clamp(_world.Plane.MidPoint.X, minCameraX, maxCameraX);

                _worldRender.Draw(_world, gameTime, cameraPositionX - minCameraX);
            });

            DrawInViewport(dashViewport, full, () =>
            {
                _dashboard.Draw(_world.Plane, dashViewport, gameTime);
            });

            DrawInViewport(gizmoViewport, full, () =>
            {
                _gizmo.Draw(_world.Plane, gameTime);
            });

            DrawInViewport(minimapViewport, full, () =>
            {
                _worldRender.DrawMinimap(_world, gameTime);
            });
        }
    }
}
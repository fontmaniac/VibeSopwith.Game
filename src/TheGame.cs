using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;
using Nage.Strata.Graphics.Global;
using Nage.Strata.Utils;
using VibeSopwith.Game.Components;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game;

public class TheGame : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private KeyboardCustodian _keyboardCustodian = new KeyboardCustodian();

    public static SpriteBatch SpriteBatchLinear = null!;
    public static SpriteBatch SpriteBatchPoint = null!;
    public static BasicEffect BasicEffect = null!;

    public static IRenderers Global = null!;

    private Components.WorldRender _worldRender = null!;
    private Components.Dashboard _dashboard = null!;
    private Components.AirplaneGizmoRender _gizmo = null!;
    private Components.DialRoundRender _dialRoundRender = null!;

    private readonly Core.GameWorld _world;
    public readonly static UpsCounter UPS = new UpsCounter();
    public readonly static UpsCounter FPS = new UpsCounter();

    public const float PlaneGizmoWidth = 4f;
    public const float PlaneGizmoHeight = 4f;

    public TheGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1920; 
        _graphics.PreferredBackBufferHeight = 1080;
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

        SpriteBatchLinear = new SpriteBatch(GraphicsDevice);
        SpriteBatchPoint = new SpriteBatch(GraphicsDevice);
        BasicEffect = new BasicEffect(GraphicsDevice);

        Global = RenderersBag.Init(this);

        _worldRender = new Components.WorldRender(this);
        _worldRender.LoadContent();

        _dashboard = new Components.Dashboard(this);
        _dashboard.LoadContent();

        _gizmo = new Components.AirplaneGizmoRender(this);
        _gizmo.LoadContent();

        _dialRoundRender = new Components.DialRoundRender(this);
        _dialRoundRender.LoadContent();
    }

    protected override void UnloadContent()
    {
        BasicEffect?.Dispose();
        SpriteBatchPoint?.Dispose();
        SpriteBatchLinear?.Dispose();
        Global?.Dispose();
    }

    public  record ControlScheme(Keys LThrottle, Keys RThrottle, Keys UPitch, Keys DPitch, Keys Roll, Keys Bomb, Keys Gun, Keys Autoland, Keys Particles, bool SpinIndependent);
    private static ControlScheme MyControlScheme = new(Keys.A, Keys.D, Keys.W, Keys.S, Keys.X, Keys.B, Keys.Space, Keys.H, Keys.D0, false);
    private static ControlScheme ClassicControlScheme = new(Keys.Z, Keys.X, Keys.OemComma, Keys.OemQuestion, Keys.OemPeriod, Keys.B, Keys.Space, Keys.H, Keys.D0, true);
    public  static ControlScheme ActiveControlScheme = ClassicControlScheme;

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        UPS.Update(DateTime.UtcNow);

        var inputs = _keyboardCustodian.Process(kc =>
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

            var cs = ActiveControlScheme;

            var throttle =
                (kc.IsKeyDown(cs.LThrottle) && !kc.IsKeyDown(cs.RThrottle)) ? Airplane.ThrottleInput.Reversing :
                (!kc.IsKeyDown(cs.LThrottle) && kc.IsKeyDown(cs.RThrottle)) ? Airplane.ThrottleInput.Throttling :
                Airplane.ThrottleInput.None;

            var pitch =
                (kc.IsKeyDown(cs.UPitch) && !kc.IsKeyDown(cs.DPitch)) ? Airplane.PitchInput.Backward :
                (!kc.IsKeyDown(cs.UPitch) && kc.IsKeyDown(cs.DPitch)) ? Airplane.PitchInput.Forward :
                Airplane.PitchInput.None;

            var roll =
                kc.IsKeyDown(cs.Roll) ? Airplane.RollInput.Roll : Airplane.RollInput.None;

            var bombLaunch =
                kc.IsKeyDown(cs.Bomb) ? Airplane.BombInput.Active : Airplane.BombInput.Inactive;

            var gunFire =
                kc.IsKeyDown(cs.Gun) ? Airplane.GunInput.Active : Airplane.GunInput.Inactive;

            var autoLand =
                kc.IsKeyDown(cs.Autoland) ? Airplane.AutoLandToggle.Active : Airplane.AutoLandToggle.Inactive;

            var emitParticles =
                kc.IsKeyDown(cs.Particles) ? true : false;

            return new
            {
                emitParticles,
                planeInputs = Airplane.Inputs.Clean() with
                {
                    Throttle = throttle,
                    Pitch = pitch,
                    Roll = roll,
                    BombLaunch = bombLaunch,
                    GunFire = gunFire,
                    AutoLand = autoLand,
                }
            };
        });

        _world.Simulate(gameTime, UPS.UPS, inputs.planeInputs, inputs.emitParticles);
    }

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

        FPS.Update(DateTime.UtcNow);

        var full = GraphicsDevice.Viewport;
        var bnd = full.Height - 120;

        var mainViewport = new Viewport(0, 0, full.Width, bnd);
        var dashViewport = new Viewport(0, bnd, 200, 120);
        var gizmoViewport = new Viewport(200, bnd, 120, 120);
        var dialsViewport = new Viewport(320, bnd, 240, 120);
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
            _dashboard.Draw(_world.Plane, UPS, FPS, dashViewport, gameTime);
        });

        DrawInViewport(gizmoViewport, full, () =>
        {
            _gizmo.Draw(_world.Plane, gameTime);
        });

        DrawInViewport(dialsViewport, full, () =>
        {
            TheGame.SpriteBatchPoint.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None,RasterizerState.CullNone, null, Matrix.Identity);
            
            _dialRoundRender.Draw(_world.Plane.SpeedDial, new(60, 60), 36, Color.Green, DialRoundRender.DefaultStyle, gameTime);
            _dialRoundRender.Draw(_world.Plane.AltDial, new(180, 60), 36, Color.Green, DialRoundRender.DefaultStyle, gameTime);

            TheGame.SpriteBatchPoint.End();
        });

        DrawInViewport(minimapViewport, full, () =>
        {
            _worldRender.DrawMinimap(_world, gameTime);
        });
    }
}
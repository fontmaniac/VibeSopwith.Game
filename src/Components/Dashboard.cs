using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class Dashboard(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private SpriteFont _font = null!;

        private UiSystem _uiSystem = null!;
        public Panel _root = null!;

        private Paragraph _alt = null!;
        private Paragraph _spd = null!;
        private Paragraph _lng = null!;
        private Paragraph _ups = null!;
        private Paragraph _bps = null!;
        private Paragraph _tb = null!;
        private Paragraph _heap = null!;

        public new void LoadContent()
        {
            _font = Game.Content.Load<SpriteFont>("Fonts\\Arial16");

            var untexturedStyle = new UntexturedStyle(TheGame.SpriteBatchPoint)
            {
                PanelColor = Color.FromNonPremultiplied(64, 64, 64, 255),
                TextScale = 1.0f,
                Font = new GenericSpriteFont(_font),
            };

            _uiSystem = new UiSystem(this.Game, untexturedStyle);

            _root = new Panel(Anchor.TopLeft, new Vector2(300, 150), new Vector2(2, 2), false, true);
            _uiSystem.Add("Root", _root);

            //_root.AddChild(_alt = new Paragraph(Anchor.AutoLeft, 1, "Altitude"));
            //_root.AddChild(_lng = new Paragraph(Anchor.AutoLeft, 1, "Position"));
            _root.AddChild(_spd = new Paragraph(Anchor.AutoLeft, 1, "Speed"));
            _root.AddChild(_ups = new Paragraph(Anchor.AutoLeft, 1, "FPS"));
            _root.AddChild(_bps = new Paragraph(Anchor.AutoLeft, 1, "BPS"));
            //_root.AddChild(_tb = new Paragraph(Anchor.AutoLeft, 1, "TotalBytes"));
            _root.AddChild(_heap = new Paragraph(Anchor.AutoLeft, 1, "Heap"));
        }

        public new void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _uiSystem.Update(gameTime);
        }

        public void Draw(Airplane plane, UpsCounter ups, UpsCounter fps, MemoryStats ms, Viewport vp, GameTime gameTime)
        {
            _root.PositionOffset = new Vector2(vp.X, vp.Y);
            _root.Size = new Vector2(vp.Width, vp.Height);

            base.Draw(gameTime);

            //_alt.Text = $"Altitude: {(int)plane.Position.Y}";
            //_lng.Text = $"Position: {(int)plane.Position.X}";
            _spd.Text = $"Speed: {plane.Speed:0.000}";
            _ups.Text = $"FPS/UPS: {fps.UPS}/{ups.UPS}";
            _bps.Text = $"BPS: {ms.BytesPerSecond}";
            //_tb.Text  = $"TotalBytes: {ms.TotalBytes}";
            _heap.Text = $"Heap: {ms.HeapSize}";

            _uiSystem.Draw(gameTime, TheGame.SpriteBatchPoint);
        }
    }
}

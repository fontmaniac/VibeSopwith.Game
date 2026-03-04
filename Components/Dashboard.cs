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

        public new void LoadContent()
        {
            _font = Game.Content.Load<SpriteFont>("Fonts\\Arial16");

            var untexturedStyle = new UntexturedStyle(TheGame.SpriteBatch)
            {
                PanelColor = Color.FromNonPremultiplied(64, 64, 64, 255),
                TextScale = 1.0f,
                Font = new GenericSpriteFont(_font),
            };

            _uiSystem = new UiSystem(this.Game, untexturedStyle);

            _root = new Panel(Anchor.TopLeft, new Vector2(200, 100), new Vector2(2, 2), false, true);
            _uiSystem.Add("Root", _root);

            _root.AddChild(_alt = new Paragraph(Anchor.AutoLeft, 1, "Altitude"));
            _root.AddChild(_spd = new Paragraph(Anchor.AutoLeft, 1, "Speed"));
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

        public void Draw(Airplane plane, Viewport vp, GameTime gameTime)
        {
            _root.PositionOffset = new Vector2(vp.X, vp.Y);
            _root.Size = new Vector2(vp.Width, vp.Height);

            base.Draw(gameTime);

            _alt.Text = $"Altitude: {(int)plane.Position.Y}";
            _spd.Text = $"Speed: {plane.Speed:0.000}";

            _uiSystem.Draw(gameTime, TheGame.SpriteBatch);
        }
    }
}

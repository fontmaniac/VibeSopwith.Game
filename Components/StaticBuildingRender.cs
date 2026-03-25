using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class StaticBuildingRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private static IDictionary<StaticBuilding.BuildingType, string> TextureSourceMap = new Dictionary<StaticBuilding.BuildingType, string>()
        {
            { StaticBuilding.BuildingType.Factory, "Textures\\Factory_1.png" },
            { StaticBuilding.BuildingType.Cistern, "Textures\\Cistern_1.png" },
        };

        private Dictionary<StaticBuilding.BuildingType, Texture2D> _textures = new Dictionary<StaticBuilding.BuildingType, Texture2D>();

        public new void LoadContent()
        {
            base.LoadContent();

            foreach (var kvp in TextureSourceMap)
            {
                using var tex = Game.Content.Load<Texture2D>(kvp.Value);
                _textures[kvp.Key] = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex);
            }
        }

        public void Draw(StaticBuilding building, GameTime gameTime)
        {
            var texture = _textures[building.TheType];
            DrawHelper.DrawOriginated(building, texture, new Vector2(64, building.Spin == BasisSpin.Down ? 128 : 0), TheGame.SpriteBatch);
        }
    }
}

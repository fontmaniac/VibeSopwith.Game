using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using VibeSopwith.Game.Core;
using VibeSopwith.Game.Graphics;

namespace VibeSopwith.Game.Components
{
    internal class StaticBuildingRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private static IDictionary<StaticBuilding.BuildingType, (string, string)> TextureSourceMap = new Dictionary<StaticBuilding.BuildingType, (string, string)>()
        {
            { StaticBuilding.BuildingType.Factory, ("Textures\\Factory_1.png", "Textures\\Factory_1_Exploded.png") },
            { StaticBuilding.BuildingType.Cistern, ("Textures\\Cistern_1.png", "Textures\\Cistern_1_Exploded.png") },
        };

        private Dictionary<StaticBuilding.BuildingType, Texture2D> _textures = new Dictionary<StaticBuilding.BuildingType, Texture2D>();
        private Dictionary<StaticBuilding.BuildingType, Texture2D> _texturesExploded = new Dictionary<StaticBuilding.BuildingType, Texture2D>();

        public new void LoadContent()
        {
            base.LoadContent();

            foreach (var kvp in TextureSourceMap)
            {
                using var tex1 = Game.Content.Load<Texture2D>(kvp.Value.Item1);
                _textures[kvp.Key] = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex1);
                using var tex2 = Game.Content.Load<Texture2D>(kvp.Value.Item2);
                _texturesExploded[kvp.Key] = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatch, tex2);
            }
        }

        public void Draw(StaticBuilding building, GameTime gameTime)
        {
            var texture = building.Exploded ? _texturesExploded[building.TheType] : _textures[building.TheType];
            DrawHelper.DrawOriginated(building, texture, new Vector2(64, building.Spin == BasisSpin.Down ? 128 : 0), TheGame.SpriteBatch);
        }
    }
}

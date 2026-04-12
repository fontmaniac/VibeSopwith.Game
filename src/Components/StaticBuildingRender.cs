using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nage.Strata.Graphics;
using VibeSopwith.Game.Core;

namespace VibeSopwith.Game.Components
{
    internal class StaticBuildingRender(Microsoft.Xna.Framework.Game game) : DrawableGameComponent(game)
    {
        private static IDictionary<StaticBuilding.BuildingType, (string, string)> TextureSourceMap = new Dictionary<StaticBuilding.BuildingType, (string, string)>()
        {
            { StaticBuilding.BuildingType.Factory, ("Textures\\Factory_1.png", "Textures\\Factory_1_Exploded.png") },
            { StaticBuilding.BuildingType.Cistern, ("Textures\\Cistern_1.png", "Textures\\Cistern_1_Exploded.png") },
            { StaticBuilding.BuildingType.ArmyBase, ("Textures\\Army_Base_1.png", "Textures\\Army_Base_1_Exploded.png") },
        };

        private Dictionary<StaticBuilding.BuildingType, TextureAtlas> _textures = new Dictionary<StaticBuilding.BuildingType, TextureAtlas>();
        private Dictionary<StaticBuilding.BuildingType, TextureAtlas> _texturesExploded = new Dictionary<StaticBuilding.BuildingType, TextureAtlas>();

        public new void LoadContent()
        {
            base.LoadContent();

            foreach (var kvp in TextureSourceMap)
            {
                _textures[kvp.Key] = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>(kvp.Value.Item1)).ToAtlas(new Vector2(64, 128));
                _texturesExploded[kvp.Key] = MipMap.CastWithMipMaps(GraphicsDevice, TheGame.SpriteBatchPoint, Game.Content.Load<Texture2D>(kvp.Value.Item2)).ToAtlas(new Vector2(64, 128));
            }
        }

        public void Draw(StaticBuilding building, GameTime gameTime)
        {
            var texture = building.Exploded ? _texturesExploded[building.TheType] : _textures[building.TheType];
            DrawHelper.DrawSlice(building, HandedSlice.LR.Wrap(texture.GetSlice()), TheGame.SpriteBatchPoint, null);
        }

        public void DrawSnapped(StaticBuilding building, GameTime gameTime, Vector2 worldPixelSize)
        {
            var texture = building.Exploded ? _texturesExploded[building.TheType] : _textures[building.TheType];
            DrawHelper.DrawSlice(building, HandedSlice.LR.Wrap(texture.GetSlice()), TheGame.SpriteBatchPoint, worldPixelSize);
        }

    }
}

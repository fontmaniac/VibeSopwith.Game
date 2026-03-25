using Microsoft.Xna.Framework;

namespace VibeSopwith.Game.Core
{
    internal class StaticBuilding : ICentered
    {
        public Vector2 Position { get; }
        public Vector2 Direction { get; }
        public BasisSpin Spin { get; }
        public float Length => 4f;
        public float Height => 4f;

        public enum BuildingType { Factory, Cistern };
        public BuildingType TheType { get; }

        public StaticBuilding(BuildingType buildingType, Vector2 position, BasisSpin spin)
        {
            Position = position;
            TheType = buildingType;
            Spin = spin;
            Direction = Vector2.UnitX * (spin == BasisSpin.Down ? +1f : -1f);
        }
    }
}

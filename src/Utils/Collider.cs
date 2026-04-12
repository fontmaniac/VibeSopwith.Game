using nkast.Aether.Physics2D.Dynamics;

namespace VibeSopwith.Game.Utils
{
    internal class Collider<TCat> where TCat : notnull
    {
        private readonly IDictionary<TCat, Category> _catMap = new Dictionary<TCat, Category>();
        private int _nextBit = 1;

        public Category GetCategories(params TCat[] cats) => cats.Aggregate(Category.None, (acc, c) => acc | _catMap[c]);

        public Category GetAll() => Category.All;

        public Category AddCategories(params TCat[] cats)
        {
            foreach (var cat in cats)
            {
                if (!_catMap.ContainsKey(cat))
                {
                    _catMap[cat] = (Category)_nextBit;
                    _nextBit <<= 1;
                }
            }

            return GetCategories(cats);
        }

    }
}

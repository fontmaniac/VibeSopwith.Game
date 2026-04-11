namespace VibeSopwith.Game.Utils
{
    internal class AutoGrowArray<T>
    {
        private T[] _items;
        private int _growChunkSize;
        private int _itemsUsed = 0;

        public ReadOnlySpan<T> ReadOnlyItems => _items.AsSpan(0, _itemsUsed);
        public Span<T> Items => _items.AsSpan(0, _itemsUsed);
        public int Length  => _itemsUsed;

        public AutoGrowArray(int growChunkSize)
        {
            _growChunkSize = Math.Max(16, growChunkSize);
            _items = new T[growChunkSize];
        }

        public void Add(T item) 
        {
            if (_itemsUsed == _items.Length)
                Array.Resize(ref _items, _items.Length + _growChunkSize); 

            _items[_itemsUsed++] = item;
        }

        public void RemoveAt(int index)
        {
            _items[index] = _items[--_itemsUsed];
        }

        public ref T this[int index] => ref _items[index];
    }
}

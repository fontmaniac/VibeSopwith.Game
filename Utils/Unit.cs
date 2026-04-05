namespace VibeSopwith.Game.Utils
{
    // `bool Equal`, `==` & `!=` are provided by compiler for `record struct` as "structural equality"
    public readonly record struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
    {
        public static readonly Unit Value = new Unit();

        bool IEquatable<Unit>.Equals(Unit other) => true;
        int IComparable<Unit>.CompareTo(Unit other) => 0;
        int IComparable.CompareTo(object? obj) => obj is Unit ? 0 : throw new ArgumentException("Object is not a Unit", nameof(obj));

        public override int GetHashCode() => 0;

        public override string ToString() => "()";
    }
}

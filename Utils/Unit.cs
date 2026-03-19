namespace VibeSopwith.Game.Utils
{
    public readonly struct Unit : IEquatable<Unit>
    {
        public static readonly Unit Value = new Unit();

        public override bool Equals(object? obj) => obj is Unit;
        public bool Equals(Unit other) => true;

        public override int GetHashCode() => 0;

        public static bool operator ==(Unit left, Unit right) => true;
        public static bool operator !=(Unit left, Unit right) => false;

        public override string ToString() => "()";
    }
}

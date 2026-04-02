using Microsoft.Xna.Framework;
using System.Diagnostics.Metrics;

namespace VibeSopwith.Game;

public enum BasisSpin { Down, Up }

public interface ILocation
{
    Vector2 Position { get; }   // In world
    Vector2 Direction { get; }
    BasisSpin Spin { get; }
    float Length { get; }       // Measurement along X-axis 
    float Height { get; }       // Measurement along Y-axis 
}

public record Location(Vector2 Position, Vector2 Direction, BasisSpin Spin, float Length, float Height) : ILocation
{
    public static Location OffInterface(ILocation src) => new Location(
        src.Position,
        src.Direction,
        src.Spin,
        src.Length,
        src.Height);
}


public interface ISimulated<TState>
{
    // Populate Aether2D Body properties from instance properties for simulation purposes.
    public void PreSimulationPrepare(TState projected);

    // Populate instance properties from Aether2D Body properties computed at simulation step.
    public void PostSimulationUpdate(TState projected);
}

public interface IDescribeMyself { string WhoAmI { get; } }

public interface ICanDieByBomb<T>
{
    Func<bool> IsExploded { get; }
    Action SetExploded { get; }
    Action<T> RemoveRigging { get; }
    Func<GameTime, Vector2, T> MakeExplosion { get; }
}

public record CanDieByBomb<T>(string WhoAmI, Func<bool> IsExploded, Action SetExploded, Action<T> RemoveRigging, Func<GameTime, Vector2, T> MakeExplosion) : ICanDieByBomb<T>, IDescribeMyself;

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

public record Poppet<T>(Func<T, bool> IsAlive, Action<T> Kill)
{
    public Poppet Embrace(T target) => new Poppet(() => IsAlive(target), () => Kill(target));
}

public record Poppet(Func<bool> IsAlive, Action Kill)
{
    public static Poppet<T> Make<T>(Func<T, bool> isAlive, Action<T> kill) => new Poppet<T>(isAlive, kill);
    public static void Make<T>(out Poppet<T> popout, Func<T, bool> isAlive, Action<T> kill) => popout = new Poppet<T>(isAlive, kill);
    public static void Make<T>(out Poppet<T> popout) => popout = new Poppet<T>(_ => true, _ => { });
}

public interface ICanDieByBomb<T>
{
    Poppet Poppet {  get; }
    Action<T> RefreshRigging { get; }
    Func<GameTime, Vector2, T> MakeExplosion { get; }
}

public record CanDieByBomb<T>(string WhoAmI, Poppet Poppet, Action<T> RefreshRigging, Func<GameTime, Vector2, T> MakeExplosion) : ICanDieByBomb<T>, IDescribeMyself;




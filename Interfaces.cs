using Microsoft.Xna.Framework;

namespace VibeSopwith.Game;

public enum Winding { Clockwise, CounterClockwise }

public interface ICentered
{
    Vector2 Position { get; }   // In world
    Vector2 Direction { get; }
    Winding NormalDown { get; }
    float Length { get; }       // Measurement along X-axis 
    float Height { get; }       // Measurement along Y-axis 
}

public record Centered(
    Vector2 Position,
    Vector2 Direction,
    Winding NormalDown,
    float Length,
    float Height) : ICentered
{
    public static Centered OffInterface(ICentered src) => new Centered(
        src.Position,
        src.Direction,
        src.NormalDown,
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



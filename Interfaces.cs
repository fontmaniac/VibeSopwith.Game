using Microsoft.Xna.Framework;

namespace VibeSopwith;

public enum Winding { Clockwise, CounterClockwise }

public interface ICentered
{
    Vector2 Position { get; }   // In world

    Vector2 Direction { get; }

    Winding NormalDown { get; }

    float Length {  get; }  // Measurement along X-axis 
    float Height { get; }     // Measurement along Y-axis 

}

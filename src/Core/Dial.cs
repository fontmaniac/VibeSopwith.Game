namespace VibeSopwith.Game.Core
{
    internal record Dial(string Name, float MinVal, float MaxVal, int SubMarks, float[] Marks, Func<float> GetVal);
}

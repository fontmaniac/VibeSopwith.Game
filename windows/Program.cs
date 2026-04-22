namespace VibeSopwith.Game;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("VibeSopwith.Game started");

        using (TheGame game = new TheGame())
        {
            game.Run();
            Console.WriteLine("VibeSopwith.Game exited");
        }
    }
}
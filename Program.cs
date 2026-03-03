using Microsoft.Xna.Framework;

namespace VibeSopwith
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("VibeSopwith started");

            using (TheGame game = new TheGame())
            {
                game.Run();
                Console.WriteLine("VibeSopwith exited");
            }
        }
    }
}
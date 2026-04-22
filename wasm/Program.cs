using System.Runtime.InteropServices.JavaScript;

public static partial class Program
{
    internal static void Main()
    {
        Environment.SetEnvironmentVariable("FNA_PLATFORM_BACKEND", "SDL2");

        Console.WriteLine("VibeSopwith.Game starting...");
        SetMainLoop(MainLoop);
    }

    private static bool _firstRun = true;
    private static VibeSopwith.Game.TheGame _myGame = null!;

    [JSExport]
    internal static void OnUserInteraction()
    {
        if (_myGame != null)
            _myGame.OnAudioAllowedToInit();
    }

    [JSExport]
    private static void MainLoop()
    {
        try
        {
            if (_firstRun)
            {
                Console.WriteLine("First run of the main loop");
                _firstRun = false;

                _myGame = new VibeSopwith.Game.TheGame(); 
            }

            if (_myGame != null)
                _myGame.RunOneFrame();
        }
        catch (Exception e)
        {
            Console.WriteLine("Unhandled exception in MainLoop:");
            Console.Error.WriteLine(e);
            throw;
        }
    }

    [JSImport("setMainLoop", "main.js")]
    internal static partial void SetMainLoop([JSMarshalAs<JSType.Function>] Action cb);
}
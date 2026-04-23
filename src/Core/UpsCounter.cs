namespace VibeSopwith.Game.Core;

public class UpsCounter
{
    private int _frameCounter = 0;
    private DateTime _startTime = DateTime.UtcNow;

    public int UPS { get; private set; }

    public void Update(DateTime utcTime)
    {
        var elapsedTime = (utcTime - _startTime).TotalSeconds;
        _frameCounter++;

        if (elapsedTime >= 1.0)
        {
            UPS = (int)((double)_frameCounter / elapsedTime);
            _frameCounter = 0;
            _startTime = DateTime.UtcNow;
        }
    }
}

namespace VibeSopwith.Game.Core;

public class MemoryStats
{
    private long _bytesCounter = 0L;
    private DateTime _startTime = DateTime.UtcNow;

    public long BytesPerSecond { get; private set; }
    public long TotalBytes { get; private set; }
    public long HeapSize { get; private set; }


    public void Update(DateTime utcTime)
    {
        var nowTotalBytes = GC.GetTotalAllocatedBytes(false);
        _bytesCounter += nowTotalBytes - TotalBytes;
        TotalBytes = nowTotalBytes;

        var elapsedTime = (utcTime - _startTime).TotalSeconds;

        if (elapsedTime >= 1.0)
        {
            HeapSize = GC.GetTotalMemory(false);
            BytesPerSecond = (int)((double)_bytesCounter / elapsedTime);
            _bytesCounter = 0;
            _startTime = DateTime.UtcNow;
        }
    }
}

using Oz.SimulationLib.Contracts;
using System.Diagnostics;

namespace Oz.SimulationLib.Default;

public class TimeManager : ITime
{
    private bool _initialized;
    private readonly Stopwatch _stopwatch = new();
    private double _previousElapsed;
    
    public ulong FrameCount { get; private set; }
    
    public double DeltaTime { get; private set; }
    public double Time { get; private set; }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        
        _stopwatch.Start();
        _previousElapsed = 0.0;
        DeltaTime = 0.0;
        Time = 0.0;
        FrameCount = 0UL;
        _initialized = true;
    }

    public void Frame()
    {
        var totalSeconds = _stopwatch.Elapsed.TotalSeconds;
        DeltaTime = totalSeconds - _previousElapsed;
        Time += DeltaTime;
        FrameCount++;
        _previousElapsed = totalSeconds;
        
        if (FrameCount > (ulong.MaxValue - 10UL))
        {
            FrameCount = 0UL;
        }
    }
}
namespace Oz.SimulationLib.Contracts;

/// <summary>
///     Object that track virtual time
/// </summary>
public interface ITime
{
    /// <summary>
    ///     Time that passed from previous frame (in seconds)
    /// </summary>
    double DeltaTime { get; }

    /// <summary>
    ///     Time that passed from initialize call (in seconds)
    /// </summary>
    double Time { get; }

    /// <summary>
    ///     Initialize time object
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Create time frame, updates values of Time and DeltaTime properties
    /// </summary>
    void Frame();
    
    /// <summary>
    /// Get frame count from Initialize() call
    /// </summary>
    ulong FrameCount { get; }
}
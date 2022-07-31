using Microsoft.Extensions.Logging;
using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default;

public class Simulator : ISimulator
{
    private const double NormalFrameLength = 0.016666667;
    private readonly ITime _time;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isSimulationRunning;
    private Task? _simulationTask;

    public Simulator(ITime time, IMessageChannel messageChannel, ILoggerFactory loggerFactory)
    {
        _time = time;
        _time.Initialize();
        Context = new SimContext(_time, messageChannel, this);
        World = new SimWorld(Context, Guid.NewGuid(), "World", loggerFactory);
    }

    public bool IsPrepared { get; private set; }

    public async Task PrepareSimulationAsync()
    {
        switch (IsPrepared)
        {
            case true:
                throw new InvalidOperationException($"{nameof(PrepareSimulationAsync)} can be called only once");
            case false:

                World.Reset();
                _cancellationTokenSource = new CancellationTokenSource();
                await World.InitializeAsync();
                IsPrepared = true;
                break;
        }
    }

    public Task<Task> StartSimulationAsync()
    {
        if (!IsPrepared)
        {
            throw new InvalidOperationException(
                $"{nameof(StartSimulationAsync)} can be called only on prepared simulation. Call {nameof(PrepareSimulationAsync)} before");
        }

        if (_cancellationTokenSource == null)
        {
            throw new InvalidOperationException(
                $"{nameof(_cancellationTokenSource)} can't be null when simulation starts");
        }

        var token = _cancellationTokenSource.Token;

        async Task RenderFunc()
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (World is null)
                    {
                        throw new NullReferenceException(nameof(World));
                    }

                    if (Context is null)
                    {
                        throw new NullReferenceException(nameof(Context));
                    }

                    _time.Frame();
                    await World.UpdateAsync().ConfigureAwait(false);

                    if (_time.DeltaTime < NormalFrameLength)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(NormalFrameLength - _time.DeltaTime), token)
                            .ConfigureAwait(false);
                    }
                }
            } catch(Exception exception) {}
        }

        var task = Task.Factory.StartNew(RenderFunc, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        _simulationTask = task.Result;
        _isSimulationRunning = true;
        return task;
    }

    public async Task FinishSimulationAsync()
    {
        if (!IsPrepared)
        {
            return;
        }

        if (!_isSimulationRunning)
        {
            return;
        }

        if (_cancellationTokenSource == null)
        {
            return;
        }

        if (_simulationTask == null)
        {
            return;
        }

        await World.DestroyAllLevelsAsync().ConfigureAwait(false);
        _cancellationTokenSource.Cancel();
        try
        {
            await _simulationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }

        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
        _isSimulationRunning = false;
        IsPrepared = false;
    }

    public ISimContext Context { get; }
    public ISimWorld World { get; private set; }
}
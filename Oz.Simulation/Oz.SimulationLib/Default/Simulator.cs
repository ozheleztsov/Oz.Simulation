using Oz.SimulationLib.Contracts;

namespace Oz.SimulationLib.Default;

public class Simulator : ISimulator
{
    private readonly ITime _time;
    private readonly IMessageChannel _messageChannel;
    private Task? _simulationTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isSimulationRunning;
    private bool _isPrepared;
    
    public Simulator(ITime time, IMessageChannel messageChannel)
    {
        _time = time;
        _messageChannel = messageChannel;
        _time.Initialize();
    }

    public async Task PrepareSimulationAsync()
    {
        switch (_isPrepared)
        {
            case true:
                throw new InvalidOperationException($"{nameof(PrepareSimulationAsync)} can be called only once");
            case false:
                Context = new SimContext(_time, _messageChannel, this);
                World = new SimWorld(Guid.NewGuid(), "World", this);
                _cancellationTokenSource = new CancellationTokenSource();
                await World.InitializeAsync(Context);
                _isPrepared = true;
                break;
        }
    }

    public Task StartSimulationAsync()
    {
        if (!_isPrepared)
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
                await World.UpdateAsync(Context).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(0.016666667), token).ConfigureAwait(false);
            }
        }

        var task = Task.Factory.StartNew(RenderFunc, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        _simulationTask = task.Result;
        _isSimulationRunning = true;
        return _simulationTask;
    }

    public async Task FinishSimulationAsync()
    {
        if (!_isPrepared)
        {
            throw new InvalidOperationException(
                $"{nameof(FinishSimulationAsync)} can be called only on prepared simulation");
        }
        if (!_isSimulationRunning)
        {
            throw new InvalidOperationException($"{nameof(FinishSimulationAsync)} can be called on running simulation");
        }

        if (World == null)
        {
            throw new InvalidOperationException($"{nameof(World)} must be not null when simulation finishes");
        }

        if (Context == null)
        {
            throw new InvalidOperationException($"{nameof(Context)} must be not null when simulation finishes");
        }
        
        if (_cancellationTokenSource == null)
        {
            throw new InvalidOperationException(
                $"{nameof(_cancellationTokenSource)} cant be null when simulation finishes");
        }

        if (_simulationTask == null)
        {
            throw new InvalidOperationException($"{nameof(_simulationTask)} cant be null when simulation finishes");
        }

        if (_simulationTask.Status != TaskStatus.Running)
        {
            throw new InvalidOperationException(
                $"{nameof(_simulationTask)} should be running when simulation finishes");
        }
        
        await World.DestroyAsync(Context).ConfigureAwait(false);
        _cancellationTokenSource.Cancel();
        try
        {
            await _simulationTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        { }

        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
        Context = null;
        World = null;
        _isSimulationRunning = false;
        _isPrepared = false;
    }

    public ISimContext? Context { get; private set; }
    public ISimWorld? World { get; private set; }
}
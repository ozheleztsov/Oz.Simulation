using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Oz.Snake.Client.Contracts;
using Oz.Snake.Client.Settings;
using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Client.Services;

public class CommunicationService : ICommunicationService
{
    private readonly IOutputService _outputService;
    private readonly IOptions<SnakeSettings> _snakeSettings;
    private readonly HubConnection _connection;

    public CommunicationService(ISnakeBoardService snakeBoardService, IOutputService outputService, IOptions<SnakeSettings> snakeSettings)
    {
        _outputService = outputService;
        _snakeSettings = snakeSettings;
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5205/snake")
            .Build();

        _connection.Closed += OnClosedAsync;
        _connection.Reconnected += OnReconnected;
        _connection.Reconnecting += OnReconnecting;
        _connection.On<SnakeBoardDto>("UpdateBoard", snakeBoard =>
        {
            snakeBoardService.UpdateBoard(snakeBoard);
            if (snakeBoardService.Board is not null)
            {
                _outputService.DrawSnakeBoard(snakeBoardService.Board);
            }
        });
    }

    private Task OnReconnecting(Exception? arg)
    {
        _outputService.DrawMessage(arg != null ? $"OnReconnecting: {arg.Message}" : "OnReconnecting: exception is null");
        return Task.CompletedTask;
    }

    private Task OnReconnected(string? arg)
    {
        _outputService.DrawMessage($"OnReconnected: {arg}");
        return Task.CompletedTask;
    }

    private async Task OnClosedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _outputService.DrawMessage(exception.Message);
            _outputService.DrawMessage(exception.StackTrace ?? string.Empty);
        }

        await Task.Delay(Random.Shared.Next(0, 5000));
        await _connection.StartAsync();
    }
    

    public async Task JoinGame(CancellationToken cancellationToken)
    {
        try
        {
            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync(cancellationToken);
                await _connection.InvokeAsync("Join", _snakeSettings.Value.ClientName,
                    cancellationToken: cancellationToken);
                
                _ = Task.Run(async () =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    }

                    if (_connection.State == HubConnectionState.Connected)
                    {
                        await _connection.StopAsync(default);
                    }
                });
            }
        }
        catch (Exception exception)
        {
            _outputService.DrawMessage(exception.Message);
            _outputService.DrawMessage(exception.StackTrace ?? string.Empty);
        }
    }
    
    

    public async Task Move(Direction direction, CancellationToken cancellationToken)
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            await _connection.InvokeAsync("MoveSnake",
                new MoveSnakeRequestDto(direction, _snakeSettings.Value.ClientName!), cancellationToken: cancellationToken);
        }
        else
        {
            _outputService.DrawMessage($"{nameof(Move)}: connection in wrong state: {_connection.State}");
        }
    }
}
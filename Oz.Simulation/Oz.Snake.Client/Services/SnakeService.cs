using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Oz.Snake.Client.Contracts;
using Oz.Snake.Client.Settings;
using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Client.Services;

public class SnakeService : ISnakeService
{
    private readonly IOptions<SnakeSettings> _snakeSettings;
    private readonly HubConnection _connection;

    public SnakeService(IOptions<SnakeSettings> snakeSettings)
    {
        _snakeSettings = snakeSettings;
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5205/snake")
            .Build();

        _connection.Closed += OnClosedAsync;
        _connection.Reconnected += OnReconnected;
        _connection.Reconnecting += OnReconnecting;
        _connection.On<SnakeBoardDto>("UpdateBoard", snakeBoard =>
        {
            Console.WriteLine(snakeBoard.ToString());
        });
    }

    private Task OnReconnecting(Exception? arg)
    {
        Console.WriteLine(arg != null ? $"OnReconnecting: {arg.Message}" : "OnReconnecting: exception is null");
        return Task.CompletedTask;
    }

    private Task OnReconnected(string? arg)
    {
        Console.WriteLine($"OnReconnected: {arg}");
        return Task.CompletedTask;
    }

    private async Task OnClosedAsync(Exception? exception)
    {
        if (exception != null)
        {
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
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
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
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
            Console.WriteLine($"{nameof(Move)}: connection in wrong state: {_connection.State}");
        }
    }
}
using Microsoft.AspNetCore.SignalR.Client;
using Oz.Snake.Client.Contracts;

namespace Oz.Snake.Client.Services;

public class SnakeService : ISnakeService
{
    private readonly HubConnection _connection;

    public SnakeService()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5205/snake")
            .Build();

        _connection.Closed += OnClosedAsync;
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

    public async Task JoinGame()
    {
        try
        {
            await _connection.InvokeAsync("Join", "Oleg" + Random.Shared.Next());
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
        }
    }
}
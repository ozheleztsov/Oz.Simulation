using Microsoft.AspNetCore.SignalR;
using Oz.Snake.Common.Dtos;
using Oz.Snake.Contracts;
using Oz.Snake.Exceptions;
using Oz.Snake.Services;

namespace Oz.Snake.Hubs;

public class SnakeHub : Hub<ISnakeClient>
{
    private readonly ISnakeService _snakeService;

    public SnakeHub(ISnakeService snakeService) =>
        _snakeService = snakeService;

    [HubMethodName(nameof(MoveSnake))]
    public async Task MoveSnake(MoveSnakeRequestDto request)
    {
        _snakeService.MoveSnake(request.Name, request.Direction);
        if (_snakeService.Board != null)
        {
            var snakeAdapter = new SnakeAdapter();
            await Clients.All.UpdateBoard(snakeAdapter.From(_snakeService.Board));
        }   
    }

    [HubMethodName(nameof(Join))]
    public async Task Join(string name)
    {
        try
        {
            _snakeService.AddSnakeOnFreeCell(name);
            await Clients.Caller.OnJoinStatus(new JoinStatusResponseDto(JoinStatus.Success, string.Empty));

            if (_snakeService.Board != null)
            {
                var snakeAdapter = new SnakeAdapter();
                await Clients.Caller.UpdateBoard(snakeAdapter.From(_snakeService.Board));
            }
        }
        catch (SnakeException exception)
        {
            await Clients.Caller.OnJoinStatus(new JoinStatusResponseDto(JoinStatus.Fail, exception.Message));
        }
        catch (Exception exception)
        {
            throw new HubException(exception.Message);
        }
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Players");
        if (!_snakeService.IsInitialized)
        {
            var (width, height) = GetWidthAndHeight();
            _snakeService.InitializeBoard(width, height);
        }

        await base.OnConnectedAsync();
    }

    private (int Width, int Height) GetWidthAndHeight()
    {
        var width = 16;
        var height = 16;
        var name = string.Empty;

        if (Context.Items.ContainsKey("Width") && Context.Items["Width"] != null)
        {
            width = (int)Context.Items["Width"];
        }

        if (Context.Items.ContainsKey("Height") && Context.Items["Height"] != null)
        {
            height = (int)Context.Items["Height"];
        }

        return (width, height);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Players");
        await base.OnDisconnectedAsync(exception);
    }
}
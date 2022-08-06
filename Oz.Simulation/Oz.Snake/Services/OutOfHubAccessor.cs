using Microsoft.AspNetCore.SignalR;
using Oz.Snake.Contracts;
using Oz.Snake.Hubs;

namespace Oz.Snake.Services;

public class OutOfHubAccessor : IOutOfHubAccessor
{
    private readonly ISnakeService _snakeService;
    private readonly IHubContext<SnakeHub, ISnakeClient> _hubContext;
    private readonly ILogger<OutOfHubAccessor> _logger;

    public OutOfHubAccessor(ISnakeService snakeService, IHubContext<SnakeHub, ISnakeClient> hubContext,
        ILogger<OutOfHubAccessor> logger)
    {
        _snakeService = snakeService;
        _hubContext = hubContext;
        _logger = logger;
    }
    
    public async Task SendBoardUpdateAsync()
    {
        if (!_snakeService.IsInitialized)
        {
            return;
        }
        var board = _snakeService.Board;
        if (board is null)
        {
            return;
        }
        
        var adapter = new SnakeAdapter();
        var boardDto = adapter.From(board);
        await _hubContext.Clients.All.UpdateBoard(boardDto);
    }
}
using Oz.Snake.Client.Contracts;
using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Client.Services;

public class KeyboardService : IKeyboardService
{
    private readonly ISnakeService _snakeService;
    private Task _listeningTask;

    public KeyboardService(ISnakeService snakeService) =>
        _snakeService = snakeService;

    public async Task StartListening(Action escapeAction, CancellationToken cancellationToken)
    {
        bool isEscape = false;
        while (!isEscape)
        {
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                {
                    await _snakeService.Move(Direction.Up, cancellationToken);
                }
                    break;
                case ConsoleKey.DownArrow:
                {
                    await _snakeService.Move(Direction.Down, cancellationToken);
                }
                    break;
                case ConsoleKey.LeftArrow:
                {
                    await _snakeService.Move(Direction.Left, cancellationToken);
                }
                    break;
                case ConsoleKey.RightArrow:
                {
                    await _snakeService.Move(Direction.Right, cancellationToken);
                }
                    break;
                case ConsoleKey.Escape:
                {
                    escapeAction.Invoke();
                    isEscape = true;
                }
                    break;
            }
        }
    }

}
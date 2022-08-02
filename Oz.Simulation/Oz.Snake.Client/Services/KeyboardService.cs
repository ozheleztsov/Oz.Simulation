using Oz.Snake.Client.Contracts;
using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Client.Services;

public class KeyboardService : IKeyboardService
{
    private readonly ICommunicationService _communicationService;
    private Task _listeningTask;

    public KeyboardService(ICommunicationService communicationService) =>
        _communicationService = communicationService;

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
                    await _communicationService.Move(Direction.Up, cancellationToken);
                }
                    break;
                case ConsoleKey.DownArrow:
                {
                    await _communicationService.Move(Direction.Down, cancellationToken);
                }
                    break;
                case ConsoleKey.LeftArrow:
                {
                    await _communicationService.Move(Direction.Left, cancellationToken);
                }
                    break;
                case ConsoleKey.RightArrow:
                {
                    await _communicationService.Move(Direction.Right, cancellationToken);
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
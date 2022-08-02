using Oz.Snake.Client.Contracts;
using Oz.Snake.Client.Models;

namespace Oz.Snake.Client.Services;

public class ConsoleOutputService : IOutputService
{
    public void DrawMessage(string message) =>
        throw new NotImplementedException();

    private const int XOffset = 1;
    private const int YOffset = 1;
    
    public void DrawSnakeBoard(SnakeBoard snakeBoard)
    {
        Console.
        Console.SetCursorPosition(XOffset, YOffset);
        for (int i = 0; i < snakeBoard.Height; i++)
        {
            for (int j = 0; j < snakeBoard.Width; j++)
            {
                
            }
        }    
    }
}
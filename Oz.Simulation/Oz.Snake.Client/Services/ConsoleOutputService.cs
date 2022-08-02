using Oz.Snake.Client.Contracts;
using Oz.Snake.Client.Models;
using Oz.Snake.Common.Models;
using System.Text;

namespace Oz.Snake.Client.Services;

public class ConsoleOutputService : IOutputService
{

    public ConsoleOutputService()
    {
        Console.OutputEncoding = Encoding.UTF8;
    }

    public void DrawMessage(string message)
    {
        
    }

    private const int XOffset = 1;
    private const int YOffset = 1;
    
    public void DrawSnakeBoard(SnakeBoard snakeBoard)
    {
        Console.Clear();
        Console.CursorVisible = false;
        for (int i = 0; i < snakeBoard.Height; i++)
        {
            for (int j = 0; j < snakeBoard.Width; j++)
            {
                Console.SetCursorPosition(XOffset + j, YOffset + i);
                var cell = snakeBoard[i, j];
                switch (cell.State)
                {
                    case CellState.Empty:
                    {
                        Console.Write(" ");
                    }
                        break;
                    case CellState.Food:
                    {
                        Console.Write("O");
                    }
                        break;
                    case CellState.Snake:
                    {
                        Console.Write('#');
                        Console.SetCursorPosition(0, YOffset + snakeBoard.Height + 1);
                        Console.Write($"{cell.Position.X}, {cell.Position.Y}");
                    }
                        break;
                }
            }
        }
        Console.CursorVisible = true;
    }
}
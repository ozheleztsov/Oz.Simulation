using Oz.Snake.Client.Contracts;
using Oz.Snake.Client.Models;
using Oz.Snake.Common.Models;
using System.Text;

namespace Oz.Snake.Client.Services;

public class ConsoleOutputService : IOutputService
{
    private const int XOffset = 1;
    private const int YOffset = 1;
    private const int MaxMessagesCount = 10;

    public ConsoleOutputService() =>
        Console.OutputEncoding = Encoding.UTF8;

    public void DrawMessage(string message)
    {
        AddMessage(message);
        RedrawAll();
    }

    private void AddMessage(string message)
    {
        if (_messages.Count > MaxMessagesCount)
        {
            while (_messages.Count > MaxMessagesCount - 1)
            {
                _messages.RemoveAt(0);
            }
        }
        _messages.Add(message);
    }

    private readonly List<string> _messages = new();
    private SnakeBoard? _cachedSnakeBoard = null;

    public void DrawSnakeBoard(SnakeBoard snakeBoard)
    {
        _cachedSnakeBoard = snakeBoard;
        RedrawAll();
    }

    private void RedrawAll()
    {
        Console.Clear();
        Console.CursorVisible = false;
        RedrawSnakeBoard();
        RedrawStatisticsInformation();
        RedrawMessages();
        Console.CursorVisible = true;
    }

    private void RedrawStatisticsInformation()
    {
        if (_cachedSnakeBoard is null)
        {
            return;
        }

        var verticalOffset = 0;
        Console.SetCursorPosition(XOffset + _cachedSnakeBoard.Width + 2, YOffset);
        foreach (var snake in _cachedSnakeBoard.Snakes)
        {
            Console.Write($"Snake: {snake.Name} last time moved: {snake.LastTimeMoved}");
            verticalOffset++;
            Console.SetCursorPosition(XOffset + _cachedSnakeBoard.Width + 2, YOffset + verticalOffset);
        }
    }

    private void RedrawMessages()
    {
        Console.SetCursorPosition(XOffset, YOffset + (_cachedSnakeBoard?.Height ?? 0) + 1);
        int index = _messages.Count - 1;
        while (Console.CursorTop < Console.WindowHeight)
        {
            if (index < 0)
            {
                break;
            }

            string message = _messages[index];
            index--;
            Console.WriteLine(message);
            Console.CursorLeft = XOffset;
        }
    }

    private void RedrawSnakeBoard()
    {
        if (_cachedSnakeBoard is null)
        {
            return;
        }
        for (var i = 0; i < _cachedSnakeBoard.Height; i++)
        {
            for (var j = 0; j < _cachedSnakeBoard.Width; j++)
            {
                Console.SetCursorPosition(XOffset + j, YOffset + i);
                var cell = _cachedSnakeBoard[i, j];
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
                    }
                        break;
                }
            }
        }
    }
}
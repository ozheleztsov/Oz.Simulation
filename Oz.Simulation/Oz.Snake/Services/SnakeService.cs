using Oz.Snake.Common.Dtos;
using Oz.Snake.Exceptions;
using Oz.Snake.Models;

namespace Oz.Snake.Services;

public interface ISnakeService
{
    void InitializeBoard(int width, int height);
    void AddSnakeOnFreeCell(string name);
    void MoveSnake(string name, Direction direction);
    bool IsInitialized { get; }
    SnakeBoard? Board { get; }
}

public class SnakeService : ISnakeService
{
    private SnakeBoard? _snakeBoard;

    private int _initialized;

    public SnakeBoard? Board => _snakeBoard;

    public void InitializeBoard(int width, int height)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 0)
        {
            _snakeBoard = new SnakeBoard(width, height);
        }
    }

    public void AddSnakeOnFreeCell(string name)
    {
        if (_snakeBoard is null)
        {
            throw new SnakeException("Snake board is null");
        }

        if (_snakeBoard.IsNameExists(name))
        {
            throw new SnakeException($"Name {name} already exists");
        }

        var freeCell = _snakeBoard.GetFreeCell();
        if (freeCell is null)
        {
            throw new SnakeException("No free cells");
        }
        _snakeBoard.AddSnake(name, freeCell.Position);
    }

    public void MoveSnake(string name, Direction direction)
    {
        if (_snakeBoard is null)
        {
            throw new InvalidOperationException($"{nameof(MoveSnake)}: board is null");
        }
        var snake = _snakeBoard.Snakes.FirstOrDefault(x => x.Name == name);
        if (snake is null)
        {
            throw new InvalidOperationException($"{nameof(MoveSnake)}: no snake with name {name}");
        }
        snake.MoveTo(direction);
    }

    public bool IsInitialized => _initialized != 0;
}
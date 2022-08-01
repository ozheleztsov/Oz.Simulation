using Oz.Snake.Exceptions;
using Oz.Snake.Models;

namespace Oz.Snake.Services;

public interface ISnakeService
{
    void InitializeBoard(int width, int height);
    void AddSnakeOnFreeCell(string name);
    
    bool IsInitialized { get; }
}

public class SnakeService : ISnakeService
{
    private SnakeBoard? _snakeBoard;

    private int _initialized;


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
        _snakeBoard.AddSnake(name, freeCell.X, freeCell.Y);
    }

    public bool IsInitialized => _initialized != 0;
}
using Oz.Snake.Common.Dtos;
using Oz.Snake.Common.Models;
using Oz.Snake.Exceptions;

namespace Oz.Snake.Models;

public class SnakeBoard
{
    private readonly SnakeCell[,] _board;

    public SnakeBoard(int width, int height)
    {
        Width = width;
        Height = height;
        _board = new SnakeCell[height, width];

        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                _board[i, j] = new SnakeCell(j, i);
            }
        }
    }

    public SnakeCell this[int i, int j] => _board[i, j];

    public int Width { get; }
    public int Height { get; }

    public List<Snake> Snakes { get; } = new();

    public void AddSnake(string name, Position position)
    {
        var cell = GetCell(position);
        if (cell.State != CellState.Empty)
        {
            throw new SnakeException($"Cell {position.X}, {position.Y} is not empty");
        }

        cell.State = CellState.Snake;
        var snake = new Snake(this, name, position);
        Snakes.Add(snake);
    }

    public SnakeCell? GetFreeCell()
    {
        List<SnakeCell> freeCells = new();
        for (var i = 0; i < Height; i++)
        {
            for (var j = 0; j < Width; j++)
            {
                if (_board[i, j].State == CellState.Empty)
                {
                    freeCells.Add(_board[i, j]);
                }
            }
        }

        return !freeCells.Any() ? null : freeCells[Random.Shared.Next(0, freeCells.Count)];
    }

    private SnakeCell GetCell(Position position) =>
        _board[position.Y, position.X];

    public bool IsNameExists(string name) => Snakes.Select(x => x.Name).Contains(name);

    public bool IsAllowedPosition(int x, int y) =>
        x >= 0 && x < Width && y >= 0 && y < Height;

    public void FreeCell(int x, int y) =>
        _board[y, x].State = CellState.Empty;

    public void SetState(CellState state, int x, int y) =>
        _board[y, x].State = state;

    public Position? GetPositionInDirection(Direction direction, Position position)
    {
        int x = position.X, y = position.Y;
        switch (direction)
        {
            case Direction.Down:
            {
                y++;
            }
                break;
            case Direction.Up:
            {
                y--;
            }
                break;
            case Direction.Left:
            {
                x--;
            }
                break;
            case Direction.Right:
            {
                x++;
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        return !IsAllowedPosition(x, y) ? null : new Position(x, y);
    }
}


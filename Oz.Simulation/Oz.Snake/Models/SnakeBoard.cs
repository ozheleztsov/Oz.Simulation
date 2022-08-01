using Oz.Snake.Dtos;
using Oz.Snake.Exceptions;

namespace Oz.Snake.Models;

public class SnakeBoard
{
    public int Width { get; }
    public int Height { get; }
    
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

    public List<Snake> Snakes { get; } = new();

    public void AddSnake(string name, int x, int y)
    {
        var cell = GetCell(x, y);
        if (cell.State != CellState.Empty)
        {
            throw new SnakeException($"Cell {x}, {y} is not empty");
        }

        cell.State = CellState.Snake;
        var snake = new Snake(this, name, x, y);
        Snakes.Add(snake);
    }

    public SnakeCell? GetFreeCell()
    {
        List<SnakeCell> freeCells = new();
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                if (_board[i, j].State == CellState.Empty)
                {
                    freeCells.Add(_board[i, j]);
                }
            }
        }

        return !freeCells.Any() ? null : freeCells[Random.Shared.Next(0, freeCells.Count)];
    }

    private SnakeCell GetCell(int x, int y) =>
        _board[y, x];

    public bool IsNameExists(string name) => Snakes.Select(x => x.Name).Contains(name);

    public Position GetPositionInDirection(Direction direction, Position position)
    {
        switch (direction)
        {
            
        }
    }
}

public class SnakeCell
{
    public SnakeCell(int x, int y) =>
        Position = new Position(x, y);

    public Position Position { get; }

    public CellState State { get; set; } = CellState.Empty;
}

public enum CellState
{
    Empty,
    Food,
    Snake
}
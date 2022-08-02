using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Common.Models;

public class SnakeCell
{
    public SnakeCell(int x, int y) =>
        Position = new Position(x, y);

    public SnakeCell(SnakeCell other)
    {
        Position = new Position(X: other.Position.X, Y: other.Position.Y);
        State = other.State;
    }

    public SnakeCell(SnakeCellDto snakeCellDto)
    {
        Position = new Position(snakeCellDto.Position.X, snakeCellDto.Position.Y);
        State = snakeCellDto.State;
    }

    public Position Position { get; }

    public CellState State { get; set; } = CellState.Empty;
}
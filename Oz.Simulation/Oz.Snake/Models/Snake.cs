using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Models;

public sealed class Snake
{
    
    private readonly SnakeBoard _board;
    
    /// <summary>
    /// Name of the snake
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// When position of the snake was updated last time
    /// </summary>
    public DateTime LastTimeMoved { get; private set; }

    
    /// <summary>
    /// Positions of the snake
    /// </summary>
    public List<Position> Positions { get; } = new();

    public Snake(SnakeBoard board, string name, Position position)
    {
        _board = board;
        Name = name; 
        Positions.Add(position);
        LastTimeMoved = DateTime.UtcNow.AddDays(-1);
    }

    public void MoveTo(Direction direction)
    {
        var nextPos = _board.GetPositionInDirection(direction, Positions[0]);
        if (nextPos == null)
        {
            return;
        }

        var lastPos = Positions.Last();
        Positions.Insert(0, nextPos);
        Positions.Remove(lastPos);
        _board.FreeCell(lastPos.X, lastPos.Y);
    }
    
}
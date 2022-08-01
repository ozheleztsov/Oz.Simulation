using Oz.Snake.Dtos;

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

    public Snake(SnakeBoard board, string name, int x, int y)
    {
        _board = board;
        Name = name; 
        Positions.Add(new Position(x, y));
        LastTimeMoved = DateTime.UtcNow.AddDays(-1);
    }

    public void MoveTo(Direction direction)
    {
        
    }
    
}
using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Client.Models;

public class Snake
{
    public string Name { get; }
    public List<Position> Positions { get; } = new();

    public Snake(SnakeDto snakeDto)
    {
        Name = snakeDto.Name;
        Positions.AddRange(snakeDto.Positions.Select(position => new Position(position.X, position.Y)));
    }
}
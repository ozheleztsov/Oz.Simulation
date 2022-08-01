namespace Oz.Snake.Dtos;

public sealed class SnakeDto
{
    public string Name { get; set; } = string.Empty;
    public List<Position> Positions { get; set; } = new();
}

public sealed record Position(int X, int Y);
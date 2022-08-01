namespace Oz.Snake.Dtos;

public sealed class SnakeBoardDto
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<SnakeDto> Snakes { get; set; } = new();
}
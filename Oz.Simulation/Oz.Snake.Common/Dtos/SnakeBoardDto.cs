using Oz.Snake.Common.Models;
using System.Text;

namespace Oz.Snake.Common.Dtos;

public sealed class SnakeBoardDto
{
    public int Width { get; set; }
    public int Height { get; set; }
    public List<SnakeDto> Snakes { get; set; } = new();
    
    public SnakeCell[,] Board { get; set; }

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"Width: {Width}, Height: {Height}");
        stringBuilder.AppendLine("Snakes:");
        foreach (var snake in Snakes)
        {
            stringBuilder.AppendLine(snake.ToString());
        }

        return stringBuilder.ToString();
    }
}
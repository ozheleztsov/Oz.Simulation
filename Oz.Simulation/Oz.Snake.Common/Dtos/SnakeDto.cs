using System.Text;

namespace Oz.Snake.Common.Dtos;

public sealed class SnakeDto
{
    public string Name { get; set; } = string.Empty;
    public List<Position> Positions { get; set; } = new();

    public override string ToString()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine($"Name: {Name}");
        stringBuilder.AppendLine("Positions:");
        foreach (var pos in Positions)
        {
            stringBuilder.AppendLine(pos.ToString());
        }

        return stringBuilder.ToString();
    }
}

public sealed record Position(int X, int Y);
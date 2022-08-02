using Oz.Snake.Common.Models;

namespace Oz.Snake.Common.Dtos;

public class SnakeCellDto
{
    public Position Position { get; set; }
    public CellState State { get; set; }
}
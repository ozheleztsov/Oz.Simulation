using Oz.Snake.Common.Dtos;
using Oz.Snake.Common.Models;

namespace Oz.Snake.Client.Models;

public class SnakeBoard
{
    private readonly SnakeCell[,] _board;

    public SnakeBoard(SnakeBoardDto snakeBoardDto)
    {
        Width = snakeBoardDto.Width;
        Height = snakeBoardDto.Height;
        _board = new SnakeCell[Height, Width];

        for (var i = 0; i < snakeBoardDto.Height; i++)
        {
            for (var j = 0; j < snakeBoardDto.Width; j++)
            {
                _board[i, j] = new SnakeCell(snakeBoardDto.Board[i * snakeBoardDto.Width + j]);
            }
        }

        Snakes.AddRange(snakeBoardDto.Snakes.Select(snake => new Snake(snake)));
    }

    public int Width { get; }
    public int Height { get; }
    public List<Snake> Snakes { get; } = new();

    public SnakeCell this[int i, int j] => _board[i, j];
}
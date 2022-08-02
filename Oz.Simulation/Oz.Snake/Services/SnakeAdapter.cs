using Oz.Snake.Common.Dtos;
using Oz.Snake.Common.Models;
using Oz.Snake.Models;

namespace Oz.Snake.Services;

public class SnakeAdapter
{
    public SnakeBoardDto From(SnakeBoard snakeBoard)
    {
        var result = new SnakeBoardDto
        {
            Width = snakeBoard.Width,
            Height = snakeBoard.Height,
            Snakes = snakeBoard.Snakes.Select(x => new SnakeDto
            {
                Name = x.Name,
                Positions = x.Positions.Select(x => x).ToList()
            }).ToList(),
            Board = new SnakeCell[snakeBoard.Height, snakeBoard.Width]
        };
        for (var i = 0; i < snakeBoard.Height; i++)
        {
            for (var j = 0; j < snakeBoard.Width; j++)
            {
                result.Board[i, j] = new SnakeCell(snakeBoard[i, j]);
            }
        }

        return result;
    }
}
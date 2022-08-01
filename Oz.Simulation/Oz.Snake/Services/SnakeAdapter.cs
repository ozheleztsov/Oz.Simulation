using Oz.Snake.Common.Dtos;
using Oz.Snake.Models;

namespace Oz.Snake.Services;

public class SnakeAdapter
{
    public SnakeBoardDto From(SnakeBoard snakeBoard) =>
        new SnakeBoardDto()
        {
            Width = snakeBoard.Width,
            Height = snakeBoard.Height,
            Snakes = snakeBoard.Snakes.Select(x => new SnakeDto()
            {
                Name = x.Name,
                Positions = x.Positions.Select(x => x).ToList()
            }).ToList()
        };
}
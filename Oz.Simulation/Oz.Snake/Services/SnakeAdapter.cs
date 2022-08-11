using Oz.Snake.Common.Dtos;
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
                Positions = x.Positions.Select(position => position).ToList(),
                LastTimeMoved = x.LastTimeMoved
            }).ToList(),
            Board = new SnakeCellDto[snakeBoard.Height * snakeBoard.Width]
        };
        for (var i = 0; i < snakeBoard.Height; i++)
        {
            for (var j = 0; j < snakeBoard.Width; j++)
            {
                var originalCell = snakeBoard[i, j];
                result.Board[i * snakeBoard.Width + j] = new SnakeCellDto()
                {
                    Position = new Position(originalCell.Position.X, originalCell.Position.Y),
                    State = originalCell.State
                };
            }
        }

        return result;
    }
}
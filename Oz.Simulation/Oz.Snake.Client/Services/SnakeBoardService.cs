using Oz.Snake.Client.Contracts;
using Oz.Snake.Client.Models;
using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Client.Services;

public class SnakeBoardService : ISnakeBoardService
{
    public SnakeBoard Board { get; private set; }

    public void UpdateBoard(SnakeBoardDto snakeBoardDto) =>
        Board = new SnakeBoard(snakeBoardDto);
}
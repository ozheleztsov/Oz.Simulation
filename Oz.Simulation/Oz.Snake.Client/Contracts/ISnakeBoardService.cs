using Oz.Snake.Client.Models;
using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Client.Contracts;

public interface ISnakeBoardService
{
    void UpdateBoard(SnakeBoardDto snakeBoardDto);
    
    SnakeBoard Board { get; }
}
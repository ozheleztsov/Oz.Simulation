using Oz.Snake.Client.Models;

namespace Oz.Snake.Client.Contracts;

public interface IOutputService
{
    void DrawMessage(string message);
    void DrawSnakeBoard(SnakeBoard snakeBoard);
}
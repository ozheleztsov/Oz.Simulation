namespace Oz.Snake.Common.Dtos;

public sealed record MoveSnakeRequestDto(Direction Direction, string Name);
public enum Direction
{
    Up,
    Down,
    Left, 
    Right
}
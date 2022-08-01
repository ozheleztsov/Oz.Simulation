namespace Oz.Snake.Dtos;

public sealed record MoveSnakeRequestDto(Direction Direction, string Name);
public enum Direction
{
    Up,
    Down,
    Left, 
    Right
}
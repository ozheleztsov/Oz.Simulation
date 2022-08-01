using Oz.Snake.Dtos;

namespace Oz.Snake.Contracts;

public interface ISnakeClient
{
    Task UpdateBoard(SnakeBoardDto snakeBoard);
    Task OnJoinStatus(JoinStatusResponseDto statusResponse);
}

public enum JoinStatus
{
    Success,
    Fail
}

public sealed record JoinStatusResponseDto(JoinStatus Status, string Message);
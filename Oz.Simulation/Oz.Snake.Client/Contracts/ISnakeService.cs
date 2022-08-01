﻿using Oz.Snake.Common.Dtos;

namespace Oz.Snake.Client.Contracts;

public interface ISnakeService
{
    Task JoinGame(CancellationToken cancellationToken);
    Task Move(Direction direction, CancellationToken cancellationToken);
}
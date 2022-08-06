using Oz.Snake.Common.Dtos;
using Oz.Snake.Common.Models;
using Oz.Snake.Models;

namespace Oz.Snake.Contracts;

/// <summary>
///     Service to manage snake board
/// </summary>
public interface ISnakeService
{
    /// <summary>
    ///     True if board had been initialized otherwise false
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    ///     Reference to the board, if board was not initialized returns null
    /// </summary>
    SnakeBoard? Board { get; }

    /// <summary>
    ///     If board is not initialized create new one
    /// </summary>
    /// <param name="width">Width of the new board</param>
    /// <param name="height">Height of the new board</param>
    void InitializeBoard(int width, int height);

    /// <summary>
    ///     Add snake of length 1 to the free board cell
    /// </summary>
    /// <param name="name">Name of the new snake</param>
    void AddSnakeOnFreeCell(string name);

    /// <summary>
    ///     Move the snake in direction
    /// </summary>
    /// <param name="name">Name of the snake</param>
    /// <param name="direction">Direction to move</param>
    void MoveSnake(string name, Direction direction);

    /// <summary>
    ///     Get random free cell on board
    /// </summary>
    /// <returns>Free cell or null if free cell is not found</returns>
    SnakeCell? GetRandomFreeCell();

    /// <summary>
    ///     Get count of food cells on the board
    /// </summary>
    /// <returns></returns>
    int GetFoodCellsCount();

    /// <summary>
    ///     Set free cell as food cell
    /// </summary>
    /// <param name="cell">Target cell, should be free</param>
    /// <returns>True is set successful otherwise false</returns>
    bool SetCellAsFood(SnakeCell cell);
}
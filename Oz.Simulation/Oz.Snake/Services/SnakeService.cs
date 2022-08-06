using Oz.Snake.Common.Dtos;
using Oz.Snake.Common.Models;
using Oz.Snake.Contracts;
using Oz.Snake.Exceptions;
using Oz.Snake.Models;

namespace Oz.Snake.Services;

public class SnakeService : ISnakeService
{
    private int _initialized;

    public SnakeBoard? Board { get; private set; }

    public void InitializeBoard(int width, int height)
    {
        //initialize only if board was not initialized previously
        if (Interlocked.Exchange(ref _initialized, 1) == 0)
        {
            Board = new SnakeBoard(width, height);
        }
    }

    public void AddSnakeOnFreeCell(string name)
    {
        if (Board is null)
        {
            throw new SnakeException("Snake board is null");
        }

        // we doesn't allow two snakes with the same name
        if (Board.IsNameExists(name))
        {
            throw new SnakeException($"Name {name} already exists");
        }

        //there should be at least one free cell
        var freeCell = Board.GetFreeCell();
        if (freeCell is null)
        {
            throw new SnakeException("No free cells");
        }

        Board.AddSnake(name, freeCell.Position);
    }

    public void MoveSnake(string name, Direction direction)
    {
        //board should be initialized
        if (Board is null)
        {
            throw new InvalidOperationException($"{nameof(MoveSnake)}: board is null");
        }

        //such snake should exists
        var snake = Board.Snakes.FirstOrDefault(x => x.Name == name);
        if (snake is null)
        {
            throw new InvalidOperationException($"{nameof(MoveSnake)}: no snake with name {name}");
        }

        snake.MoveTo(direction);
    }

    public SnakeCell? GetRandomFreeCell()
    {
        if (!IsInitialized)
        {
            return null;
        }

        if (Board is null)
        {
            return null;
        }

        var snakeCells = Board.Where(cell => cell.State == CellState.Empty).ToList();

        return !snakeCells.Any() ? null : snakeCells[Random.Shared.Next(0, snakeCells.Count)];
    }

    public int GetFoodCellsCount()
    {
        if (!IsInitialized)
        {
            return 0;
        }

        return Board?.Count(cell => cell.State == CellState.Food) ?? 0;
    }

    public bool SetCellAsFood(SnakeCell cell)
    {
        if (!IsInitialized)
        {
            return false;
        }

        if (Board is null)
        {
            return false;
        }

        return Board.SetStateIfPredicate(CellState.Food, cell.Position.X, cell.Position.Y,
            c => c.State == CellState.Empty);
    }

    public bool IsInitialized => _initialized != 0;
}
using Oz.Snake.Contracts;

namespace Oz.Snake.HostedServices;

public class FoodFiller : BackgroundService
{
    private const int WaitSeconds = 1;
    private const int MaxFoodCells = 6;
    private readonly ILogger<FoodFiller> _logger;

    private readonly ISnakeService _snakeService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public FoodFiller(ISnakeService snakeService, IServiceScopeFactory serviceScopeFactory, ILogger<FoodFiller> logger)
    {
        _snakeService = snakeService;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(WaitSeconds), stoppingToken);

            var foodCellsCount = _snakeService.GetFoodCellsCount();
            if (foodCellsCount >= MaxFoodCells)
            {
                continue;
            }

            var freeCell = _snakeService.GetRandomFreeCell();
            if (freeCell is null)
            {
                continue;
            }

            if (!_snakeService.SetCellAsFood(freeCell))
            {
                continue;
            }

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var outOfHubAccessor = scope.ServiceProvider.GetService<IOutOfHubAccessor>();
            if (outOfHubAccessor is null)
            {
                continue;
            }
            await outOfHubAccessor.SendBoardUpdateAsync();
            _logger.LogInformation("Cell {X},{Y} set as food", freeCell.Position.X, freeCell.Position.Y);
        }
    }
}
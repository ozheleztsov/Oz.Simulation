namespace Oz.Snake.Contracts;

/// <summary>
///     Provides access to hub methods outside of hubs
/// </summary>
public interface IOutOfHubAccessor
{
    /// <summary>
    ///     Send update of the snake board to all clients outside of snake board hub
    /// </summary>
    /// <returns></returns>
    Task SendBoardUpdateAsync();
}
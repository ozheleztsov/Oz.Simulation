namespace Oz.Snake.Client.Contracts;

public interface IKeyboardService
{
    Task StartListening(Action escapeAction, CancellationToken cancellationToken);
}
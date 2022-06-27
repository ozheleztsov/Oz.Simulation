using Oz.Simulation.Client.Contracts.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Oz.Simulation.Client.Services;

public class AsyncService : IAsyncService
{
    public async Task ExecuteOnUiThreadAsync(Action action)
    {
        var dispatcherOperation = Application.Current?.Dispatcher?.InvokeAsync(action);
        if (dispatcherOperation != null)
        {
            await dispatcherOperation;
        }
    }

    public async Task<T?> ExecuteOnUiThreadAsync<T>(Func<T> func)
    {
        var dispatcherOperation = Application.Current?.Dispatcher?.InvokeAsync(func);
        if (dispatcherOperation != null)
        {
            return await dispatcherOperation;
        }

        return default;
    }
}
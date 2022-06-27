using System;
using System.Threading.Tasks;

namespace Oz.Simulation.Client.Contracts.Services;

public interface IAsyncService
{
    Task ExecuteOnUiThreadAsync(Action action);

    Task<T?> ExecuteOnUiThreadAsync<T>(Func<T> func);
}
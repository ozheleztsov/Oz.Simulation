namespace Oz.SimulationLib.Contracts;

public interface ISimComponent : ISimEntity
{
    ISimObject Owner { get; }

    Task TryInitializeAsync();

    Task OnInitializeAsync();

    Task OnUpdateAsync();

    Task OnDestroyAsync();
}
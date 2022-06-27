using System.Windows;

namespace Oz.Simulation.Client.Contracts.Windows;

public interface IWindow
{
    void Show()
    {
        if (this is Window thisWindow)
        {
            thisWindow.Show();
        }
    }
}
using Oz.Simulation.ClientLib.Contracts;
using System.Windows;
using System.Windows.Controls;

namespace Oz.Simulation.Client.Services;

public class WindowService : IWindowService
{
    private Window? _mainWindow;

    public Window? GetMainWindow() =>
        _mainWindow ??= Application.Current.MainWindow;

    public Viewport3D? GetRenderViewport()
    {
        var window = GetMainWindow();
        if (window is null)
        {
            return null;
        }

        return window is not MainWindow mainWindow ? null : mainWindow.MainViewport;
    }
}
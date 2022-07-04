using System.Windows;
using System.Windows.Controls;

namespace Oz.Simulation.ClientLib.Contracts;

public interface IWindowService
{
    Window? GetMainWindow();
    Viewport3D? GetRenderViewport();
}
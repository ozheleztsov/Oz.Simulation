using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.ClientLib.Contracts;
using System.Windows;

namespace Oz.Simulation.Client.Services;

public class DialogService : IDialogService
{
    private readonly IWindowService _windowService;
    private readonly IAsyncService _asyncService;

    public DialogService(IWindowService windowService, IAsyncService asyncService)
    {
        _windowService = windowService;
        _asyncService = asyncService;
    }

    public void ShowErrorDialog(string text) =>
        _asyncService.ExecuteOnUiThreadAsync(() =>
        {
            var mainWindow = _windowService.GetMainWindow();
            if (mainWindow != null)
            {
                MessageBox.Show(mainWindow, text, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(text, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
}
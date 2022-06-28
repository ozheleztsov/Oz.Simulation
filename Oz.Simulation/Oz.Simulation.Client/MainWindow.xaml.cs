using Oz.Simulation.Client.Contracts.Windows;
using Oz.Simulation.Client.SampleWindows;
using Oz.Simulation.Client.ViewModels;
using Oz.Simulation.ClientLib;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.Client;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IMainWindow
{
    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        DataContext = mainWindowViewModel;
    }

    private void PerspectiveCameraMenu_OnClick(object sender, RoutedEventArgs e)
    {
        PerspectiveCameraWindow window = new PerspectiveCameraWindow();
        window.Show();
    }

    private void OrthographicCameraMenu_OnClick(object sender, RoutedEventArgs e)
    {
        new OrthographicCameraWindow().Show();
    }

    private void WireframeMenu_OnClick(object sender, RoutedEventArgs e)
    {
        new WireframeWindow().Show();
    }
}
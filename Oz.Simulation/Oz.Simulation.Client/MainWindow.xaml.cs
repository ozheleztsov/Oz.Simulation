using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.Client.Contracts.Windows;
using Oz.Simulation.Client.Extensions;
using Oz.Simulation.Client.ViewModels;
using Oz.Simulation.ClientLib.Contracts;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.Client;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IMainWindow
{
    private readonly IAsyncService _asyncService;
    private readonly ISimulationViewportService _simulationViewportService;
    private readonly Stopwatch _stopwatch = new();
    private long _frameCounter;
    private Point _from;
    private TimeSpan _previousElapsed;

    public MainWindow(MainWindowViewModel mainWindowViewModel,
        ISimulationViewportService simulationViewportService,
        IAsyncService asyncService)
    {
        _simulationViewportService = simulationViewportService;
        _asyncService = asyncService;
        InitializeComponent();
        DataContext = mainWindowViewModel;
        CompositionTarget.Rendering += OnRender;
    }

    public Viewport3D MainViewport => _mainViewport;

    private void OnRender(object? sender, EventArgs e)
    {
        if (_frameCounter == 0)
        {
            _frameCounter++;
            _stopwatch.Start();
            _previousElapsed = _stopwatch.Elapsed;
        }

        var currentTime = _stopwatch.Elapsed;
        var deltaTime = (currentTime - _previousElapsed).TotalSeconds;
        _previousElapsed = currentTime;
        _frameCounter++;

        if (_frameCounter > int.MaxValue)
        {
            _frameCounter = 0L;
        }

        var frameRate = (long)(_frameCounter / _stopwatch.Elapsed.TotalSeconds);
        if (frameRate > 0)
        {
            _fpsText.Text = $"Elapsed: {_stopwatch.Elapsed}, Frame: {_frameCounter}, FPS: {frameRate}";
        }

        _asyncService.ExecuteOnUiThreadAsync(async () => { await _simulationViewportService.RenderAsync(); });
    }

    private void _mainViewport_OnPreviewKeyDown(object sender, KeyEventArgs e) =>
        _mainCamera.MoveBy(e.Key).RotateBy(e.Key);

    private void _mainViewport_OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        var till = e.GetPosition(sender as IInputElement);
        var dx = till.X - _from.X;
        var dy = till.Y - _from.Y;
        _from = till;
        var distance = (dx * dx) + (dy * dy);
        if (distance <= 0)
        {
            return;
        }

        if (e.MouseDevice.LeftButton is MouseButtonState.Pressed)
        {
            var angle = distance / _mainCamera.FieldOfView % 45;
            _mainCamera.Rotate(new Vector3D(dy, -dx, 0d), angle);
        }
    }
}
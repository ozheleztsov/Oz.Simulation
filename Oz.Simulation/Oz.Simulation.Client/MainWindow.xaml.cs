using Oz.Simulation.Client.Contracts.Services;
using Oz.Simulation.Client.Contracts.Windows;
using Oz.Simulation.Client.SampleWindows;
using Oz.Simulation.Client.ViewModels;
using Oz.Simulation.ClientLib.Contracts;
using Oz.Simulation.ClientLib.Objects;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.Client;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IMainWindow
{
    private readonly ISimulationViewportService _simulationViewportService;
    private readonly IAsyncService _asyncService;
    private readonly Stopwatch _stopwatch = new();
    private long _frameCounter;
    private TimeSpan _previousElapsed;

    public Viewport3D MainViewport => _mainViewport;
    
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

        _asyncService.ExecuteOnUiThreadAsync(async () =>
        {
            await _simulationViewportService.RenderAsync();
        });
    }
}
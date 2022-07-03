using Oz.Simulation.Client.Contracts.Windows;
using Oz.Simulation.Client.SampleWindows;
using Oz.Simulation.Client.ViewModels;
using Oz.Simulation.ClientLib;
using Oz.Simulation.ClientLib.Objects;
using System;
using System.Collections.Generic;
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
    private long _frameCounter = 0;
    private readonly Stopwatch _stopwatch = new();
    private TimeSpan _previousElapsed;
    
    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        DataContext = mainWindowViewModel;
        AddPlanets();
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
        double deltaTime = (currentTime - _previousElapsed).TotalSeconds;
        _previousElapsed = currentTime;
        _frameCounter++;

        if (_frameCounter > int.MaxValue)
        {
            _frameCounter = 0L;
        }

        var frameRate = (long)((double)_frameCounter / _stopwatch.Elapsed.TotalSeconds);
        if (frameRate > 0)
        {
            _fpsText.Text = $"Elapsed: {_stopwatch.Elapsed}, Frame: {_frameCounter}, FPS: {frameRate}";
        }

        _planets.ForEach(p => p.UpdatePosition(deltaTime));
    }

    private readonly List<Planet> _planets = new();

    private void AddPlanets()
    {
        for (int i = 0; i < 9; i++)
        {
            Ellipsoid planetModel = new Ellipsoid()
            {
                XLength = 0.5,
                YLength = 0.5,
                ZLength = 0.5,
                Transform = new TranslateTransform3D(0, 0, 0),
                Material = new DiffuseMaterial(Brushes.Brown)
            };
            _planets.Add(new Planet(planetModel, Math.Sqrt(4 * i * i + 9 * i * i + 2), 0.03 * Random.Shared.NextDouble()));
            _mainViewport.Children.Add(planetModel);
        }
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

public class Planet
{
    private readonly Ellipsoid _model;
    private readonly double _distance;
    private readonly double _angleSpeed;
    private double _currentAngle = 0.0;

    public Planet(Ellipsoid model, double distance, double angleSpeed = 0.01)
    {
        _model = model;
        _distance = distance;
        _angleSpeed = angleSpeed;
        UpdatePosition(0);
    }

    public void UpdatePosition(double deltaTime)
    {
        _currentAngle += deltaTime * _angleSpeed;
        var translateTransform = _model.Transform as TranslateTransform3D;
        var newPosition = GetPosition();
        translateTransform.OffsetX = newPosition.X;
        translateTransform.OffsetY = newPosition.Y;
        translateTransform.OffsetZ = newPosition.Z;

        if (_currentAngle >= 2 * Math.PI)
        {
            _currentAngle -= 2 * Math.PI;
        }

        if (_currentAngle <= -2 * Math.PI)
        {
            _currentAngle += 2 * Math.PI;
        }
    }

    private Vector3D GetPosition() =>
        new Vector3D(_distance * Math.Cos(_currentAngle), 0, _distance * Math.Sin(_currentAngle));
}
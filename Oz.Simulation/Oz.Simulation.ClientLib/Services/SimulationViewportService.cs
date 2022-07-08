using Oz.Simulation.ClientLib.Contracts;
using Oz.Simulation.ClientLib.Objects;
using Oz.Simulation.ClientLib.Tools;
using Oz.SimulationLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib.Services;

public class SimulationViewportService : ISimulationViewportService
{
    private readonly Dictionary<Guid, PlanetGraphic> _models = new();
    private readonly ISimulationService _simulationService;
    private readonly IWindowService _windowService;

    public SimulationViewportService(IWindowService windowService, ISimulationService simulationService)
    {
        _windowService = windowService;
        _simulationService = simulationService;
    }

    public async Task RenderAsync()
    {
        var viewport = _windowService.GetRenderViewport();
        if (viewport is null)
        {
            return;
        }

        var planetPositions = await _simulationService.GetPlanetPositionsAsync();

        List<PlanetGraphic> modelsToRemove = new();
        List<Guid> idsToRemove = new();
        foreach (var (id, uiElem) in _models)
        {
            if (!planetPositions.ContainsKey(id))
            {
                modelsToRemove.Add(uiElem);
                idsToRemove.Add(id);
            }
        }

        foreach (var planetGraphic in modelsToRemove)
        {
            viewport.Children.Remove(planetGraphic.Planet);

            if (planetGraphic.TrajectoryUi is not null)
            {
                viewport.Children.Remove(planetGraphic.TrajectoryUi);
            }
        }

        foreach (var id in idsToRemove)
        {
            _models.Remove(id);
        }

        List<Guid> idsToAdd = new();
        foreach (var (id, pos) in planetPositions)
        {
            if (!_models.ContainsKey(id))
            {
                idsToAdd.Add(id);
            }
        }

        foreach (var id in idsToAdd)
        {
            var planetModel = new Ellipsoid
            {
                XLength = 0.1,
                YLength = 0.1,
                ZLength = 0.1,
                Transform = new TranslateTransform3D(0, 0, 0),
                Material = new DiffuseMaterial(Brushes.Brown)
            };

            var trajectoryPoints = new Point3DCollection();

            var trajectoryUi = new ScreenSpaceLines3D
            {
                Color = Colors.Brown,
                Points = trajectoryPoints
            };
            _models.Add(id, new PlanetGraphic(planetModel, trajectoryUi, new List<Vector3>()));
            viewport.Children.Add(planetModel);
            viewport.Children.Add(trajectoryUi);
        }

        foreach (var (id, pos) in planetPositions)
        {
            var translate = _models[id].Planet.Transform as TranslateTransform3D;
            if (translate is null)
            {
                throw new InvalidOperationException("Translate is null");
            }

            var trajectoryPoints = _models[id].TrajectoryPoints;
            trajectoryPoints.AddPointIfFurther(pos);

            if (trajectoryPoints.Count > 100)
            {
                while (trajectoryPoints.Count > 90)
                {
                    trajectoryPoints.RemoveRange(0, 11);
                }
            }

            var trajectoryUi = _models[id].TrajectoryUi;

            translate.ApplyPosition(pos);
            trajectoryUi?.ApplyPoints(trajectoryPoints);
        }
    }
}

public static class TransformExtensions
{
    public static void ApplyPosition(this TranslateTransform3D transform, Vector3 position)
    {
        transform.OffsetX = position.X;
        transform.OffsetY = position.Y;
        transform.OffsetZ = position.Z;
    }
}

public static class ScreenSpaceLineExtensions
{
    public static void ApplyPoints(this ScreenSpaceLines3D spaceLine, List<Vector3> positions) =>
        spaceLine.Points = new Point3DCollection(positions.Select(p => new Point3D(p.X, p.Y, p.Z)));
}

public static class ListExtensions
{
    public static void AddPointIfFurther(this List<Vector3> points, Vector3 point)
    {
        if (points.Count == 0)
        {
            points.Add(point);
        }
        else
        {
            var lastPoint = points.Last();
            var distance = (point - lastPoint).Magnitude;
            if (distance >= 0.05)
            {
                points.Add(point);
            }
        }
    }
}

public record PlanetGraphic(UIElement3D Planet, ScreenSpaceLines3D? TrajectoryUi, List<Vector3> TrajectoryPoints);
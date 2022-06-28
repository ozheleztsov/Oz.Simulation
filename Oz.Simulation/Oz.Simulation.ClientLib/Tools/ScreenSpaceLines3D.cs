﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib.Tools;

/// <summary>
///     ScreenSpaceLines3D are a 3D line primitive whose thickness
///     is constant in 2D space post projection.
///     This means that the lines do not become foreshortened as
///     they receed from the camera as other 3D primitives do under
///     a typical perspective projection.
///     Example Usage:
///     &lt;tools:ScreenSpaceLines3D
///     Points="0,0,0 0,1,0 0,1,0 1,1,0 1,1,0 0,0,1"
///     Thickness="5" Color="Red"&gt;
///     "Screen space" is a bit of a misnomer as the line thickness
///     is specified in the 2D coordinate system of the container
///     Viewport3D, not the screen.
/// </summary>
public class ScreenSpaceLines3D : ModelVisual3D
{
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(ScreenSpaceLines3D),
            new PropertyMetadata(
                Colors.White,
                OnColorChanged));

    public static readonly DependencyProperty ThicknessProperty =
        DependencyProperty.Register(
            "Thickness",
            typeof(double),
            typeof(ScreenSpaceLines3D),
            new PropertyMetadata(
                1.0,
                OnThicknessChanged));

    public static readonly DependencyProperty PointsProperty =
        DependencyProperty.Register(
            "Points",
            typeof(Point3DCollection),
            typeof(ScreenSpaceLines3D),
            new PropertyMetadata(
                null,
                OnPointsChanged));

    private readonly MeshGeometry3D _mesh;
    private readonly GeometryModel3D _model;
    private Matrix3D _screenToVisual;

    private Matrix3D _visualToScreen;

    public ScreenSpaceLines3D()
    {
        _mesh = new MeshGeometry3D();
        _model = new GeometryModel3D {Geometry = _mesh};
        SetColor(Color);

        Content = _model;
        Points = new Point3DCollection();

        CompositionTarget.Rendering += OnRender;
    }

    public Color Color
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public Point3DCollection Points
    {
        get => (Point3DCollection)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    private static void OnColorChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((ScreenSpaceLines3D)sender).SetColor((Color)args.NewValue);

    private void SetColor(Color color)
    {
        var unlitMaterial = new MaterialGroup();
        unlitMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
        unlitMaterial.Children.Add(new EmissiveMaterial(new SolidColorBrush(color)));
        unlitMaterial.Freeze();

        _model.Material = unlitMaterial;
        _model.BackMaterial = unlitMaterial;
    }

    private static void OnThicknessChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((ScreenSpaceLines3D)sender).GeometryDirty();

    private static void OnPointsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args) =>
        ((ScreenSpaceLines3D)sender).GeometryDirty();

    private void OnRender(object? sender, EventArgs e)
    {
        if (Points.Count == 0 && _mesh.Positions.Count == 0)
        {
            return;
        }

        if (UpdateTransforms())
        {
            RebuildGeometry();
        }
    }

    private void GeometryDirty() =>
        // Force next call to UpdateTransforms() to return true.
        _visualToScreen = MathUtils.ZeroMatrix;

    private void RebuildGeometry()
    {
        var halfThickness = Thickness / 2.0;
        var numLines = Points.Count / 2;

        var positions = new Point3DCollection(numLines * 4);

        for (var i = 0; i < numLines; i++)
        {
            var startIndex = i * 2;

            var startPoint = Points[startIndex];
            var endPoint = Points[startIndex + 1];

            AddSegment(positions, startPoint, endPoint, halfThickness);
        }

        positions.Freeze();
        _mesh.Positions = positions;

        var indices = new Int32Collection(Points.Count * 3);

        for (var i = 0; i < Points.Count / 2; i++)
        {
            indices.Add((i * 4) + 2);
            indices.Add((i * 4) + 1);
            indices.Add((i * 4) + 0);

            indices.Add((i * 4) + 2);
            indices.Add((i * 4) + 3);
            indices.Add((i * 4) + 1);
        }

        indices.Freeze();
        _mesh.TriangleIndices = indices;
    }

    private void AddSegment(Point3DCollection positions, Point3D startPoint, Point3D endPoint, double halfThickness)
    {
        // NOTE: We want the vector below to be perpendicular post projection so
        //       we need to compute the line direction in post-projective space.
        var lineDirection = (endPoint * _visualToScreen) - (startPoint * _visualToScreen);
        lineDirection.Z = 0;
        lineDirection.Normalize();

        // NOTE: Implicit Rot(90) during construction to get a perpendicular vector.
        var delta = new Vector(-lineDirection.Y, lineDirection.X);
        delta *= halfThickness;

        Widen(startPoint, delta, out var pOut1, out var pOut2);

        positions.Add(pOut1);
        positions.Add(pOut2);

        Widen(endPoint, delta, out pOut1, out pOut2);

        positions.Add(pOut1);
        positions.Add(pOut2);
    }

    private void Widen(Point3D pIn, Vector delta, out Point3D pOut1, out Point3D pOut2)
    {
        var pIn4 = (Point4D)pIn;
        var pOut41 = pIn4 * _visualToScreen;
        var pOut42 = pOut41;

        pOut41.X += delta.X * pOut41.W;
        pOut41.Y += delta.Y * pOut41.W;

        pOut42.X -= delta.X * pOut42.W;
        pOut42.Y -= delta.Y * pOut42.W;

        pOut41 *= _screenToVisual;
        pOut42 *= _screenToVisual;

        // NOTE: Z is not modified above, so we use the original Z below.

        pOut1 = new Point3D(
            pOut41.X / pOut41.W,
            pOut41.Y / pOut41.W,
            pOut41.Z / pOut41.W);

        pOut2 = new Point3D(
            pOut42.X / pOut42.W,
            pOut42.Y / pOut42.W,
            pOut42.Z / pOut42.W);
    }

    private bool UpdateTransforms()
    {
        var visualToScreen = MathUtils.TryTransformTo2DAncestor(this, out _, out var success);

        if (!success || !visualToScreen.HasInverse)
        {
            _mesh.Positions = null;
            return false;
        }

        if (visualToScreen == _visualToScreen)
        {
            return false;
        }

        _visualToScreen = _screenToVisual = visualToScreen;
        _screenToVisual.Invert();

        return true;
    }

    #region MakeWireframe

    public void MakeWireframe(Model3D? model)
    {
        Points.Clear();

        if (model == null)
        {
            return;
        }

        var transform = new Matrix3DStack();
        transform.Push(Matrix3D.Identity);

        WireframeHelper(model, transform);
    }

    private void WireframeHelper(Model3D model, Matrix3DStack matrixStack)
    {
        var transform = model.Transform;

        if (transform != null && transform != Transform3D.Identity)
        {
            matrixStack.Prepend(model.Transform.Value);
        }

        try
        {
            if (model is Model3DGroup group)
            {
                WireframeHelper(group, matrixStack);
                return;
            }

            if (model is GeometryModel3D geometry)
            {
                WireframeHelper(geometry, matrixStack);
            }
        }
        finally
        {
            if (transform != null && transform != Transform3D.Identity)
            {
                matrixStack.Pop();
            }
        }
    }

    private void WireframeHelper(Model3DGroup group, Matrix3DStack matrixStack)
    {
        foreach (var child in group.Children)
        {
            WireframeHelper(child, matrixStack);
        }
    }

    private void WireframeHelper(GeometryModel3D model, Matrix3DStack matrixStack)
    {
        var geometry = model.Geometry;

        if (geometry is not MeshGeometry3D mesh)
        {
            return;
        }

        var positions = new Point3D[mesh.Positions.Count];
        mesh.Positions.CopyTo(positions, 0);
        matrixStack.Peek().Transform(positions);

        var indices = mesh.TriangleIndices;

        if (indices.Count > 0)
        {
            var limit = positions.Length - 1;

            for (int i = 2, count = indices.Count; i < count; i += 3)
            {
                var i0 = indices[i - 2];
                var i1 = indices[i - 1];
                var i2 = indices[i];

                // WPF halts rendering on the first deformed triangle.  We should
                // do the same.
                if (0 > i0 || i0 > limit || 0 > i1 || i1 > limit || 0 > i2 || i2 > limit)
                {
                    break;
                }

                AddTriangle(positions, i0, i1, i2);
            }
        }
        else
        {
            for (int i = 2, count = positions.Length; i < count; i += 3)
            {
                var i0 = i - 2;
                var i1 = i - 1;
                var i2 = i;

                AddTriangle(positions, i0, i1, i2);
            }
        }
    }

    private void AddTriangle(IReadOnlyList<Point3D> positions, int i0, int i1, int i2)
    {
        Points.Add(positions[i0]);
        Points.Add(positions[i1]);
        Points.Add(positions[i1]);
        Points.Add(positions[i2]);
        Points.Add(positions[i2]);
        Points.Add(positions[i0]);
    }

    #endregion MakeWireframe
}
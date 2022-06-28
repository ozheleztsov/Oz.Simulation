//---------------------------------------------------------------------------
//
// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Limited Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/limitedpermissivelicense.mspx
// All other rights reserved.
//
// This file is part of the 3D Tools for Windows Presentation Foundation
// project.  For more information, see:
// 
// http://CodePlex.com/Wiki/View.aspx?ProjectName=3DTools
//
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib.Tools;

#region Mesh IValueConverters

/// <summary>
///     Abstract base class for all type converters that have a MeshGeometry3D source
/// </summary>
public abstract class MeshConverter<TargetType> : IValueConverter
{
    /// <summary>
    ///     IValueConverter.Convert
    /// </summary>
    /// <param name="value">The binding source (should be a MeshGeometry3D)</param>
    /// <param name="targetType">The binding target</param>
    /// <param name="parameter">Optionaly parameter to the converter</param>
    /// <param name="culture">(ignored)</param>
    /// <returns>The converted value</returns>
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return DependencyProperty.UnsetValue;
        }

        if (targetType != typeof(TargetType))
        {
            throw new ArgumentException(string.Format("MeshConverter must target a {0}", typeof(TargetType).Name));
        }

        var mesh = value as MeshGeometry3D;
        if (mesh == null)
        {
            throw new ArgumentException("MeshConverter can only convert from a MeshGeometry3D");
        }

        return Convert(mesh, parameter);
    }

    /// <summary>
    ///     IValueConverter.ConvertBack
    ///     Not implemented
    /// </summary>
    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();

    /// <summary>
    ///     Subclasses should override this to do conversion
    /// </summary>
    /// <param name="mesh">The mesh source</param>
    /// <param name="parameter">Optional converter argument</param>
    /// <returns>The converted value</returns>
    public abstract object Convert(MeshGeometry3D mesh, object parameter);
}

/// <summary>
///     MeshTextureCoordinateConverter
///     A MeshConverter that returns a PointCollection and takes an optional direction argument
/// </summary>
public abstract class MeshTextureCoordinateConverter : MeshConverter<PointCollection>
{
    /// <summary>
    /// </summary>
    /// <param name="mesh">The source mesh</param>
    /// <param name="parameter">The optional parameter</param>
    /// <returns>The converted value</returns>
    public override object Convert(MeshGeometry3D mesh, object parameter)
    {
        var paramAsString = parameter as string;
        if (parameter != null && paramAsString == null)
        {
            throw new ArgumentException("Parameter must be a string.");
        }

        // Default to the positive Y axis
        var dir = MathUtils.YAxis;

        if (paramAsString != null)
        {
            dir = Vector3D.Parse(paramAsString);
            MathUtils.TryNormalize(ref dir);
        }

        return Convert(mesh, dir);
    }

    /// <summary>
    ///     Subclasses should override this to do conversion
    /// </summary>
    /// <param name="mesh">The source mesh</param>
    /// <param name="dir">The normalized direction parameter</param>
    /// <returns></returns>
    public abstract object Convert(MeshGeometry3D mesh, Vector3D dir);
}

/// <summary>
///     IValueConverter that generates texture coordinates for a plane.
/// </summary>
public class PlanarTextureCoordinateGenerator : MeshTextureCoordinateConverter
{
    public override object Convert(MeshGeometry3D mesh, Vector3D dir) =>
        MeshUtils.GeneratePlanarTextureCoordinates(mesh, dir);
}

/// <summary>
///     IValueConverter that generates texture coordinates for a sphere.
/// </summary>
public class SphericalTextureCoordinateGenerator : MeshTextureCoordinateConverter
{
    public override object Convert(MeshGeometry3D mesh, Vector3D dir) =>
        MeshUtils.GenerateSphericalTextureCoordinates(mesh, dir);
}

/// <summary>
///     IValueConverter that generates texture coordinates for a cylinder
/// </summary>
public class CylindricalTextureCoordinateGenerator : MeshTextureCoordinateConverter
{
    public override object Convert(MeshGeometry3D mesh, Vector3D dir) =>
        MeshUtils.GenerateCylindricalTextureCoordinates(mesh, dir);
}

#endregion

public static class MeshUtils
{
    internal static double GetPlanarCoordinate(double end, double start, double width) =>
        (end - start) / width;

    internal static double GetUnitCircleCoordinate(double y, double x) =>
        (Math.Atan2(y, x) / (2.0 * Math.PI)) + 0.5;

    /// <summary>
    ///     Finds the transform from 'dir' to '<0, 1, 0>' and transforms 'bounds'
    ///     and 'points' by it.
    /// </summary>
    /// <param name="bounds">The bounds to transform</param>
    /// <param name="points">The vertices to transform</param>
    /// <param name="dir">The orientation of the mesh</param>
    /// <returns>
    ///     The transformed points. If 'dir' is already '<0, 1, 0>' then this
    ///     will equal 'points.'
    /// </returns>
    internal static IEnumerable<Point3D> TransformPoints(ref Rect3D bounds, Point3DCollection points, ref Vector3D dir)
    {
        if (dir == MathUtils.YAxis)
        {
            return points;
        }

        var rotAxis = Vector3D.CrossProduct(dir, MathUtils.YAxis);
        var rotAngle = Vector3D.AngleBetween(dir, MathUtils.YAxis);
        Quaternion q;

        if (rotAxis.X != 0 || rotAxis.Y != 0 || rotAxis.Z != 0)
        {
            Debug.Assert(rotAngle != 0);

            q = new Quaternion(rotAxis, rotAngle);
        }
        else
        {
            Debug.Assert(dir == -MathUtils.YAxis);

            q = new Quaternion(MathUtils.XAxis, rotAngle);
        }

        var center = new Vector3D(
            bounds.X + (bounds.SizeX / 2),
            bounds.Y + (bounds.SizeY / 2),
            bounds.Z + (bounds.SizeZ / 2)
        );

        var t = Matrix3D.Identity;
        t.Translate(-center);
        t.Rotate(q);

        var count = points.Count;
        var transformedPoints = new Point3D[count];

        for (var i = 0; i < count; i++)
        {
            transformedPoints[i] = t.Transform(points[i]);
        }

        // Finally, transform the bounds too
        bounds = MathUtils.TransformBounds(bounds, t);

        return transformedPoints;
    }

    #region Texture Coordinate Generation Methods

    /// <summary>
    ///     Generates texture coordinates as if mesh were a cylinder.
    ///     Notes:
    ///     1) v is flipped for you automatically
    ///     2) 'mesh' is not modified. If you want the generated coordinates
    ///     to be assigned to mesh, do:
    ///     mesh.TextureCoordinates = GenerateCylindricalTextureCoordinates(mesh, foo)
    /// </summary>
    /// <param name="mesh">The mesh</param>
    /// <param name="dir">The axis of rotation for the cylinder</param>
    /// <returns>The generated texture coordinates</returns>
    public static PointCollection GenerateCylindricalTextureCoordinates(MeshGeometry3D mesh, Vector3D dir)
    {
        if (mesh == null)
        {
            return null;
        }

        var bounds = mesh.Bounds;
        var count = mesh.Positions.Count;
        var texcoords = new PointCollection(count);
        var positions = TransformPoints(ref bounds, mesh.Positions, ref dir);

        foreach (var vertex in positions)
        {
            texcoords.Add(new Point(
                GetUnitCircleCoordinate(-vertex.Z, vertex.X),
                1.0 - GetPlanarCoordinate(vertex.Y, bounds.Y, bounds.SizeY)
            ));
        }

        return texcoords;
    }

    /// <summary>
    ///     Generates texture coordinates as if mesh were a sphere.
    ///     Notes:
    ///     1) v is flipped for you automatically
    ///     2) 'mesh' is not modified. If you want the generated coordinates
    ///     to be assigned to mesh, do:
    ///     mesh.TextureCoordinates = GenerateSphericalTextureCoordinates(mesh, foo)
    /// </summary>
    /// <param name="mesh">The mesh</param>
    /// <param name="dir">The axis of rotation for the sphere</param>
    /// <returns>The generated texture coordinates</returns>
    public static PointCollection GenerateSphericalTextureCoordinates(MeshGeometry3D mesh, Vector3D dir)
    {
        if (mesh == null)
        {
            return null;
        }

        var bounds = mesh.Bounds;
        var count = mesh.Positions.Count;
        var texcoords = new PointCollection(count);
        var positions = TransformPoints(ref bounds, mesh.Positions, ref dir);

        foreach (var vertex in positions)
        {
            // Don't need to do 'vertex - center' since TransformPoints put us
            // at the origin
            var radius = new Vector3D(vertex.X, vertex.Y, vertex.Z);
            MathUtils.TryNormalize(ref radius);

            texcoords.Add(new Point(
                GetUnitCircleCoordinate(-radius.Z, radius.X),
                1.0 - ((Math.Asin(radius.Y) / Math.PI) + 0.5)
            ));
        }

        return texcoords;
    }

    /// <summary>
    ///     Generates texture coordinates as if mesh were a plane.
    ///     Notes:
    ///     1) v is flipped for you automatically
    ///     2) 'mesh' is not modified. If you want the generated coordinates
    ///     to be assigned to mesh, do:
    ///     mesh.TextureCoordinates = GeneratePlanarTextureCoordinates(mesh, foo)
    /// </summary>
    /// <param name="mesh">The mesh</param>
    /// <param name="dir">The normal of the plane</param>
    /// <returns>The generated texture coordinates</returns>
    public static PointCollection GeneratePlanarTextureCoordinates(MeshGeometry3D mesh, Vector3D dir)
    {
        if (mesh == null)
        {
            return null;
        }

        var bounds = mesh.Bounds;
        var count = mesh.Positions.Count;
        var texcoords = new PointCollection(count);
        var positions = TransformPoints(ref bounds, mesh.Positions, ref dir);

        foreach (var vertex in positions)
        {
            // The plane is looking along positive Y, so Z is really Y

            texcoords.Add(new Point(
                GetPlanarCoordinate(vertex.X, bounds.X, bounds.SizeX),
                GetPlanarCoordinate(vertex.Z, bounds.Z, bounds.SizeZ)
            ));
        }

        return texcoords;
    }

    #endregion
}
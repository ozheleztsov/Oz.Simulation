using Oz.Simulation.ClientLib.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib;

public static class PredefinedObjects
{
    private static Point3D GetSpherePosition(double radius, double theta, double phi)
    {
        var pt = new Point3D();
        var snt = Math.Sin(theta * Math.PI / 180);
        var cnt = Math.Cos(theta * Math.PI / 180);
        var snp = Math.Sin(phi * Math.PI / 180);
        var cnp = Math.Cos(phi * Math.PI / 180);
        pt.X = radius * snt * cnp;
        pt.Y = radius * cnt;
        pt.Z = -radius * snt * snp;
        return pt;
    }

    public static GeometricModel? CreateSphere(Point3D center, double radius, int u, int v, Color color, bool isWireframe)
    {
        if (u < 2 || v < 2)
        {
            return null;
        }

        var pts = new Point3D[u, v];
        for (var i = 0; i < u; i++)
        {
            for (var j = 0; j < v; j++)
            {
                pts[i, j] = GetSpherePosition(radius, i * 180.0 / (u - 1), j * 360.0 / (v - 1));
                pts[i, j] += (Vector3D)center;
            }
        }

        var p = new Point3D[4];
        List<ModelUIElement3D> triangles = new();
        List<ScreenSpaceLines3D> wireframes = new();

        for (var i = 0; i < u - 1; i++)
        {
            for (var j = 0; j < v - 1; j++)
            {
                p[0] = pts[i, j];
                p[1] = pts[i + 1, j];
                p[2] = pts[i + 1, j + 1];
                p[3] = pts[i, j + 1];
                var triangleFace1 = Utils3d.CreateTriangleFace(p[0], p[1], p[2], color, isWireframe);
                var triangleFace2 = Utils3d.CreateTriangleFace(p[2], p[3], p[0], color, isWireframe);
                triangles.Add(triangleFace1.Model);
                triangles.Add(triangleFace2.Model);
                if (isWireframe && triangleFace1.Wireframe != null && triangleFace2.Wireframe != null)
                {
                    wireframes.Add(triangleFace1.Wireframe);
                    wireframes.Add(triangleFace2.Wireframe);
                }
            }
        }

        return new GeometricModel(triangles, wireframes.Count > 0 ? wireframes : null);
    }
}

public record GeometricModel(List<ModelUIElement3D> Triangles, List<ScreenSpaceLines3D>? Wireframes);
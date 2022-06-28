using System;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib.Geometries;

public class EllipsoidGeometry
{
    public double XLength { get; set; } = 1.0;
    public double YLength { get; set; } = 1.0;
    public double ZLength { get; set; } = 1.0;
    public int ThetaDiv { get; set; } = 30;
    public int PhiDiv { get; set; } = 20;
    public Point3D Center { get; set; } = new();

    public MeshGeometry3D Mesh => GetMesh();

    private MeshGeometry3D GetMesh()
    {
        var dt = 360.0 / ThetaDiv;
        var dp = 180.0 / PhiDiv;
        MeshGeometry3D mesh = new();
        for (var i = 0; i <= PhiDiv; i++)
        {
            var phi = i * dp;
            for (var j = 0; j <= ThetaDiv; j++)
            {
                var theta = j * dt;
                mesh.Positions.Add(GetPosition(theta, phi));
            }
        }

        for (var i = 0; i < PhiDiv; i++)
        {
            for (var j = 0; j < ThetaDiv; j++)
            {
                var x0 = j;
                var x1 = j + 1;
                var y0 = i * (ThetaDiv + 1);
                var y1 = (i + 1) * (ThetaDiv + 1);
                mesh.TriangleIndices.Add(x0 + y0);
                mesh.TriangleIndices.Add(x0 + y1);
                mesh.TriangleIndices.Add(x1 + y0);
                mesh.TriangleIndices.Add(x1 + y0);
                mesh.TriangleIndices.Add(x0 + y1);
                mesh.TriangleIndices.Add(x1 + y1);
            }
        }

        mesh.Freeze();
        return mesh;
    }

    private Point3D GetPosition(double theta, double phi)
    {
        theta *= Math.PI / 180.0;
        phi *= Math.PI / 180.0;
        var x = XLength * Math.Sin(theta) * Math.Sin(phi);
        var y = YLength * Math.Cos(phi);
        var z = -ZLength * Math.Cos(theta) * Math.Sin(phi);
        var pt = new Point3D(x, y, z);
        pt += (Vector3D)Center;
        return pt;
    }
}
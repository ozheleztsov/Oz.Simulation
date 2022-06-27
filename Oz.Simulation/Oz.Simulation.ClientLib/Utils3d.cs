using System;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib;

public static class Utils3d
{
    public static Matrix3D SetViewMatrix(Point3D cameraPosition, Vector3D lookDirection, Vector3D upDirection)
    {
        lookDirection.Normalize();
        upDirection.Normalize();

        var denominator = Math.Sqrt(1.0 - Math.Pow(Vector3D.DotProduct(lookDirection, upDirection), 2));
        var xScale = Vector3D.CrossProduct(lookDirection, upDirection) / denominator;
        var yScale = (upDirection - (Vector3D.DotProduct(upDirection, lookDirection) * lookDirection)) / denominator;
        var zScale = lookDirection;
        var m = new Matrix3D {M11 = xScale.X, M21 = xScale.Y, M31 = xScale.Z, M12 = yScale.X,
            M22 = yScale.Y,
            M32 = yScale.Z,
            M13 = zScale.X,
            M23 = zScale.Y,
            M33 = zScale.Z
        };

        var translateMatrix = new Matrix3D();
        translateMatrix.Translate(new Vector3D(-cameraPosition.X, -cameraPosition.Y, -cameraPosition.Z));
        var reflectMatrix = new Matrix3D {M33 = -1};
        var viewMatrix = translateMatrix * m * reflectMatrix;
        return viewMatrix;
    }

    public static Matrix3D SetPerspectiveOffCenter(double left, double right, double bottom, double top,
        double near, double far)
    {
        var perspectiveMatrix = new Matrix3D
            {
                M11 = 2 * near / (right - left), 
                M22 = 2 * near / (top - bottom), 
                M31 = (right + left) / (right - left),
                M32 = (top + bottom) / (top - bottom),
                M33 = far / (near - far),
                M34 = -1.0,
                OffsetZ = near * far / (near - far),
                M44 = 0
            };
        return perspectiveMatrix;
    }

    public static Matrix3D SetPerspective(double width, double height, double near, double far)
    {
        var perspectiveMatrix = new Matrix3D
            {
                M11 = 2 * near / width, 
                M22 = 2 * near / height, 
                M33 = far / (near - far), 
                M34 = -1.0,
                OffsetZ = near * far / (near - far),
                M44 = 0
            };
        return perspectiveMatrix;
    }

    public static Matrix3D SetPerspectiveFov(double fov, double aspectRatio, double near, double far)
    {
        var perspectiveMatrix = new Matrix3D();
        var yScale = 1.0 / Math.Tan(fov * Math.PI / 180 / 2);
        var xScale = yScale / aspectRatio;
        perspectiveMatrix.M11 = xScale;
        perspectiveMatrix.M22 = yScale;
        perspectiveMatrix.M33 = far / (near - far);
        perspectiveMatrix.M34 = -1.0;
        perspectiveMatrix.OffsetZ = near * far / (near - far);
        perspectiveMatrix.M44 = 0;
        return perspectiveMatrix;
    }
}
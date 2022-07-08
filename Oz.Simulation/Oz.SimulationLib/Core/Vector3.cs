namespace Oz.SimulationLib.Core;

public readonly struct Vector3
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3() =>
        X = Y = Z = 0.0;

    public Vector3(double value) =>
        X = Y = Z = value;

    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3(IReadOnlyList<double> components)
    {
        switch (components.Count)
        {
            case 0:
                X = Y = Z = 0.0;
                break;
            case 1:
                X = components[0];
                Y = Z = 0.0;
                break;
            case 2:
                X = components[0];
                Y = components[1];
                Z = 0.0;
                break;
            default:
                X = components[0];
                Y = components[1];
                Z = components[2];
                break;
        }
    }

    public double MagnitudeSqr => (X * X) + (Y * Y) + (Z * Z);

    public double Magnitude => Math.Sqrt(MagnitudeSqr);

    public Vector3 Normalized
    {
        get
        {
            var magnitude = Magnitude;
            return new Vector3(X / magnitude, Y / magnitude, Z / magnitude);
        }
    }

    public static Vector3 operator +(Vector3 v1, Vector3 v2) =>
        new(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);

    public static Vector3 operator -(Vector3 v1, Vector3 v2) =>
        new(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);

    public static double Dot(Vector3 v1, Vector3 v2) =>
        (v1.X * v2.X) + (v1.Y * v2.Y) + (v1.Z * v2.Z);

    public static Vector3 Cross(Vector3 v1, Vector3 v2)
    {
        var x = v1.Y * v2.Z - v1.Z * v2.Y;
        var y = v1.Z * v2.X - v1.X * v2.Z;
        var z = v1.X * v2.Y - v1.Y * v2.X;
        return new Vector3(x, y, z);
    }

    public static Vector3 operator -(Vector3 v) =>
        new(-v.X, -v.Y, -v.Z);

    public override readonly string ToString() =>
        $"[{X}, {Y}, {Z}]";

    public static Vector3 operator *(double d, Vector3 v) =>
        new(d * v.X, d * v.Y, d * v.Z);
    
    public static Vector3 operator *(Vector3 v, double d) =>
        new(d * v.X, d * v.Y, d * v.Z);
}
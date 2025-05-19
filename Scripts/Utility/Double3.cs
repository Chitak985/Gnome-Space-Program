// Higher precision Vector3 for those days when you need a hefty number
using System;
using Godot;

public class Double3(double x, double y, double z)
{
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public double Z { get; set; } = z;

    // Operator overloads
    public static Double3 operator +(Double3 a, Double3 b){return new Double3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);}
    public static Double3 operator -(Double3 a, Double3 b){return new Double3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);}
    public static Double3 operator *(Double3 a, Double3 b){return new Double3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);}
    public static Double3 operator /(Double3 a, Double3 b){return new Double3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);}

    // Default vectors
    public static Double3 Zero { get; } = new Double3(0,0,0);
    public static Double3 One { get; } = new Double3(1,1,1);

    // I add to this as I go so don't expect everything you need to be here
    public double Length()
    {
        return Math.Sqrt(X*X + Y*Y + Z*Z);
    }

    public static Double3 Cross(Double3 a, Double3 b)
    {
        return new Double3(
            (a.Y * b.Z) - (a.Z * b.Y),
            (a.Z * b.X) - (a.X * b.Z),
            (a.X * b.Y) - (a.Y * b.X)
        );
    }

    public static double Dot(Double3 a, Double3 b)
    {
        return (a.X * b.X)
                + (a.Y * b.Y)
                + (a.Z * b.Z);
    }

    public Vector3 ToFloat3()
    {
        return new Vector3(
            (float)X,
            (float)Y,
            (float)Z);
    }
}

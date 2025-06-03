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

    public static Double3 operator *(Double3 a, double b){return new Double3(a.X * b, a.Y * b, a.Z * b);}
    public static Double3 operator /(Double3 a, double b){return new Double3(a.X / b, a.Y / b, a.Z / b);}
    public static Double3 operator *(double a, Double3 b){return new Double3(a * b.X, a * b.Y, a * b.Z);}
    public static Double3 operator /(double a, Double3 b){return new Double3(a / b.X, a / b.Y, a / b.Z);}

    // Default vectors
    public static Double3 Zero { get; } = new Double3(0,0,0);
    public static Double3 One { get; } = new Double3(1,1,1);

    // Functions to get points with Y as up rather than Z
    // (Because I don't want to even LOOK at the coordinate transformation functions again thank you very much)
    public Double3 GetPosYUp()
    {
        return new Double3(X,Z,Y);
    }

    // I add to this as I go so don't expect everything you need to be here
    public double Length()
    {
        return Math.Sqrt(X*X + Y*Y + Z*Z);
    }

    public Double3 Clip(double min, double max)
    {
        double x = X;
        if (x < min)
        {
            x = min;
        }else if (x > max){
            x = max;
        }

        double y = Y;
        if (y < min)
        {
            y = min;
        }else if (y > max){
            y = max;
        }

        double z = Z;
        if (z < min)
        {
            z = min;
        }else if (z > max){
            z = max;
        }

        return new Double3(x,y,z);
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

    public Double3 Normalized()
    {
        return this / Length();
    }

    public Double3 Pow(double n)
    {
        return new Double3(Math.Pow(X,n),Math.Pow(Y,n),Math.Pow(Z,n));
    }

    public Vector3 ToFloat3()
    {
        return new Vector3(
            (float)X,
            (float)Y,
            (float)Z);
    }
}

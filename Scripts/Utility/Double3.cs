// Higher precision Vector3 for those days when you need a hefty number
public class Double3(double x, double y, double z)
{
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Z { get; } = z;

    // Operator overloads
    public static Double3 operator +(Double3 a, Double3 b){return new Double3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);}
    public static Double3 operator -(Double3 a, Double3 b){return new Double3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);}
    public static Double3 operator *(Double3 a, Double3 b){return new Double3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);}
    public static Double3 operator /(Double3 a, Double3 b){return new Double3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);}

    // Default vectors
    public static Double3 Zero { get; } = new Double3(0,0,0);
    public static Double3 One { get; } = new Double3(1,1,1);
}

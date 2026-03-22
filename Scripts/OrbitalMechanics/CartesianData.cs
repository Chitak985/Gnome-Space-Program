using System;
using Godot;

// Cartesian data
public class CartesianData
{
    public CelestialBody parent;
    public CelestialBody cBody;

    public Vector3 position;
    public Vector3 rotation;
    public Vector3 velocity;

    public override string ToString()
    {
        string posRes = $"\n{Math.Round(position.X, 2)}, \n{Math.Round(position.Y, 2)}, \n{Math.Round(position.Z, 2)}";
        string rotRes = $"\n{Math.Round(rotation.X, 2)}, \n{Math.Round(rotation.Y, 2)}, \n{Math.Round(rotation.Z, 2)}";
        string velRes = $"\n{Math.Round(velocity.X, 2)}, \n{Math.Round(velocity.Y, 2)}, \n{Math.Round(velocity.Z, 2)}";
        return $"Pos (m): {posRes} \n\nRot (rad): {rotRes} \n\nVel (m/s): {velRes}";
    }
}
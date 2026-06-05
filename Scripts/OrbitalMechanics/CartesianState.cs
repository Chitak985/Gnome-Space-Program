using System;
using Godot;

public partial class CartesianState : Node
{
    public CartesianElements elements;

    public struct CartesianElements
    {
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 velocity;
    }

    public override string ToString()
    {
        string posRes = $"\n{Math.Round(elements.position.X, 2)}, \n{Math.Round(elements.position.Y, 2)}, \n{Math.Round(elements.position.Z, 2)}";
        string rotRes = $"\n{Math.Round(elements.rotation.X, 2)}, \n{Math.Round(elements.rotation.Y, 2)}, \n{Math.Round(elements.rotation.Z, 2)}";
        string velRes = $"\n{Math.Round(elements.velocity.X, 2)}, \n{Math.Round(elements.velocity.Y, 2)}, \n{Math.Round(elements.velocity.Z, 2)}";
        return $"Pos (m): {posRes} \n\nRot (rad): {rotRes} \n\nVel (m/s): {velRes}";
    }
}

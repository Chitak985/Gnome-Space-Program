using Godot;
using System;

public partial class Background : Node3D
{
    [Export] private Node3D positionTarget;
    [Export] private Node3D rotationTarget;

    public override void _Process(double delta)
    {
        GlobalPosition = positionTarget.GlobalPosition;
        GlobalRotation = rotationTarget.GlobalRotation;
    }
}

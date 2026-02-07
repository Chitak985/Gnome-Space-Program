using Godot;
using System;

public partial class Background : Node3D
{
    public override void _Process(double delta)
    {
        // Keep the background still if we're in the map, spin it around otherwise
        if (FlightCamera.Instance.inMap)
        {
            Rotation = Vector3.Zero;
        }else{
            Rotation = LocalSpace.Instance.Planets.Rotation;
        }
    }
}

using Godot;
using System;

// MapIcon for ordinary Node3Ds
public partial class MapIcon : TextureButton
{
    [Export] public Node3D thing;
    [Export] public Camera3D camera; // Doesn't matter if it's the scaled or local camera

    public override void _Process(double delta)
    {
        ForceUpdate();
    }

    public void ForceUpdate()
    {
        if (thing != null && camera != null) 
        {
            Vector3 thingPos = thing.GlobalPosition;
            Vector2 screenPos = camera.UnprojectPosition(thingPos);

            Position = screenPos;
        }
    }

    public override void _Pressed()
    {
        FlightCamera flightCam = FlightCamera.Instance;
        //flightCam.TargetObject(thing, new Vector3(1, float.PositiveInfinity, 2), true); //thing.Scale.Z * 1.25f, thing.Scale.Z * 2f);
    }
}

using Godot;
using System;

// MapIcon for ordinary Node3Ds
public partial class ColonyIcon : MapIcon
{
    [Export] public Colony colony;

    public override void _PhysicsProcess(double delta)
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
        //flightCam.TargetObject(thing, new Vector3(1, float.PositiveInfinity, 2), true);

        Godot.Collections.Dictionary info = new()
        {
            { "colony", colony }
        };

        MapUI.Instance.contextMenus.OpenMenu("ColonyMenu", info, true);
    }
}

using Godot;
using System;

public partial class PlanetIcon : MapIcon
{
    [Export] public CelestialBody planet;

    public override void _Ready()
    {
        thing = planet.mapObject;
    }

    public override void _Pressed()
    {
        FlightCamera flightCam = FlightCamera.Instance;
        flightCam.TargetObject(planet);

        Godot.Collections.Dictionary info = new()
        {
            { "planet", planet }
        };

        MapUI.Instance.contextMenus.OpenMenu("PlanetMenu", info, true);
    }
}

using Godot;
using System;
using System.Collections.Generic;

public partial class CBodyLight : Node3D
{
    [Export] private DirectionalLight3D localLight;
    [Export] private DirectionalLight3D mapLight;

    private CelestialBody cBody;

    [Export] public float brightness = 1;
    [Export] public Color colour = new(1,1,1,1);

    public void Create(CelestialBody cBody)
    {
        this.cBody = cBody;

        mapLight.GetParent().RemoveChild(mapLight);
        MapView.Instance.AddChild(mapLight);
    }

    public void UpdateLight()
    {
        if (cBody != null)
        {
            mapLight.LookAtFromPosition(cBody.mapObject.GlobalPosition, FlightCamera.Instance.GlobalPosition);
            localLight.LookAtFromPosition(cBody.GlobalPosition, FlightCamera.Instance.GlobalPosition);
        }

        localLight.LightEnergy = brightness;
        mapLight.LightEnergy = brightness;

        localLight.LightColor = colour;
        mapLight.LightColor = colour;
    }
}

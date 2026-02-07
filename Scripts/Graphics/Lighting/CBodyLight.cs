using Godot;
using System;

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
    }

    public void UpdateLight()
    {
        if (cBody != null)
        {
            // Control lights depending on if we're in map or local
            if (FlightCamera.Instance.inMap)
            {
                mapLight.Visible = true;
                localLight.Visible = false;
                mapLight.LookAtFromPosition(cBody.mapObject.GlobalPosition, FlightCamera.Instance.GlobalPosition);
            }else{
                mapLight.Visible = false;
                localLight.Visible = true;
                localLight.LookAtFromPosition(cBody.GlobalPosition, FlightCamera.Instance.GlobalPosition);
            }
        }

        localLight.LightEnergy = brightness;
        mapLight.LightEnergy = brightness;

        localLight.LightColor = colour;
        mapLight.LightColor = colour;
    }
}

using Godot;

public partial class CBodyLight : Node3D
{
    [Export] private Node3D localLightContainer;
    [Export] private Node3D mapLightContainer;

    private CelestialBody cBody;

    [Export] public float brightness = 1;
    [Export] public Color colour = new(1,1,1,1);

    public void Create(CelestialBody cBody)
    {
        this.cBody = cBody;

        // This is a horrible idea please fix this as soon as you're not fixing every other possible issue in this game thank youuu :3
        mapLightContainer.GetParent().RemoveChild(mapLightContainer);
        MapView.Instance.AddChild(mapLightContainer);
    }

    public void UpdateLight()
    {
        if (cBody != null)
        {
            if(cBody.mapObject.GlobalPosition != MapView.Instance.mapCamera.GlobalPosition)
                mapLightContainer.LookAtFromPosition(cBody.mapObject.GlobalPosition, MapView.Instance.mapCamera.GlobalPosition);
            if(cBody.GlobalPosition != FlightCamera.Instance.GlobalPosition)
                localLightContainer.LookAtFromPosition(cBody.GlobalPosition, FlightCamera.Instance.GlobalPosition);
        }

        foreach (Node node in localLightContainer.GetChildren())
        {
            if (node is DirectionalLight3D light)
            {
                light.LightEnergy = brightness / localLightContainer.GetChildCount();
                light.LightColor = colour;
            }
        }

        foreach (Node node in mapLightContainer.GetChildren())
        {
            if (node is DirectionalLight3D light)
            {
                light.LightEnergy = brightness / localLightContainer.GetChildCount();
                light.LightColor = colour;
            }
        }
    }
}

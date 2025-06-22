using Godot;
using System;

public partial class FloatingOrigin : Node
{
    public static FloatingOrigin Instance;

    [Export] public bool enabled = true;
    [Export] public double distanceThreshold = 1000; // meters

    // This WILL get yucky and messy over time don't trust it fully
    public Double3 offset = Double3.Zero;

    public override void _Ready()
    {
        Instance = this;
    }

    // This sucks
    public override void _Process(double delta)
    {
        if (enabled)
        {
            Craft currentCraft = FlightManager.Instance.currentCraft;

            if (currentCraft.GlobalPosition != null)
            {
                double craftDistance = currentCraft.GlobalPosition.Length();

                if (craftDistance > distanceThreshold)
                {
                    offset -= Double3.ConvertToDouble3(currentCraft.GlobalPosition);
                    currentCraft.Position = Vector3.Zero;
                    foreach (CelestialBody cbody in PlanetSystem.Instance.celestialBodies)
                    {
                        cbody.ProcessOrbitalPosition();
                    }
                }
            }
        }else{
            offset = Double3.Zero;
        }
    }
}

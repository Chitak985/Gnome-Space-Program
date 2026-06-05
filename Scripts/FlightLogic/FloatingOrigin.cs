using Godot;
using System;

public partial class FloatingOrigin : Node
{
    public static FloatingOrigin Instance;

    [Export] public bool enabled = true;
    [Export] public double distanceThreshold = 1000; // meters

    // This WILL get yucky and messy over time don't trust it fully
    public Vector3 offset = Vector3.Zero;

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

            //if (currentCraft.GlobalPosition != null)
            //{
                double craftDistance = currentCraft.GlobalPosition.Length();

                if (craftDistance > distanceThreshold)
                {
                    offset -= currentCraft.GlobalPosition;
                    currentCraft.Position = Vector3.Zero;
                    foreach (CelestialBody cbody in CelestialBodyManager.Instance.CelestialBodies)
                    {
                        //cbody.ProcessOrbitalPosition();
                    }
                }
            //}
        }else{
            offset = Vector3.Zero;
        }
    }
}

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

    // Stupid function but we need things to be synchronized
    public void RunCheck()
    {
        if (enabled)
        {
            Craft currentCraft = FlightManager.Instance.currentCraft;

            if (currentCraft.truePosition != null)
            {
                double craftDistance = currentCraft.truePosition.Length();

                if (craftDistance > distanceThreshold)
                {
                    offset -= currentCraft.truePosition;

                    currentCraft.Position = Vector3.Zero;
                }
            }
        }else{
            offset = Double3.Zero;
        }
    }
}

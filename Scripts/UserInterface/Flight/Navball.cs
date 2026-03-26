using Godot;
using System;

public partial class Navball : Node3D
{
    public override void _Process(double delta)
    {
        Craft craft = StateManager.Instance.CurrentFlightState.activeCraft;

        // NOT CORRECT RIGHT NOW
        if (craft != null)
        {
            Rotation = craft.CentralPart.Rotation;
        }
    }
}

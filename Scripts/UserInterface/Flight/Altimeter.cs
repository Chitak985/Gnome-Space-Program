using Godot;
using System;

public partial class Altimeter : Node
{
    [Export] private int digits = 2;
    [Export] private RichTextLabel label;

    public override void _Process(double delta)
    {
        Craft craft = StateManager.Instance.CurrentFlightState.activeCraft;

        if (craft != null)
        {
            label.Text = $"{Math.Round(craft.OrbitDriver.parent.radius - craft.OrbitDriver.cartesian.position.Length(), digits)} m";
        }
    }
}

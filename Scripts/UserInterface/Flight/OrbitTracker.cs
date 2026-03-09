using Godot;
using System;

public partial class OrbitTracker : Control
{
    [Export] private RichTextLabel label;

    public override void _Process(double delta)
    {
        if (label.IsVisibleInTree())
        {
            Craft craft = StateManager.Instance.flightState.activeCraft;
            if (craft != null)
            {
                label.Text = craft.OrbitDriver.ToString();
            }
        }
    }
}

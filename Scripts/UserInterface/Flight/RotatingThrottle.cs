using Godot;
using System;

public partial class RotatingThrottle : Control
{
    [Export] private float emptyRot;
    [Export] private float fullRot;

    public override void _Process(double delta)
    {
        Craft craft = StateManager.Instance.CurrentFlightState.activeCraft;

        // NOT CORRECT RIGHT NOW
        if (craft != null)
        {
            Rotation = Mathf.Lerp(emptyRot, fullRot, craft.Throttle);
        }
    }
}

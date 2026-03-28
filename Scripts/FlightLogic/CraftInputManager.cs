using Godot;
using System;

// Singleton for keyboard controls (and other controls) of craft in flight
public partial class CraftInputManager : Node
{
    [Export] private double throttleIncrement = 0.1;

    [Export] private StringName throttleIncreaseAction;
    [Export] private StringName throttleDecreaseAction;

    private Craft activeCraft;

    public override void _Process(double delta)
    {
        activeCraft = StateManager.Instance.CurrentFlightState.activeCraft;
        if (activeCraft != null)
        {
            if (Input.IsActionPressed(throttleIncreaseAction))
            {
                activeCraft.SetThrottle(activeCraft.Throttle + throttleIncrement * delta);
            } else if (Input.IsActionPressed(throttleDecreaseAction)) {
                activeCraft.SetThrottle(activeCraft.Throttle - throttleIncrement * delta);
            }
        }
    }
}

using Godot;
using System;

public partial class Speedometer : Node
{
    [Export] private int digits = 2;
    [Export] private RichTextLabel label;
    [Export] private RichTextLabel typeLabel;

    [Export] private DisplayType displayType;

    private enum DisplayType
    {
        Orbit,
        Surface
    }

    public override void _Process(double delta)
    {
        Craft craft = StateManager.Instance.CurrentFlightState.activeCraft;
        
        typeLabel.Text = displayType.ToString();

        if (craft != null)
        {
            CelestialBody cBody = craft.OrbitDriver.ParentCBody;
            Vector3 vel = craft.OrbitDriver.CartState.elements.velocity;

            //if (displayType == DisplayType.Surface)
            //    vel -= cBody.GetLocalVelocity(cBody.GetSurfaceRotationVelocity(craft.OrbitDriver.cartesian.position));

            double speed = vel.Length();

            label.Text = $"{Math.Round(speed, digits):0.00} m/s";
        }
    }

    private void OnSwitchPressed()
    {
        displayType = displayType.Next();
    }
}

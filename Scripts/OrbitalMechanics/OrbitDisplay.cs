using Godot;
using System;

public partial class OrbitDisplay : Control
{
    public CelestialBody cBody;

    [Export] public Label cartParent;
    [Export] public Label position;
    [Export] public Label velocity;

    [Export] public Label parent;
    [Export] public Label MU;

    [Export] public Label semiMajorAxis;
    [Export] public Label eccentricity;
    [Export] public Label inclination;
    [Export] public Label argumentOfPeriapsis;
    [Export] public Label longitudeOfAscendingNode;
    [Export] public Label trueAnomaly;

    [Export] public Label period;

    public override void _Process(double delta)
    {
        if (cBody != null)
        {
            CartesianData cartData = cBody.cartesianData;
            cartParent.Text = $"Parent: {cartData.parent}";
            position.Text = $"Position: ({cartData.position.X}, {cartData.position.Y}, {cartData.position.Z})";
            velocity.Text = $"Velocity: ({cartData.velocity.X}, {cartData.velocity.Y}, {cartData.velocity.Z})";

            Orbit orbit = cBody.orbit;
            parent.Text = $"Parent: {orbit.parent}";
            MU.Text = $"MU: {orbit.MU}";
            semiMajorAxis.Text = $"Semimajor Axis: {orbit.semiMajorAxis}";
            eccentricity.Text = $"Eccentricity: {orbit.eccentricity}";
            inclination.Text = $"Inclination: {orbit.inclination}";
            argumentOfPeriapsis.Text = $"Arg of Periapsis: {orbit.argumentOfPeriapsis}";
            longitudeOfAscendingNode.Text = $"Lon of Ascending Node: {orbit.longitudeOfAscendingNode}";
            trueAnomaly.Text = $"True Anomaly: {orbit.trueAnomaly}";
            period.Text = $"Period: {orbit.period}";
        }
    }
}

using Godot;

public partial class Background : Node3D
{
    [Export] private bool AffectedByReferenceFrame = true;
    public override void _Process(double _delta)
    {
        if (RealityTangler.Instance.RotatingReferenceFrame != null)
        {
            Node3D localPlanets = LocalSpace.Instance.Planets;

            GlobalRotation = localPlanets.GlobalRotation;
        }else{
            GlobalRotation = Vector3.Zero;
        }

        GlobalPosition = FlightCamera.Instance.CamNode.GlobalPosition;
    }
}

using Godot;

public partial class ScaledObject : Node3D
{
    public Node3D counterpart;
    public Vector3 truePosition = Vector3.Zero;

    // Pray you never have to use this in anything other than scaled space
    public Vector3 altPosition = Vector3.Zero;

    public Vector3 originalScale = Vector3.One;

    public override void _Process(double delta)
    {
        ForceUpdate();
    }

    public void ForceUpdate()
    {
        // If the counterpart is null then assume that another object is handling positioning
        if (counterpart != null)
            truePosition = counterpart.Position;
    }
}

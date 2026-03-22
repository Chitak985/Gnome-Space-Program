using Godot;
using Godot.Collections;

public partial class RocketEngine : PartModule
{
    public Node3D ThrustPivot { get; private set; }
    public Vector3 ThrustDirection { get; private set; } // Local to the pivot, normalized vector
    public double Thrust { get; private set; }

    public override void PartInit() 
    {
        Array<double> posArray = (Array<double>)configData["pivotPos"];

        ThrustPivot = new() {
            Position = new(
            posArray[0],
            posArray[1],
            posArray[2])
        };

        part.AddChild(ThrustPivot);

        Array<double> thrustArray = (Array<double>)configData["thrustDirection"];

        ThrustDirection = new Vector3(thrustArray[0], thrustArray[1], thrustArray[2]);

        Thrust = (double)configData["thrust"];
    }

    public override void PartProcess()
    {
        part.AddForce(CalculateForce(), Vector3.Zero); // Force is applied the center of part, this is fine but should be fixed later
    }

    private Vector3 CalculateForce()
    {
        Vector3 forceVector = ThrustDirection * Thrust;
        Vector3 globalForceVector = ThrustPivot.GlobalTransform.Basis * forceVector;

        return globalForceVector;
    }
}

using Godot;
using Godot.Collections;

public partial class ReactionWheel : PartModule
{
    public StringName PitchUp { get; private set; }
    public StringName PitchDown { get; private set; }
    public StringName YawRight { get; private set; }
    public StringName YawLeft { get; private set; }
    public StringName RollRight { get; private set; }
    public StringName RollLeft { get; private set; }

    public Vector3 X { get; private set; }
    public Vector3 Y { get; private set; }
    public Vector3 Z { get; private set; }

    public double Force { get; private set; }
    public double DampForce { get; private set; }

    public override void PartInit() 
    {
        Array<double> xArr = (Array<double>)configData["x"];
        Array<double> yArr = (Array<double>)configData["y"];
        Array<double> zArr = (Array<double>)configData["z"];

        X = new(
            xArr[0],
            xArr[1],
            xArr[2]
        );
        Y = new(
            yArr[0],
            yArr[1],
            yArr[2]
        );
        Z = new(
            zArr[0],
            zArr[1],
            zArr[2]
        );

        Force = (double)configData["force"];
        DampForce = (double)configData["dampForce"];

        // Input maps
        PitchUp = (string)configData["pitchUp"];
        PitchDown = (string)configData["pitchDown"];

        YawRight = (string)configData["yawRight"];
        YawLeft = (string)configData["yawLeft"];

        RollRight = (string)configData["rollRight"];
        RollLeft = (string)configData["rollLeft"];
    }

    public override void PartProcess()
    {
        // Pitch
        if (Input.IsActionPressed(PitchDown))
        {
            part.AddAngularForce(part.Basis * X * Force);
        }
        if (Input.IsActionPressed(PitchUp))
        {
            part.AddAngularForce(part.Basis * X * -Force);
        }

        // Yaw
        if (Input.IsActionPressed(YawRight))
        {
            part.AddAngularForce(part.Basis * Z * Force);
        }
        if (Input.IsActionPressed(YawLeft))
        {
            part.AddAngularForce(part.Basis * Z * -Force);
        }

        // Roll
        if (Input.IsActionPressed(RollLeft))
        {
            part.AddAngularForce(part.Basis * Y * Force);
        }
        if (Input.IsActionPressed(RollRight))
        {
            part.AddAngularForce(part.Basis * Y * -Force);
        }

        Stabilize();
    }

    // Attempts to stop rotation
    private void Stabilize()
    {
        part.AddAngularForce(-part.AngularVelocity * DampForce);
    }
}

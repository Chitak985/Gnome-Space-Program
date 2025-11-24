using Godot;
using System;

/* 
Everything "reality-breaking" should ideally be done here.

That includes stuff like floating origin, inverse rotation, velocity frames, all that BS.
Exceptions may include ScaledSpace and the OrbitManager 
(Stuff within OrbitManager might need to be moved here..? or vice-versa depends on how angry I am)
*/
public partial class RealityTangler : Node
{
    public static RealityTangler Instance { get; private set; }

    [Export] public float originResetThreshold = 100;
    [Export] public Vector3 originOffset = Vector3.Zero;

    // Planets, crafts, and whatnot should like and subscribe to this
    [Signal] public delegate void OriginResetEventHandler();
    [Signal] public delegate void OrbitProcessEventHandler();
    [Signal] public delegate void ScaledProcessEventHandler();

    public override void _Ready()
    {
        Instance = this;
    }

    // Lotsa EVIL stuff
    public override void _Process(double delta)
    {
        // Orbits uhh
        Process();

        Node3D activeThing = ActiveSave.Instance.activeThing;
        if (activeThing != null)
        {
            // We don't need the square root of this anyways
            double originDistance = activeThing.GlobalPosition.DistanceSquaredTo(Vector3.Zero);

            if (originDistance > originResetThreshold * originResetThreshold)
            {
                ResetOrigin(activeThing);
            }
        }
    }

    // Resets origin. Duh.
    public void ResetOrigin(Node3D relativeTo)
    {
        Vector3 focusedObjectPos = relativeTo.GlobalPosition;
        EmitSignal(SignalName.ScaledProcess);
        EmitSignal(SignalName.OrbitProcess);
        EmitSignal(SignalName.OriginReset);
        originOffset -= focusedObjectPos;

        //GD.Print("burh!!!");
        //GD.Print(originOffset);
    }

    // Eaten from OrbitManager.cs because we need all the syncing we can get
    public void Process()
    {
        EmitSignal(SignalName.ScaledProcess);
        EmitSignal(SignalName.OrbitProcess);

        OrbitRendererManager.Instance.UpdateOrbitRenderers();

        //foreach (CelestialBody cBody in PlanetSystem.Instance.celestialBodies)
        //{
            // Force many things to update because I don't know
            //ScaledSpace.Instance.ForceUpdate();

            //cBody.ProcessOrbitalPosition();
            //cBody.scaledSphere.truePosition = cBody.GlobalPosition; //cBody.cartesianData.position.GetPosYUp();
            //cBody.scaledSphere.ForceUpdate();
        //}
    }
}

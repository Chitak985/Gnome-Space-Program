using Godot;
using System;

/* 
Everything "reality-breaking" should ideally be done here.

That syncs stuff like floating origin, inverse rotation, velocity frames, all that BS.
Exceptions may include ScaledSpace and the OrbitManager 
(Stuff within OrbitManager might need to be moved here..? or vice-versa depends on how angry I am)
*/
public partial class RealityTangler : Node
{
    public static RealityTangler Instance { get; private set; }
    public static readonly string classTag = "([color=darkred]RealityTangler[color=white])";

    [Export] public float originResetThreshold = 100;
    [Export] public Vector3 PlanetaryOffset { get; private set;} // Only the planet's offset
    [Export] public Vector3 OriginOffset { get; private set;} // planetaryOffset + local position
    [Export] public Vector3 ReferenceFrameOriginOffset { get; private set;} // Whatever the hell this is

    // Universe will rotate around this, but it won't if it's null.
    public CelestialBody RotatingReferenceFrame { get; private set;}

    // Planets, crafts, and whatnot should like and subscribe to this
    [Signal] public delegate void OriginResetEventHandler();
    [Signal] public delegate void OrbitProcessEventHandler();
    [Signal] public delegate void CameraProcessEventHandler();
    [Signal] public delegate void ScaledProcessEventHandler();
    [Signal] public delegate void ReferenceFrameChangedEventHandler();

    public override void _Ready()
    {
        Instance = this;
    }

    // Lotsa EVIL stuff
    public override void _Process(double delta)
    {
        // Orbits uhh
        Process();

        // Reference frame nonsense
        //UpdateRotatingFrame();
    }

    // Resets origin. Duh.
    public void ResetOrigin(Node3D relativeTo)
    {
        Vector3 focusedObjectPos = relativeTo.GlobalPosition;
        EmitSignal(SignalName.ScaledProcess);
        EmitSignal(SignalName.OrbitProcess);
        EmitSignal(SignalName.OriginReset);
        OriginOffset -= focusedObjectPos;

        //GD.Print("burh!!!");
        //GD.Print(originOffset);
    }

    // Eaten from OrbitManager.cs because we need all the syncing we can get
    private void Process()
    {
        OrbitRendererManager.Instance.UpdateOrbitRenderers();
        // Orbits will not work unless you process them twice for some fucking reason
        EmitSignal(SignalName.OrbitProcess);
        switch (StateManager.Instance.CurrentGameState)
        {
            case StateManager.GameState.Flight:
                Craft activeCraft = StateManager.Instance.CurrentFlightState.activeCraft;
                if (activeCraft != null)
                {
                    // We don't need the square root of this anyways
                    //double originDistance = activeCraft.GlobalPosition.DistanceSquaredTo(Vector3.Zero);

                    //if (originDistance > originResetThreshold * originResetThreshold)
                    //{
                        //ResetOrigin(activeCraft);
                    //}
                    CelestialBody cBody = activeCraft.OrbitDriver.ParentCBody;
                    PlanetaryOffset = cBody.AbsolutePosition;
                    OriginOffset = cBody.AbsolutePosition;
                    EmitSignal(SignalName.OrbitProcess);
                }
                break;
            case StateManager.GameState.Colony:
                // Just recenter based off the colony's parent position
                if (StateManager.Instance.CurrentColonyState.activeColony != null)
                {
                    Colony colony = StateManager.Instance.CurrentColonyState.activeColony;
                    CelestialBody cBody = colony.parentBody;

                    PlanetaryOffset = cBody.AbsolutePosition;
                    ReferenceFrameOriginOffset = cBody.GetAbsolutePositionOfPoint(colony.position);
                    OriginOffset = cBody.AbsolutePosition + colony.position;
                    EmitSignal(SignalName.OrbitProcess);
                }
                break;
            default:
                OriginOffset = Vector3.Zero; // We once again panic because what the hell
                break;
        }

        EmitSignal(SignalName.CameraProcess);
        //FlightCamera.Instance.Update();
        EmitSignal(SignalName.ScaledProcess);

        UpdateRotatingFrame();

        //foreach (CelestialBody cBody in PlanetSystem.Instance.celestialBodies)
        //{
            // Force many things to update because I don't know
            //ScaledSpace.Instance.ForceUpdate();

            //cBody.ProcessOrbitalPosition();
            //cBody.scaledSphere.truePosition = cBody.GlobalPosition; //cBody.cartesianData.position.GetPosYUp();
            //cBody.scaledSphere.ForceUpdate();
        //}
    }

    // Makes the universe rotate around this celestial body
    // Sets the reference frame to non-rotating inertial when no cBody is supplied
    public void SwitchReferenceFrame(CelestialBody cBody = null)
    {
        Logger.Print($"{classTag} Switching reference frame to {cBody}");

        // Just reset all of them
        foreach (CelestialBody c in CelestialBodyManager.Instance.CelestialBodies)
        {
            c.TopLevel = false;
        }

        if (cBody != null)
        {
            RotatingReferenceFrame = cBody;
            
            cBody.TopLevel = true;
        }else{
            RotatingReferenceFrame = null;
        }

        EmitSignal(SignalName.ReferenceFrameChanged);
    }

    private void UpdateRotatingFrame()
    {
        if (RotatingReferenceFrame != null)
        {
            Transform3D trans = new()
            {
                Basis = RotatingReferenceFrame.AbsoluteTransform.Basis.Inverse()
            };

            CelestialBodyManager.Instance.GlobalTransform = trans;

            // Just set the position after doing all that transform stuff, because I can.
            CelestialBodyManager.Instance.GlobalPosition = RotatingReferenceFrame.GlobalPosition;
        }else{
            // Keep all the planets at 0,0,0 (and rotated to 0,0,0)
            CelestialBodyManager.Instance.Position = Vector3.Zero;
            CelestialBodyManager.Instance.Rotation = Vector3.Zero;
        }

        // Determine if we gotta switch (based on altitude IF we are a shipy)
        if (StateManager.Instance.CurrentGameState == StateManager.GameState.Flight)
        {
            Craft craft = StateManager.Instance.CurrentFlightState.activeCraft;
            double altitude = craft.OrbitDriver.CartState.elements.position.DistanceTo(Vector3.Zero) - craft.OrbitDriver.ParentCBody.Config.properties.radius;

            if (altitude > craft. OrbitDriver.ParentCBody.Config.properties.inverseRotAltitude)
            {
                if (RotatingReferenceFrame != null) SwitchReferenceFrame(null); // Go into a non-rotating frame
            }
            else
            {
                if (RotatingReferenceFrame != craft.OrbitDriver.ParentCBody)
                {
                    SwitchReferenceFrame(craft.OrbitDriver.ParentCBody);
                }
            }
        }
    }
}

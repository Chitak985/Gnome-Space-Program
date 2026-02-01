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
    [Export] public Vector3 originOffset = Vector3.Zero;

    // Universe will rotate around this, won't if it's null.
    public CelestialBody activeReferenceFrame;

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
        originOffset -= focusedObjectPos;

        //GD.Print("burh!!!");
        //GD.Print(originOffset);
    }

    // Eaten from OrbitManager.cs because we need all the syncing we can get
    private void Process()
    {
        EmitSignal(SignalName.OrbitProcess);

        UpdateRotatingFrame();

        switch (StateManager.Instance.gameState)
        {
            case StateManager.GameState.Flight:
                Craft activeCraft = StateManager.Instance.flightState.activeCraft;
                if (activeCraft != null)
                {
                    // We don't need the square root of this anyways
                    double originDistance = activeCraft.GlobalPosition.DistanceSquaredTo(Vector3.Zero);

                    if (originDistance > originResetThreshold * originResetThreshold)
                    {
                        ResetOrigin(activeCraft);
                    }
                }
                break;
            case StateManager.GameState.Colony:
                // Just recenter based off the colony's parent position
                if (StateManager.Instance.colonyState.activeColony != null)
                {
                    Colony colony = StateManager.Instance.colonyState.activeColony;
                    CelestialBody cBody = colony.parentBody;

                    Vector3 addedPosition = cBody.cartesianData.position + colony.position;

                    originOffset = -addedPosition;
                    
                    EmitSignal(SignalName.OrbitProcess);
                }
                break;
            default:
                originOffset = Vector3.Zero; // We once again panic because what the hell
                break;
        }

        //FlightCamera.Instance.Update();
        EmitSignal(SignalName.ScaledProcess);

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

    // Makes the universe rotate around this celestial body
    // Sets the reference frame to be global when no cBody is supplied
    public void SwitchReferenceFrame(CelestialBody cBody = null)
    {
        Logger.Print($"{classTag} Switching reference frame to {cBody}");

        // Just reset all of them
        foreach (CelestialBody c in PlanetSystem.Instance.celestialBodies)
        {
            c.TopLevel = false;
        }

        if (cBody != null)
        {
            activeReferenceFrame = cBody;
            
            cBody.TopLevel = true;
        }else{
            activeReferenceFrame = null;

        }
    }

    private void UpdateRotatingFrame()
    {
        if (activeReferenceFrame != null)
        {
            Node3D localPlanets = PlanetSystem.Instance.localSpacePlanets;

            Transform3D trans = new()
            {
                Basis = activeReferenceFrame.cachedTransform.Basis.Inverse()
            };

            localPlanets.GlobalTransform = trans;

            // Just set the position after doing all that transform stuff, because I can.
            localPlanets.GlobalPosition = activeReferenceFrame.GlobalPosition;
        }else{
            // Keep all the planets at 0,0,0 (and rotated to 0,0,0)
            PlanetSystem.Instance.localSpacePlanets.Position = Vector3.Zero;
            PlanetSystem.Instance.localSpacePlanets.Rotation = Vector3.Zero;
        }
    }
}

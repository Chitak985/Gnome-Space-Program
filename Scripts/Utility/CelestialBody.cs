using Godot;
using System;
using System.Collections.Generic;

public partial class CelestialBody : Node3D
{
    // General info
    public string name;
    public bool focusOnload;
    public double mass;
    public double geeASL;
    public double radius;
    public Vector3 originPos;

    // Rotation info
    public double initialRot;
    public double rot;
    public double rotPeriod;
    public Vector3 tilt;
    // Really just stores the rotation but okay I guess
    public Transform3D cachedTransform;

    // Axial tilt nodes
    public Node3D pivot;
    public Node3D gimbal;

    // Orbital info
    public string parentName;
    public Orbit orbit;
    public CartesianData cartesianData;

    public List<CelestialBody> childPlanets = [];

    // Procedural info
    public TerrainGen pqsSphere;
    public ScaledObject scaledSphere;
    public List<Node> pqsMods;

    // Miscellaneous info
    public bool isRoot; // only ONE body per save should ever have this be true!
    public string configPath;

    // DEBUG
    public MeshInstance3D debugOrb;

    public void CreateDebugOrb(Node3D parent)
    {
        debugOrb = new MeshInstance3D();
        debugOrb.Mesh = new SphereMesh();
        parent.AddChild(debugOrb);
    }

    public override void _Process(double delta)
    {
        // Propagate the cBody's orbit

        //ProcessOrbitalPosition();

        //scaledSphere.truePosition = GetPosYUp(cartesianData.position);

        //pivot.Rotation += new Vector3(0, 0.01, 0);
    }

    // Process the cBody orbital positioning calculations. Used by RealityTangler to "force" repositioning to avoid jitter.
    public void ProcessTransform()
    {
        if (orbit != null)
        {
            orbit.trueAnomaly = Conics.TimeToTrueAnomaly(orbit, ActiveSave.Instance.saveTime, 0) + orbit.trueAnomalyAtEpoch;
            CartesianData data = Conics.ElemToCart(orbit);
            cartesianData.position = data.position + orbit.parent.cartesianData.position;
            cartesianData.velocity = data.position;
            //GD.Print(SaveManager.Instance.saveTime);
            //GD.Print($"{cartesianData.position.X}, {cartesianData.position.Y}, {cartesianData.position.Z}");
        }

        // Uh
        originPos = cartesianData.position + RealityTangler.Instance.originOffset;

        // Modify originPos such that the active planet is at the world origin
        //if (ActiveSave.Instance.activePlanet != null)
        //    originPos -= ActiveSave.Instance.activePlanet.cartesianData.position;

        Position = originPos;

        //Logger.Print($"{name} {Position}");

        scaledSphere.truePosition = GlobalPosition; //cBody.cartesianData.position.GetPosYUp();
        scaledSphere.altPosition = Position;
        scaledSphere.ForceUpdate();

        // Update rotation
        rot = Math.Tau * (ActiveSave.Instance.saveTime / rotPeriod); //ActiveSave.Instance.saveTime * rotPeriod;

        Transform3D trans = new()
        {
            Basis = Basis.FromEuler(new Vector3(0, rot, 0))
        };

        // Don't rotate if we're the active reference frame
        if (RealityTangler.Instance.activeReferenceFrame != this)
        {
            gimbal.Transform = trans;
        }else{
            gimbal.GlobalRotation = Vector3.Zero;           
        }

        // Update cached trash
        cachedTransform.Basis = pivot.Transform.Basis * trans.Basis;

        // Update scaled mesh rotation because we'll only be seeing celestial bodies rotate anyways (though maybe change this in the future?)
        if (RealityTangler.Instance.activeReferenceFrame != this)
        {
            scaledSphere.Rotation = cachedTransform.Basis.GetEuler();
        }else{
            // Rotate the active body only if we're looking at it in the map view
            if (FlightCamera.Instance.inMap)
            {
                scaledSphere.Rotation = cachedTransform.Basis.GetEuler();
            }else{
                scaledSphere.Rotation = Vector3.Zero;
            }
        }
    }

    public void InitializeSelf()
    {
        // Create gimbals and pivots for axial tilt and all that other jazz
        pivot = new();
        AddChild(pivot);
        pivot.RotationDegrees = tilt;
        pivot.Name = "Pivot";

        gimbal = new();
        pivot.AddChild(gimbal);
        gimbal.Name = "Gimbal";

        pqsSphere = new TerrainGen
        {
            cBody = this,
            runInSeparateThread = false,
            radius = (float)radius,
            Name = "PQS"
        };
        gimbal.AddChild(pqsSphere);
    }

    public void ResetOrigin()
    {
        // Just to prevent jitter
        ProcessTransform();
    }

    public override string ToString()
    {
        return name;
    }
}

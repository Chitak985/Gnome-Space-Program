using Godot;
using System;
using System.Collections.Generic;

public partial class CelestialBody : Node3D
{
    // General info
    public string cBodyName;
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

    // Light info (This might not exist!)
    public CBodyLight light; // This updates independently of the cBody

    // Orbital info
    public string parentName;
    public Orbit orbit;
    public CartesianData cartesianData;

    public List<CelestialBody> childPlanets = [];

    // Procedural info
    public TerrainGen pqsSphere;
    public ScaledObject scaledObject;
    public MapObject mapObject;
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
            orbit.trueAnomaly = Conics.TimeToTrueAnomaly(orbit, ActiveSave.Instance.SaveTime, 0) + orbit.trueAnomalyAtEpoch;
            CartesianData data = Conics.ElemToCart(orbit);
            cartesianData.position = data.position + orbit.parent.cartesianData.position;
            cartesianData.velocity = data.position;
            //GD.Print(SaveManager.Instance.saveTime);
            //GD.Print($"{cartesianData.position.X}, {cartesianData.position.Y}, {cartesianData.position.Z}");
        }

        // Uh
        if (RealityTangler.Instance.activeReferenceFrame == this)
        {
            originPos = cartesianData.position - RealityTangler.Instance.originOffset; 
        }else{
            originPos = cartesianData.position - RealityTangler.Instance.planetaryOffset;
        }
        

        Position = originPos;

        //Logger.Print($"{name} {Position}");

        scaledObject.truePosition = GlobalPosition; //cBody.cartesianData.position.GetPosYUp();

        mapObject.truePosition = cartesianData.position;
        mapObject.Rotation = cachedTransform.Basis.GetEuler();

        // Update rotation
        rot = Math.Tau * (ActiveSave.Instance.SaveTime / rotPeriod); //ActiveSave.Instance.saveTime * rotPeriod;

        Transform3D trans = new()
        {
            Basis = Basis.FromEuler(new Vector3(0, rot, 0))
        };

        // Update cached trash
        cachedTransform.Basis = pivot.Transform.Basis * trans.Basis;

        //Logger.Print(this);
        //Logger.Print((PlanetSystem.Instance.localSpacePlanets.Transform.Basis * cachedTransform.Basis).GetEuler());
        //Logger.Print(gimbal.GlobalTransform.Basis.GetEuler());

        // Don't rotate if we're the active reference frame
        if (RealityTangler.Instance.activeReferenceFrame != this)
        {
            gimbal.Transform = trans;
            scaledObject.Rotation = gimbal.GlobalTransform.Basis.GetEuler();
        }else{
            gimbal.GlobalRotation = Vector3.Zero;
            scaledObject.Rotation = Vector3.Zero;
        }
    }

    public void InitializeSelf()
    {
        // Scaled space and map view
        scaledObject = new() { Name = $"{cBodyName}_Scaled" };
        ScaledSpace.Instance.AddChild(scaledObject);

        mapObject = new() { Name = $"{cBodyName}_Map" };
        MapView.Instance.AddChild(mapObject);
        MapView.Instance.AddMapIcon(mapObject);

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

    // Gets the global position of a point on the planet (factoring in rotation)
    public Vector3 GetGlobalPositionOfPoint(Vector3 point)
    {
        Transform3D trans = new()
        {
            Origin = point
        };;

        Transform3D finalTrans = cachedTransform * trans;

        return finalTrans.Origin;
    }

    public override string ToString()
    {
        return cBodyName;
    }
}

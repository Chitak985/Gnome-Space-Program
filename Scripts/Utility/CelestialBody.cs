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
    }

    // Process the cBody orbital positioning calculations. Used by RealityTangler to "force" repositioning to avoid jitter.
    public void ProcessOrbitalPosition()
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

        // Modify originPos such that the active planet is at at a the world origin
        if (ActiveSave.Instance.activePlanet != null)
            originPos -= ActiveSave.Instance.activePlanet.cartesianData.position;

        Position = originPos;

        scaledSphere.truePosition = GlobalPosition; //cBody.cartesianData.position.GetPosYUp();
        scaledSphere.ForceUpdate();
    }

    public void ResetOrigin()
    {
        // Just to prevent jitter
        ProcessOrbitalPosition();
    }

    public override string ToString()
    {
        return name;
    }
}

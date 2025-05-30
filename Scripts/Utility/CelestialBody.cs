using Godot;
using System;
using System.Collections.Generic;

public partial class CelestialBody : Node
{
    // General info
    public string name;
    public double mass;
    public double geeASL;
    public double radius;

    // Orbital info
    public string parentName;
    public Orbit orbit;
    public CartesianData cartesianData;

    public List<CelestialBody> childPlanets;

    // Procedural info
    public List<PlanetNoise> pqsNoiseLayers;

    // Miscellaneous info
    public string configPath;

    // DEBUG
    public MeshInstance3D debugOrb;

    public void CreateDebugOrb(Node3D parent)
    {
        debugOrb = new MeshInstance3D();
        debugOrb.Mesh = new SphereMesh();
        parent.AddChild(debugOrb);
    }
}

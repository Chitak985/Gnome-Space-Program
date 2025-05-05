using Godot;
using System;
using System.Collections.Generic;

public partial class CelestialBody : Node
{
    public double mass;
    public Orbit orbit;

    // DEBUG
    public MeshInstance3D debugOrb;

    public void CreateDebugOrb(Node3D parent)
    {
        debugOrb = new MeshInstance3D();
        debugOrb.Mesh = new SphereMesh();
        parent.AddChild(debugOrb);
    }
}

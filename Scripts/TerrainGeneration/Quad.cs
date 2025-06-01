using Godot;
using System.Collections.Generic;

public partial class Quad : Node
{
    // other quad definitions
    public List<Quad> children;

    // subdivision definition
    public int detailLevel;
    public bool readyToSubdivide;

    // positioning
    public Vector3 position;
    public Vector3 scale;
    public Vector3 centerPosition;
    public Basis basis;

    // visuals
    public Mesh mesh;
    public Mesh largeMesh; // this dont change either (DONT ASSIGN ARRAY MESH OR EVERYTHING EXPLODES)
    public StaticBody3D renderedMesh;
    public bool rendered = false;

    // mesh data
    public Godot.Collections.Array meshData;

    // THIS SHOULD NEVER GET REASSIGNED OR CHANGED!
    public Godot.Collections.Array originalMeshData;
    public Godot.Collections.Array largeMeshData;

    // collision data
    public HeightMapShape3D collider;
    public HeightMapShape3D originalCollider;
    public Vector3 colliderRotation;
}

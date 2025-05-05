using Godot;
using System.Collections.Generic;

public partial class ScaledObject : Node
{
    public Vector3 truePosition;
    public Vector3 offsetPosition;
    
    public Node3D associatedNode;
    public float objectRadius;
}

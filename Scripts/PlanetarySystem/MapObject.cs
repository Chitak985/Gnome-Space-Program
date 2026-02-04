using Godot;
using System;

// Same stuff as scaled space maybe bad coding practice but i dont care these need to stay distinct
public partial class MapObject : Node3D
{
    public Vector3 truePosition;
    public Vector3 originalScale = Vector3.One;
    public Node3D counterpart;
}

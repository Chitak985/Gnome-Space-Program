using Godot;
using System;

public partial class LocalSpace : Node3D
{
    public static LocalSpace Instance { get; private set; }

    [Export] public SubViewport Viewport { get; private set; }
    [Export] public Camera3D Camera { get; private set; }
    [Export] public Node3D Planets { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}

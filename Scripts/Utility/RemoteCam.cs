using Godot;
using System;

public partial class RemoteCam : Node3D
{
	// Those
	[Export] public Camera3D localCamera;
	[Export] public Camera3D scaledCamera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

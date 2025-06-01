using Godot;
using System;

public partial class Planet : Node3D
{
	// Shows up as 0 in the editor anyways
	private const float gravConstant = 6.674e-11F; // 0.00000000006674
	// primary values
	[Export] public float mass = -1;
	[Export] public float geeASL = -1;
	[Export] public float radius;
	// pqs definitions
	//[Export] int scaledLevel = 5;
	//[Export] int minLevel = 4;
	//[Export] int maxLevel = 15;
	//[Export] Material terrainMaterial;
	// ocean
	//[Export] bool hasOcean;
	//[Export] Material oceanMaterial;
	// noises
    ///[Export] public FastNoiseLite baseNoise;
    //[Export] public float baseNoiseDeformity;
    //[Export] public FastNoiseLite detailNoise;
    //[Export] public float detailNoiseDeformity;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (mass < 0)
		{
			mass = geeASL/gravConstant*Mathf.Pow(radius, 2f);
			// object mass is 1 so it is ignored.
			// float force = gravConstant * planetMass * objectMass / Mathf.Pow(distance, 2);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	//public override void _Process(double delta)
	//{
	//}
}

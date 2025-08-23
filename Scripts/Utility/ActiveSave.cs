using Godot;
using System;
using System.Collections.Generic;

// All major save data is stored here (crafts, celestials, etc)

public partial class ActiveSave : Node3D
{
	public static readonly string classTag = "([color=orange]PlanetSystem[color=white])";
	public static ActiveSave Instance;
	[Export] public PlanetSystem planetSystem;

	// The great dictionary.planetSystem
	public Dictionary<string, Variant> saveParams;

	// This should always be 1.0 upon loading!
	[Export] public double timeSpeed = 1;
	// In milliseconds
	public double saveTime;

	public override void _Ready()
	{
		GD.PrintRich($"{classTag} Active save starting...");
		Instance = this;
		foreach (KeyValuePair<string, Variant> param in saveParams)
		{
			GD.Print(param);
		}
		GD.PrintRich($"{classTag} Active save ready for init!");
	}

	// Start up all vital systems such as the planet system and whatnot
	public void InitSave()
	{
		// We first initialize the planets
		GD.PrintRich($"{classTag} Starting PlanetSystem");
		PlanetSystem.Instance = planetSystem; // Set instance, very yucky but oh well.
		Dictionary<string, PlanetPack> planetPacks = SaveManager.GetPlanetPacks();
		string chosenRootSystem = (string)saveParams["Celestial Bodies/Root System"];
		// !!! ADD EXTRA SYSTEMS IMPLEMENTATION WHEN RELEVANT !!!
		List<string> planetPackPaths = [];
		planetPackPaths.Add(planetPacks[chosenRootSystem].path);
		planetSystem.InitSystem(planetPackPaths);
	}

	public override void _Process(double delta)
	{
		// Increment time since save creation (for orbital calculations mostly)
		saveTime += delta * 1000 * timeSpeed / 1000;

		// Set physics speed to match time speed
		Engine.TimeScale = timeSpeed;
	}
}

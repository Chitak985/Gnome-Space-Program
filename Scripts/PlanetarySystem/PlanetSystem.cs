using Godot;
using System.Collections.Generic;

public partial class PlanetSystem : Node3D
{
	public static PlanetSystem Instance;

	public static readonly string ConfigPath = "res://GameData";

	public List<CelestialBody> celestialBodies;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreateSystem(GetPlanetConfigs(ConfigPath));
	}

	public void CreateSystem(List<string> configs)
	{
		foreach (string str in configs)
		{
			ParseConfig(str);
		}
	}

	public CelestialBody FindCBodyByName(string name)
	{
		foreach (CelestialBody cBody in celestialBodies)
		{
			if (cBody.name == name) return cBody;
		}
		GD.PushWarning($"Couldn't find CelestialBody with name '{name}'");
		return null;
	}

	// Recursive function to get all planet configs in a path
	public static List<string> GetPlanetConfigs(string path)
	{
		List<string> files = [];
		DirAccess dir = DirAccess.Open(path);
		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (fileName != "")
			{
				string filePath = path + "/" + fileName;

				ConfigFile configFile = new();
				Error err = configFile.Load(filePath);
				
				if (err == Error.Ok && (string)configFile.GetValue("Metadata", "configType") == "CBody")
				{
					GD.Print($"Config found: {filePath}");
					files.Add(filePath);
				}
				
				List<string> subFiles = GetPlanetConfigs(filePath);
				if (subFiles.Count > 0) files.AddRange(subFiles);

				fileName = dir.GetNext();
			}
		}
		return files;
	}

	public void ParseConfig(string path)
	{
		ConfigFile config = new();
		Error err = config.Load(path);

		if (err != Error.Ok)
		{
			GD.PushWarning($"Config file {path} could not be loaded.");
			return;
		}

		CelestialBody cBody = new();
		
		cBody.name = (string)config.GetValue("Properties", "name");
		// only mass or geeASL is required, the unassigned one will be calculated based off one of the values.
		cBody.mass = (double)config.GetValue("Properties", "mass", -1);
		cBody.geeASL = (double)config.GetValue("Properties", "geeASL", -1);
		cBody.radius = (double)config.GetValue("Properties", "radius");

		if (cBody.mass < 0)
		{
			cBody.mass = cBody.geeASL*PatchedConics.EarthGravity*Mathf.Pow(cBody.radius, 2f)/PatchedConics.GravConstant;
		}
		if (cBody.geeASL < 0)
		{
			cBody.geeASL = cBody.mass*PatchedConics.GravConstant/Mathf.Pow(cBody.radius, 2f) / PatchedConics.EarthGravity;
		}

		// the "parent" parameter is set in a different function because the parent might be parsed before this one
		if ((bool)config.GetValue("Orbit", "enabled", true))
		{
			cBody.orbit = new Orbit{
				semiMajorAxis = 10,
				inclination = 0.1,
				eccentricity = 1.1,
				argumentOfPeriapsis = 0,
				longitudeOfAscendingNode = 0,
				trueAnomaly = 0
			};
		}

		// Loop through all sections for noise
		string[] allSects = config.GetSections();
		for (int i = 0; i < allSects.Length; i++)
		{
			string noiseSect = allSects[i];
			
		}

		GD.Print($"Name: {cBody.name}");
		GD.Print($"Mass: {cBody.mass}");
		GD.Print($"GeeASL: {cBody.geeASL}");
		GD.Print($"Radius: {cBody.radius}");
		GD.Print($"Orbit: {cBody.orbit}");
	}
}

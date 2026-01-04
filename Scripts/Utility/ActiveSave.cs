using Godot;
using System;
using System.Collections.Generic;

// All major save data is stored here (crafts, celestials, etc)

public partial class ActiveSave : Node3D
{
	public static readonly string classTag = "([color=orange]ActiveSave[color=white])";
	public static ActiveSave Instance { get; private set; }
	[Export] public PlanetSystem planetSystem;
	[Export] public PartManager partManager;
	[Export] public ColonyManager colonyManager;
	[Export] public FlightCamera flightCam;
	[Export] public Camera3D localCamera;

    // Spaces
    [Export] public Node3D localSpace;

    // Every surface base/colony
    public List<Colony> colonies = [];

	// The great dictionary
	public Dictionary<string, Variant> saveParams;

	// This should always be 1.0 upon loading!
	[Export] public double timeSpeed = 1;
	// In milliseconds
	public double saveTime;

	public void ReportSelf()
	{
		Logger.Print($"{classTag} Here!");
	}

	public override void _Ready()
	{
		Logger.Print($"{classTag} Active save starting...");

		Instance = this;
		SingletonRegistry.Register(this); // Register self

		foreach (KeyValuePair<string, Variant> param in saveParams)
		{
			Logger.Print(param);
		}
		Logger.Print($"{classTag} Active save ready for init!");
	}

	// Start up all vital systems such as the planet system and whatnot
	public void InitSave()
	{
		// We first initialize the planets
		Logger.Print($"{classTag} Starting PlanetSystem");
		Dictionary<string, PlanetPack> planetPacks = SaveManager.GetPlanetPacks();
		string chosenRootSystem = (string)saveParams["Celestial Bodies/Root System"];
		// !!! ADD EXTRA SYSTEMS IMPLEMENTATION WHEN RELEVANT !!!
		List<string> planetPackPaths = [];
		planetPackPaths.Add(planetPacks[chosenRootSystem].path);
		planetSystem.InitSystem(planetPackPaths);
		InitCamera();

		// Handle part packs
		Dictionary<string, PartPack> partPacks = SaveManager.GetPartPacks();
        List<PartPack> pPacksToLoad = [];
        // Yes hello welcome to hell.
        foreach (KeyValuePair<string, PartPack> partPack in partPacks)
		{
			// Ideally we don't want to use display names for this
			if (((Godot.Collections.Array<string>)saveParams["Parts/Selected Part Packs"]).Contains(partPack.Value.displayName))
			{
                //Logger.Print($"{classTag} Loading part pack '{partPack.Value.displayName}'...");
                pPacksToLoad.Add(partPack.Value);
            }
		}

		Logger.Print($"{classTag} Starting PartManager");
        partManager.LoadPartModules();
        // Start it
        partManager.LoadPartPacks(pPacksToLoad);

		Logger.Print($"{classTag} Starting ColonyManager");
        // Handle Colonies (blueprints)
		foreach (KeyValuePair<string, PlanetPack> pack in planetPacks)
		{
			colonies.AddRange(colonyManager.ParseColonies(pack.Value.path, true));
		}
    }

	public void InitCamera()
	{
		// Fall back to root body if no focus on load body has been set
		CelestialBody focusBody;
		if (planetSystem.focusOnLoadBody != null)
		{
			focusBody = planetSystem.focusOnLoadBody;
		}else{
			focusBody = planetSystem.rootBody;
		}

		flightCam.TargetObject(focusBody);

		// Default context menu
		Godot.Collections.Dictionary info = new()
        {
            { "planet", focusBody }
        };
        MapUI.Instance.contextMenus.OpenMenu("PlanetMenu", info, true);
	}

	public override void _Process(double delta)
	{
		// Increment time since save creation (for orbital calculations mostly)
		saveTime += delta * 1000 * timeSpeed / 1000;

		// Set physics speed to match time speed
		//Engine.TimeScale = timeSpeed;
	}
}

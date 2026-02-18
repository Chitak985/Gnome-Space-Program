using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

// Helper singleton for managing colonies
public partial class ColonyManager : Node
{
    public static readonly string classTag = "([color=LEMON_CHIFFON]ColonyManager[color=white])";
    public static ColonyManager Instance { get; private set; }
    [Export] public PackedScene iconPrefab;
    [Export] public Control iconParent;
    
    public List<Colony> colonies = [];

    public override void _Ready()
    {
        Instance = this;
        Logger.Print($"{classTag} ColonyManager Ready!");
    }

    public void Initialize(System.Collections.Generic.Dictionary<string, PlanetPack> planetPacks)
    {
        // Handle Colonies (blueprints)
		foreach (KeyValuePair<string, PlanetPack> pack in planetPacks)
		{
			colonies.AddRange(ParseColonies(pack.Value.path, true));
		}
    }

    public List<Colony> ParseColonies(string path, bool blueprint = false)
    {
        Logger.Print($"{classTag} Parsing path: {path}");
        List<Colony> packColonies = [];
        List<string> configs = ConfigUtility.GetConfigs($"{ConfigUtility.GameData}/{path}", "Base");
        Logger.Print($"{classTag} {configs.Count}");
        foreach (string cfg in configs)
        {
            Colony colony = ParseColony(cfg, blueprint);
            if (colony != null)
            {
                packColonies.Add(colony);
            }
        }

        return packColonies;
    }
    public Colony ParseColony(string configPath, bool blueprint = false)
    {
        Logger.Print($"{classTag} Parsing colony: {configPath}");
        Dictionary data = ConfigUtility.ParseConfig(configPath);

        // This isn't what I wanted get it outta here
        if ((bool)data["blueprint"] != blueprint)
        {
            return null;
        }

        Colony colony = new()
        {
            name = (string)data["name"],
            Name = (string)data["name"]
        };

        // Grrrrr...
        if (ConfigUtility.TryGetArray("position", data, out Godot.Collections.Array posArr))
        {
            colony.position = new Vector3((double)posArr[0], (double)posArr[1], (double)posArr[2]);
        }
        
        if (ConfigUtility.TryGetArray("rotation", data, out Godot.Collections.Array rotArr))
        {
            colony.rotation = new Vector3((double)rotArr[0], (double)rotArr[1], (double)rotArr[2]);
        }

        // Add all buildings
        if (ConfigUtility.TryGetDictionary("buildings", data, out Dictionary buildingDict))
        {
            colony.savedRootParts = GetColonyParts(buildingDict, true);
            colony.savedParts = GetColonyParts(buildingDict);
        }

        // Yeah.....
        colony.Position = colony.position;
        colony.RotationDegrees = colony.rotation;

        // Initial base is whatever the user loads into when opening a save
        colony.initialBase = data.TryGetValue("initialBase", out var initialBase) && (bool)initialBase;

        // Add to the planet and also add a map icon
        CelestialBody parent = PlanetSystem.Instance.FindCBodyByName((string)data["parent"]);
        colony.parentBody = parent;
        parent.gimbal.AddChild(colony);

        colony.mapObject = new() {Name = $"{colony.name} Map"};
        //MapView.Instance.AddChild(colony.mapObject);
        parent.mapObject.AddChild(colony.mapObject);
        colony.mapObject.Position = colony.position;
        colony.mapObject.counterpart = colony;

        //if (ConfigUtility.TryGetDictionary("buildings", data, out Dictionary buildings))
        //{
        //    colony.savedParts = buildings;
        //}

        // Load it here for now.. We can change this later.
        colony.Load();

        return colony;
    }

    // Parse parts from config
    public static System.Collections.Generic.Dictionary<string, UnloadedPart> GetColonyParts(Dictionary dict, bool justRoot = false)
    {
        System.Collections.Generic.Dictionary<string, UnloadedPart> shit = [];
        // This is genuinely the worst shit you had to put me through godot what the fuck
        foreach (KeyValuePair<Variant, Variant> part in dict)
        {
            Dictionary partData = (Dictionary)part.Value;
            string partName = (string)part.Key;

            UnloadedPart unloadedPart = ParseColonyPart(partName, partData);

            shit.Add(partName, unloadedPart);
        }

        return shit;
    }

    public static UnloadedPart ParseColonyPart(string name, Dictionary data)
    {
        UnloadedPart unloadedPart = new()
        {
            template = PartManager.Instance.partCache[name],
        };

        if (ConfigUtility.TryGetArray("position", data, out Godot.Collections.Array posArr))
        {
            unloadedPart.position = new Vector3((float)posArr[0], (float)posArr[1], (float)posArr[2]);
        }

        if (ConfigUtility.TryGetArray("rotation", data, out Godot.Collections.Array rotArr))
        {
            unloadedPart.rotation = new Vector3((float)rotArr[0], (float)rotArr[1], (float)rotArr[2]);
        }

        // ADD ADDITIONAL DATA AND RECURSIVE PARSING LATER

        return unloadedPart;
    }
}

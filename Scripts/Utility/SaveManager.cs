using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SaveManager : Node
{
	public static readonly string classTag = "([color=red]SaveManager[color=white])";
	public static readonly string CPackMetaName = "cPackMeta";
	public static readonly string SaveParamName = "saveParameters";

	public static SaveManager Instance;

	public ActiveSave currentSave;

	public PackedScene activeSavePrefab = (PackedScene)ResourceLoader.Load("res://Scenes/ActiveSave.tscn");

	public override void _Ready()
	{
		Instance = this;
		GD.PrintRich($"{classTag} SaveManager ready!");
	}

	public void LoadSave(
		System.Collections.Generic.Dictionary<string, Variant> creationParams,
		string saveFile = null)
	{
		GD.Print($"{classTag} Clearing currently loaded save..");
		foreach (Node child in GetChildren())
		{
			child.QueueFree();
		}
		if (saveFile == null) 
		{
			GD.Print($"{classTag} Creating new save!");
			ActiveSave activeSave = activeSavePrefab.Instantiate<ActiveSave>();
			currentSave = activeSave;
			activeSave.saveParams = creationParams;
			AddChild(activeSave);
			activeSave.InitSave();
		}else{
			// Load save from file, params will be ignored and instead acquired from the saveFile.
			// NOT TOO IMPORTANT YET!
			GD.Print($"{classTag} Loading save from file {saveFile}");
		}
	}

	public static System.Collections.Generic.Dictionary<string, PlanetPack> GetPlanetPacks(string type = null)
	{
		System.Collections.Generic.Dictionary<string, PlanetPack> planetPacks = [];

		List<string> metaConfigs = ConfigUtility.GetConfigs(ConfigUtility.GameData, CPackMetaName);

		foreach (string cfgPath in metaConfigs)
		{
			Dictionary data = ConfigUtility.ParseConfig(cfgPath);

			string packType = (string)data["packType"];
			string packName = (string)data["name"];

			// ugh
			PlanetPack pack = new PlanetPack
			{
				type = packType,
				name = packName,
				path = (string)data["path"],
			};

			if (ConfigUtility.TryGetDictionary("displayData", data, out Dictionary displayData))
			{
				pack.displayName = (string)displayData["displayName"];
			}

			// Only add those of specific type or all if type isn't specified
			// Use display name for now, it shouldn't matter much though
			if (type != null)
			{
				if (packType == type) planetPacks.Add(pack.displayName, pack);
			}else{
				planetPacks.Add(pack.displayName, pack);
			}
		}

		return planetPacks;
	}

	/* 
	SUMMARY:
	Returns a dictionary of every save schema. 
	This is only ever useful for save creation as save parameters should be read from the savefile itself.
	Save parameters alone DO NOTHING! They are just variables to be used by various... things. Anything, really.
	*/
	public static System.Collections.Generic.Dictionary<string, SaveParam> GetSaveSchemas()
	{
		System.Collections.Generic.Dictionary<string, SaveParam> schemas = [];

		List<string> schemaConfigs = ConfigUtility.GetConfigs(ConfigUtility.GameData, SaveParamName);

		// Loop over every schema config FILE
		foreach (string configPath in schemaConfigs)
		{
			Dictionary schemaData = ConfigUtility.ParseConfig(configPath);

			// Loop over every schema DICTIONARY within the file! First checking if it's there, of course.
			if (ConfigUtility.TryGetArray("parameters", schemaData, out Godot.Collections.Array dict))
			{
				foreach (Dictionary scheme in dict.Select(v => (Dictionary)v))
				{
					// Initialize with the important stuff
					SaveParam saveParam = new()
					{
						name = (string)scheme["name"],
						resetOnLoad = (bool)scheme["resetOnLoad"],
						category = (string)scheme["category"],
						data = scheme["data"]
					};

					// Sadly this has to be hardcoded for now, 
					// May add some way for mods to "hook" into this part in the future.
					switch ((string)scheme["name"])
					{
						case "Root System":
							System.Collections.Generic.Dictionary<string, PlanetPack> rootSystems = 
								GetPlanetPacks("rootSystem");

							Array<string> systemList = new Array<string>();
							foreach (KeyValuePair<string, PlanetPack> pack in rootSystems)
							{
								systemList.Add(pack.Key);
							}
							saveParam.data = systemList;
							break;
						default:
							break;
					}


					// We check if it has input information, if not, then inputData remains null.
					if (ConfigUtility.TryGetDictionary("selector", scheme, out Dictionary inpDict))
					{
						saveParam.inputData = new()
						{
							selectorType = (string)inpDict["arraySelectorType"]
						};
					}

					// We check if it has input information, if not, then inputData remains null.
					if (ConfigUtility.TryGetDictionary("dependency", scheme, out Dictionary depDict))
					{
						saveParam.dependency = new()
						{
							key = (string)depDict["setting"],
							value = depDict["item"]
						};
					}

					// Throw it into the dictionary
					schemas.Add(saveParam.category + "/" + saveParam.name, saveParam);
				}
			}
		}

		return schemas;
	}
}

public struct PlanetPack
{
	public string type;
	public string name;
	public string displayName;
	public string path;
}

// Data-driven save schema for if modders want to patch in their own savegame parameters.
// Hopefully it's versatile enough. I started overthinking so I'm just gonna go with whatever this is,
public class SaveParam
{
	public string name;
	public bool resetOnLoad;
	public string category;
	public Variant data;
	public SaveCreationInput inputData;
	public SaveDependency dependency;
}

// Struct that tells the save creation UI what to do with regards to user input 
public class SaveCreationInput
{
	public Variant currentSelection;
	public string selectorType; // Non array/dictionary items will be auto determined. Can be "single" or "multiple"
}
// Basic struct to tell parsers what the save setting depends on
public class SaveDependency
{
	public string key;
	public Variant value;
}

using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class SaveManager : Control
{
    // Schema for generic save parameters. Use for very simple values.
    public static System.Collections.Generic.Dictionary<string, SaveParam> genericParamSchema;

    public void CreateSave(SaveData saveData)
    {

    }

    public static List<PlanetPack> GetPlanetPacks()
    {
        List<PlanetPack> planetPacks = [];

        List<string> metaConfigs = ConfigUtility.GetConfigs(ConfigUtility.GameData, "cPackMeta");

        foreach (string cfgPath in metaConfigs)
        {
            Dictionary data = ConfigUtility.ParseConfig(cfgPath);

            // ugh
            PlanetPack pack = new PlanetPack
            {
                type = (string)data["packType"],
                name = (string)data["name"],
                path = (string)data["path"],
            };

            if (ConfigUtility.TryGetDictionary("displayData", data, out Dictionary displayData))
            {
                pack.displayName = (string)displayData["displayName"];
            }

            planetPacks.Add(pack);
        }

        return planetPacks;
    }
}

public struct PlanetPack
{
    public string type;
    public string name;
    public string displayName;
    public string path;
}

public struct SaveData
{
    // Path to 
    public string rootPSystem;
}

// Data-driven save schema for if modders want to patch in their own savegame parameters.
public struct SaveParam
{
    public string name;
    public bool selectable; // Whether or not it can be selected during save creation
    public string arraySelectorType; // Not used by anything other than arrays and dictionaries. Can be "single" or "multiple"
    public Variant data;
}
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class SaveManager : Control
{
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
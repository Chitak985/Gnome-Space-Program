using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SaveManager : Control
{
    public static string CPackMetaName = "cPackMeta";
    public static string SaveParamName = "saveParameters";

    public void CreateSave(SaveData saveData)
    {

    }

    public static List<PlanetPack> GetPlanetPacks()
    {
        List<PlanetPack> planetPacks = [];

        List<string> metaConfigs = ConfigUtility.GetConfigs(ConfigUtility.GameData, CPackMetaName);

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
                    schemas.Add(saveParam.name, saveParam);
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

public struct SaveData
{
    // Path to 
    public string rootPSystem;
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
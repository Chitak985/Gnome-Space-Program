using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

// Helper class for parsing JSON configs
public partial class ConfigUtility : Node
{
    public static readonly string classTag = "([color=aaccff]ConfigUtility[color=white])";
    // GameData fallback if the game is being run in the editor
    public static readonly string GameDataFolder = "GameData";
    public static readonly string EditorGameData = "res://GameData";
    public static string GameData {get; private set;}

    public override void _EnterTree()
    {
        bool inEditor = OS.HasFeature("editor");
        
        if (inEditor)
        {
            GameData = EditorGameData;
            Logger.Print($"{classTag} Game running in editor! Using {EditorGameData}");
        }else{
            string dataDir = $"{OS.GetExecutablePath().GetBaseDir()}/{GameDataFolder}";
            GameData = dataDir;
            Logger.Print($"{classTag} Game running in executable! Using {dataDir}");
        }
    }

    public static Dictionary ParseConfig(string path)
    {
        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            
        string content = file.GetAsText();

        Json jsonFile = new();
        Error err = jsonFile.Parse(content);
        Dictionary data = (Dictionary)jsonFile.Data;

        if (err == Error.Ok)
            return data;

        Logger.Print($"Failed to parse config {path}");
        return null;
    }

    public static List<string> GetConfigs(string path, string type = null)
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

                // Check if directory is a valid JSON
                FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    string content = file.GetAsText();

                    Json jsonFile = new();
                    Error err = jsonFile.Parse(content);
                    

                    // Get the config if it matches the type or if the type is null
                    // Also check for errors but that's obviouuss
                    if (err == Error.Ok)
                    {
                        Dictionary data = (Dictionary)jsonFile.Data;

                        string configType = (string)data["configType"];

                        if(configType == type || type == null)files.Add(filePath);
                    }
                }
                
                List<string> subFiles = GetConfigs(filePath, type);
                if (subFiles.Count > 0) files.AddRange(subFiles);

                fileName = dir.GetNext();
            }
        }
        return files;
    }
    public static bool TryGetDictionary(string name, Dictionary parent, out Dictionary dict)
    {
        if (parent.TryGetValue(name, out var dih))
        {
            dict = (Dictionary)dih;
            return true;
        }
        dict = null;
        return false;
    }
    public static bool TryGetArray(string name, Dictionary parent, out Godot.Collections.Array array)
    {
        if (parent.TryGetValue(name, out var arr))
        {
            array = (Godot.Collections.Array)arr;
            return true;
        }
        array = null;
        return false;
    }

    // So that ordinary trygets don't clutter everything up
    public static Variant GetValue(string name, Dictionary parent, Variant defaultValue)
    {
        if (parent.TryGetValue(name, out var val))
        {
            return val;
        }else{
            //Logger.Print($"{classTag} Value ({name}) could not be found!!");
            return defaultValue;
        }
    }
    public static Variant GetValue(string name, Dictionary parent)
    {
        if (parent.TryGetValue(name, out var val))
        {
            return val;
        }else{
            throw new NullReferenceException($"Value {name} could not be found and has no default value.");
        }
    }
    public static Vector3 GetVector3(string name, Dictionary parent, Vector3 defaultValue)
    {
        if (TryGetArray(name, parent, out Godot.Collections.Array vector))
        {
            return new Vector3((float)vector[0],(float)vector[1],(float)vector[2]);
        }else{
            return defaultValue;
        }
    }
    public static Vector3 GetVector3(string name, Dictionary parent)
    {
        if (TryGetArray(name, parent, out Godot.Collections.Array vector))
        {
            return new Vector3((float)vector[0],(float)vector[1],(float)vector[2]);
        }else{
            throw new NullReferenceException($"Vector3 {name} could not be found and has no default value.");
        }
    }
}

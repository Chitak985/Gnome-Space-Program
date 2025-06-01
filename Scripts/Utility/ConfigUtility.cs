using Godot;
using Godot.Collections;

// Helper class for parsing JSON configs
public partial class ConfigUtility
{
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
    public static bool TryGetArray(string name, Dictionary parent, out Array array)
    {
        if (parent.TryGetValue(name, out var arr))
        {
            array = (Array)arr;
            return true;
        }
        array = null;
        return false;
    }
}

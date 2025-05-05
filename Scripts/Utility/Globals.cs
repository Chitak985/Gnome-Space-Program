using Godot;
using System;

public partial class Globals : Node
{
    // To determine if a craft is being launched or not
    public static bool Launching {get;set;}

    // Currently saved craft (Change this to a better system when craft persistence is added!)
    // Speaking of craft persistence, how will I even do that? 
    public static Craft SavedCraft {get;set;}
}

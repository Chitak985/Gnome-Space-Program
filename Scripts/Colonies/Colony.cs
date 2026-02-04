using Godot;
using System;
using System.Collections.Generic;

public partial class Colony : Node3D
{
    /*
    Colonies also use "parts" to take advantage of the attachment node system.
    In return, craft get to take advantage of stuff like launchsites and VABs.
    */

    public CelestialBody parentBody;
    public MapObject mapObject;

    public string name;
    public Vector3 position;
    public Vector3 rotation;

    public bool initialBase = false;

    public Dictionary<string, UnloadedPart> savedRootParts = []; // Only lists the saved root parts
    public Dictionary<string, UnloadedPart> savedParts = [];

    public List<Part> rootParts = [];
    public List<Part> allParts = [];

    public void Load()
    {
        // Iterate over parts
        Logger.Print($"({name}) Instantiating...");
        foreach (KeyValuePair<string, UnloadedPart> pair in savedRootParts)
        {
            string partName = pair.Key;
            UnloadedPart data = pair.Value;

            // ... Now we need to make a part manager.....
            // ... Okay now that we made a part manager.....

            Part part = data.template.Instantiate(this, anchored:true);
            allParts.Add(part);
            //part.Freeze = true;
            //part.LockRotation = true;
            part.Position = data.position;
            part.RotationDegrees = data.rotation;

            part.parentThing = this;
            part.cachedPart = data.template;

            part.enabled = true;

            part.InitPart();
        }
    }

    public void Unload()
    {
        // lol no
    }

    public Godot.Collections.Array<Part> GetPartsWithModule(Type type)
    {
        Godot.Collections.Array<Part> parts = [];
        foreach (Part part in allParts)
        {
            List<PartModule> modules = part.GetPartModules(type);
            if (modules.Count > 0) parts.Add(part);
        }
        return parts;
    }
}

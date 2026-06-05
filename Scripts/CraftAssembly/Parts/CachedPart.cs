using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class CachedPart
{
    public string name;
    public string displayName;
    public string category;
    public string pckFile;
    public string scenePath;
    public bool listedInSelector = true;
    public Dictionary config;

    // Dynamic stuff - self assigned
    public PackedScene PartScene { get; private set; }
    public Aabb PartAABB { get; private set; }

    // Run this ONLY ONCE per part!
    public void LoadAssets()
    {
        // This will actively "install" resources into the game. 
        // Follow the part modding convention PLEASE!!!!! 
        bool success = ProjectSettings.LoadResourcePack($"{ConfigUtility.GameData}/{pckFile}");

        if (!success)
        {
            Logger.Print($"(Cached {name}) Failed to load resource pack '{ConfigUtility.GameData}/{pckFile}'.");
            Logger.Print($"(Cached {name}) Attempting to forcefully load scene '{scenePath}'...");
        }

        // Errors out if the scene doesn't exist. The only reason this even tries to load anyways is for testing.
        PackedScene scene = (PackedScene)ResourceLoader.Load(scenePath);
        if (scene != null)
        {
            PartScene = scene;
            Logger.Print($"(Cached {name}) Scene loading success!");
        }else{
            Logger.Print($"(Cached {name}) Could not load part.");
        }

        GetSceneData();
    }

    // ermm......... erm.....!!!! this was meant to do something for sure
    //public void LoadModules()
    //{
        
    //}

    public Part Instantiate(Node parent, bool inEditor = false, bool anchored = false)
    {
        Logger.Print($"(Cached {name}) Instantiating...");
        Part part = (Part)PartScene.Instantiate();
        part.inEditor = inEditor;
        part.Anchor(anchored); // Anchor if we need to
        part.Name = $"{name}_{part.GetInstanceId()}";
        parent.AddChild(part);

        // Copy this to the parent
        // No longer used! Keep it here in case we need it again though
        /* 
        if (copyColliders)
        {
            foreach (CollisionShape3D collider in part.colliders)
            {
                CollisionShape3D newCollider = (CollisionShape3D)collider.Duplicate();
                parent.AddChild(newCollider);
            }
        }
        */

        return part;
    }

    // Instantiates a temporary part to get various information out of the scene
    private void GetSceneData()
    {
        Logger.Print($"(Cached {name}) Instantiating temporary extraction part");
        Part part = Instantiate(PartManager.Instance.temporaryPartDump, anchored: true);

        PartAABB = part.GetAABB();

        // We're done here.
        part.QueueFree();
    }
}

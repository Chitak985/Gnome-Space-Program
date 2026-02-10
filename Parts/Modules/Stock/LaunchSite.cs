using Godot;
using Godot.Collections;
using System;

/*
    This part module is BUILT IN to GSP.
    As such, this module should remain internal and not be compiled as a mod.
*/
public partial class LaunchSite : PartModule
{
    public string siteName;
    // Where the craft spawns
    public Node3D spawnNode;

    public override void PartInit() 
    {
        Array<float> posArray = (Array<float>)configData["spawnPos"];

        spawnNode = new() {
            Position = new(
            posArray[0],
            posArray[1],
            posArray[2])
        };

        part.AddChild(spawnNode);

        siteName = (string)configData["siteName"];
    }

    public Craft SpawnCraft(Dictionary partData, bool focus = false)
    {
        Craft craft = new();
        ActiveSave.Instance.localSpace.AddChild(craft);
        craft.Instantiate(partData);

        craft.GlobalPosition = spawnNode.GlobalPosition;

        craft.Initialize();

        // We can not focus on the craft and stay in the editor if we absolutely want to
        if (focus)
        {
            BuildingManager.Instance.ExitBuildMode(false);
            craft.SnatchFocus();
        }

        // Unleash physics and let it do its thing
        craft.Anchor(false);

        return craft;
    }
}

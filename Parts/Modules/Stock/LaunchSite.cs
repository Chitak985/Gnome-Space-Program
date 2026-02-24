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
        Vector3 originPos = Vector3.Zero;
        // Handle crafts soon for fucks sake
        if (part.parentThing is Colony colony)
        {
            CelestialBody cBody = colony.parentBody;
            originPos = cBody.GetGlobalPositionOfPoint(colony.position);
        }

        Transform3D transform = new() {
            Origin = originPos
        };

        // Defer this to craft manager
        Craft craft = CraftManager.Instance.SpawnCraft(partData, transform, focus);

        return craft;
    }
}

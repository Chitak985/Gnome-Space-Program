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

    public Vector3 GetLaunchPosition()
    {
        // Pull a position out of this heap of trash
        Vector3 parentPosition = Vector3.Zero;
        if (part.parentThing is Colony colony)
        {
            parentPosition = colony.position;
        }

        Vector3 result = spawnNode.GlobalPosition + parentPosition;

        return result;
    }

    public Craft SpawnCraft(Dictionary partData, bool focus = false)
    {
        CelestialBody cBody = null;
        Vector3 originPos = Vector3.Zero;

        // Handle crafts soon for crying out loud
        if (part.parentThing is Colony colony)
        {
            cBody = colony.parentBody;
            originPos = GetLaunchPosition();
        }

        CartesianData cartesianData = new()
        {
            parent = cBody,
            position = originPos,
            velocity = cBody.GetSurfaceRotationVelocity(originPos)
        };

        Orbit orbit = new() {
            parent = cBody,
            semiMajorAxis = 7000000,
            eccentricity = 0.8,
            inclination = 12,
            argumentOfPeriapsis = 32,
            longitudeOfAscendingNode = 11,
            trueAnomaly = 1,
            trueAnomalyAtEpoch = 0
        };

        // Send this to craft manager
        Craft craft = CraftManager.Instance.SpawnCraft(partData, orbit, focus);

        return craft;
    }
}

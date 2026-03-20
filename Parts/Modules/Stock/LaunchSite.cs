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

    // Returns a LOCAL position, this will have to be converted to an inertial frame later!
    public Vector3 GetLaunchPosition()
    {
        // Pull a position out of this heap of trash
        Vector3 result = Vector3.Zero;

        if (part.parentThing is Colony colony)
        {
            CelestialBody cBody = colony.parentBody;

            Vector3 parentPosition = colony.position;

            result = spawnNode.GlobalPosition + parentPosition;
        }

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
            position = cBody.GetGlobalPositionOfPoint(originPos),
            velocity = cBody.GetSurfaceRotationVelocity(cBody.GetGlobalPositionOfPoint(originPos))
        };

        //Orbit orbit = new() {
        //    parent = cBody,
        //    semiMajorAxis = 650000,
        //    eccentricity = 0,
        //    inclination = 0,
        //    argumentOfPeriapsis = 0,
        //    longitudeOfAscendingNode = 0,
        //    trueAnomaly = 0,
        //    trueAnomalyAtEpoch = 0
        //};

        //Orbit orbit = Conics.CartToElem(new CartesianData(){
        //    position = new Vector3(700000, 0, 0),
        //    velocity = new Vector3(0, 0, 3000),
        //    parent = cBody
        //});

        // Send this to craft manager
        Craft craft = CraftManager.Instance.SpawnCraft(partData, cartesianData, focus);

        return craft;
    }
}

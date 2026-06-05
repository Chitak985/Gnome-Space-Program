using Godot;
using Godot.Collections;

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

        Craft craft = CraftManager.Instance.CreateCraft(partData, cBody);

        Aabb craftAABB = craft.GetAABB();

        CartesianState.CartesianElements cartesianData = new()
        {
            position = cBody.GetAbsolutePositionOfPoint(originPos + cBody.GlobalPosition.DirectionTo(originPos) * craftAABB.Size.Y),
            rotation = spawnNode.GlobalRotation,
            velocity = cBody.GetSurfaceRotationVelocity(cBody.GetAbsolutePositionOfPoint(originPos))
        };

        craft.OrbitDriver.ToggleOnRailsOrbit(false);
        craft.SnatchFocus();
        craft.OrbitDriver.SetFromElements(cartesianData, cBody);

        BuildingManager.Instance.ExitBuildMode(false);
        craft.SnatchFocus();

        return craft;
    }
}

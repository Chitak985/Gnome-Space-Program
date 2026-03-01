using Godot;
using Godot.Collections;
using System;

public partial class CraftManager : Node
{
    public static readonly string classTag = "([color=#5f9fdf]CraftManager[color=white])";
    public static CraftManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    /*
        partData - A dictionary which stores the craft's structure and all necessary data to build it
        driver - All orbital / cartesian paremeters
        inRotatingFrame - Whether or not to interpret the given velocity/position as in the parent planet's rotating reference frame
        focus - Whether or not to "snatch" the camera's focus to this new craft
    */
    public Craft SpawnCraft(Dictionary partData, OrbitDriver driver, bool focus = false)
    {
        Logger.Print($"{classTag} Spawning craft at {driver.cartesian.position} with velocity {driver.cartesian.velocity}");

        Craft craft = new();
        ActiveSave.Instance.localSpace.AddChild(craft);

        craft.Initialize(driver, partData);
        craft.Load(true);

        // We can focus on the craft if we want to
        if (focus)
        {
            BuildingManager.Instance.ExitBuildMode(false);
            craft.SnatchFocus();
        }

        // Unleash physics and let it do its thing...
        craft.Anchor(false);
        craft.SetPositionFromCartesian();

        return craft;
    }

    // For if one wants to spawn a craft at a specific orbit
    public Craft SpawnCraft(Dictionary partData, Orbit orbit, bool focus = false)
    {
        OrbitDriver driver = new()
        {
            parent = orbit.parent,
            orbit = orbit,
            cartesian = Conics.ElemToCart(orbit)
        };

        Craft craft = SpawnCraft(partData, driver, focus);
        return craft;
    }

    // For if one wants to spawn a craft at a specific position
    public Craft SpawnCraft(Dictionary partData, CartesianData cartesian, bool focus = false)
    {
        OrbitDriver driver = new()
        {
            parent = cartesian.parent,
            orbit = new(), // TODO: Make function for converting cartesian elements to orbital elements!!!
            cartesian = cartesian
        };

        Logger.Print($"POSITION IS {cartesian.position}");
        Logger.Print($"VELOCITY IS {cartesian.velocity}");

        Craft craft = SpawnCraft(partData, driver, focus);
        return craft;
    }
}

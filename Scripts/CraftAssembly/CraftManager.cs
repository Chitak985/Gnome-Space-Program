using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class CraftManager : Node
{
    public static readonly string classTag = "([color=#5f9fdf]CraftManager[color=white])";
    public static CraftManager Instance { get; private set; }

    // List of all crafts currently instantiated with physics (NOT imaginary craft!)
    public List<Craft> LoadedCrafts { get; private set; } = [];

    [Signal] public delegate void CraftSpawnedEventHandler(Craft newCraft);

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

    // RUN THIS EVERY TIME A CRAFT IS SPAWNED!
    public void RegisterLoadedCraft(Craft craft)
    {
        Logger.Print($"{classTag} Registered craft physics object ({craft})");
        LoadedCrafts.Add(craft);
    }

    public Craft SpawnCraft(Dictionary partData, OrbitDriver driver, bool focus = false)
    {
        Logger.Print($"{classTag} Spawning craft at {driver.cartesian.position} with velocity {driver.cartesian.velocity}");

        Craft craft = new();
        ActiveSave.Instance.localSpace.AddChild(craft);

        RealityTangler.Instance.SwitchReferenceFrame();

        craft.Initialize(driver, partData);
        craft.Load(true);

        // Unleash physics and let it do its thing...
        craft.SetPositionFromCartesian();

        // We can focus on the craft if we want to
        if (focus)
        {
            BuildingManager.Instance.ExitBuildMode(false);
            craft.SnatchFocus();
        }

        // Shoot out a signal for anyone who wants to know
        EmitSignal(SignalName.CraftSpawned);

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

        Logger.Print(driver.cartesian.velocity);

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

        //Logger.Print($"POSITION IS {cartesian.position}");
        //Logger.Print($"VELOCITY IS {cartesian.velocity}");

        Craft craft = SpawnCraft(partData, driver, focus);
        return craft;
    }
}

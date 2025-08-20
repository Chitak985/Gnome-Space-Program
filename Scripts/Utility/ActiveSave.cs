using Godot;
using System;
using System.Collections.Generic;

// All major save data is stored here (crafts, celestials, etc)

public partial class ActiveSave : Node3D
{
    public static readonly string classTag = "([color=magenta]PlanetSystem[color=white])";
    public static ActiveSave Instance;

    // The great dictionary.
    public Dictionary<string, Variant> saveParams;

    // This should always be 1.0 upon loading!
    [Export] public double timeSpeed = 1;
    // In milliseconds
    public double saveTime;

    public override void _Ready()
    {
        GD.PrintRich($"{classTag} Active save starting...");
        Instance = this;
        foreach (KeyValuePair<string, Variant> param in saveParams)
        {
            GD.Print(param);
        }
        GD.PrintRich($"{classTag} Active save ready for init!");
    }

    // Start up all vital systems such as the planet system and whatnot
    public void InitSave()
    {

    }

    public override void _Process(double delta)
    {
        // Increment time since save creation (for orbital calculations mostly)
        saveTime += delta * 1000 * timeSpeed / 1000;

        // Set physics speed to match time speed
        Engine.TimeScale = timeSpeed;
    }
}
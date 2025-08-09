using Godot;
using System;
using System.Collections.Generic;

// All major save data is stored here (crafts, celestials, etc)

public partial class ActiveSave : Node
{
    public static ActiveSave Instance;

    // List of planet pack folders to load in this save
    public List<string> planetPacks;

    // This should always be 1.0 upon loading!
    [Export] public double timeSpeed = 1;

    // In milliseconds
    public double saveTime;

    public override void _Ready()
    {
        Instance = this;
    }

    // Start up all vital systems such as the planet system
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
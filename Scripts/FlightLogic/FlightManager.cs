using Godot;
using System;

public partial class FlightManager : Node
{
    // There should only be one instance of FlightManager per save
    public static FlightManager Instance;

    // The current craft to be the center of the universe
    [Export] public Craft currentCraft;

    public override void _Ready()
    {
        Instance = this;
    }
}

using Godot;
using System;

/*
    Handles game state, such as the active craft, planet, or what have you
*/

public partial class StateManager : Node
{
    public static readonly string classTag = "([color=yellow]StateManager[color=white])";
	public static StateManager Instance { get; private set; }

    public GameState gameState = GameState.Colony; // By default (when a save is loaded) we'll be focused on a colony

    // Every state is split up into each "substate" to keep stuff organized
    public FlightState flightState;
    public ColonyState colonyState;
    public MapState mapState;

    public enum GameState 
    {
        Flight,
        Colony
    }

    public struct FlightState 
    {
        public Craft activeCraft;
        // ... add more as needed
    }

    public struct ColonyState 
    {
        public Colony activeColony;
        // ... add more as needed
    }

    public struct MapState 
    {
        public CelestialBody focusedCBody;
        // ... add more as needed
    }

    public override void _Ready()
	{
        Instance = this;

        // Initialize states (to prevent nullref errors mostly)
        flightState = new();
        colonyState = new();
        mapState = new();
    }
}

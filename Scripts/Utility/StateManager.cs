using Godot;
using System;

/*
    Handles game state, such as the active craft, planet, or what have you
*/

public partial class StateManager : Node
{
    public static readonly string classTag = "([color=yellow]StateManager[color=white])";
	public static StateManager Instance { get; private set; }

    public GameState CurrentGameState { get; private set; } = GameState.Colony; // By default (when a save is loaded) we'll be focused on a colony

    // Every state is split up into each "sub-state" to keep stuff organized
    public FlightState CurrentFlightState { get; private set; }
    public ColonyState CurrentColonyState { get; private set; }

    // Signals
    [Signal] public delegate void AnyStateChangedEventHandler(StateManager stateManager); // Fires whenever ANY state is changed

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

    public void Initialize()
    {
        Instance = this;

        // Initialize states (to prevent nullref errors mostly)
        CurrentFlightState = new();
        CurrentColonyState = new();

        Logger.Print($"({classTag}) Ready!");
    }

    public void ChangeGameState(GameState newState)
    {
        CurrentGameState = newState;
        EmitSignal(SignalName.AnyStateChanged, this);
    }

    public void ChangeFlightState(FlightState newState)
    {
        CurrentFlightState = newState;
        EmitSignal(SignalName.AnyStateChanged, this);
    }

    public void ChangeColonyState(ColonyState newState)
    {
        CurrentColonyState = newState;
        EmitSignal(SignalName.AnyStateChanged, this);
    }
}

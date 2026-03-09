using Godot;
using System;

public partial class FlightCamera : CamControl
{
    // THERE SHOULD ONLY EVER BE ONE FLIGHT CAMERA!!
    // The same camera (THIS ONE) is used in both colony view and flight
    public static readonly string classTag = "([color=MEDIUM_SPRING_GREEN]FlightCamera[color=white])";
    public static FlightCamera Instance { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;

        RealityTangler.Instance.CameraProcess += Update;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        StateManager.GameState gameState = StateManager.Instance.gameState;

        switch (gameState)
        {
            case StateManager.GameState.Flight:
                ground = StateManager.Instance.flightState.activeCraft.OrbitDriver.parent;
                break;
            case StateManager.GameState.Colony:
                Colony colony = StateManager.Instance.colonyState.activeColony;
                // Check if colony is null to prevent nullrefs
                ground = colony?.parentBody;
                break;
            default:
                break;
        }

        //Update();
    }
}

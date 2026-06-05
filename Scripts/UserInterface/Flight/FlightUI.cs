using Godot;
using System;

public partial class FlightUI : Control
{
    public static FlightUI Instance { get; private set; }

    public override void _Ready()
    {
        ActiveSave.Instance.GameInitComplete += Init;
    }

    private void Init()
    {
        StateManager.Instance.AnyStateChanged += AnyStateChanged;
    }

    private void AnyStateChanged(StateManager stateManager)
    {
        if (stateManager.CurrentGameState == StateManager.GameState.Flight)
        {
            Visible = true;
        }else{
            Visible = false;
        }
    }
}

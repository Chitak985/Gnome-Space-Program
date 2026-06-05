using Godot;
using System;

public partial class TimewarpButton : Button
{
    public int level = 0;

    [Signal] public delegate void OnTimewarpClickedEventHandler(int level);

    public override void _Ready()
    {
        Pressed += OnPressed;
    }

    private void OnPressed()
    {
        EmitSignal(SignalName.OnTimewarpClicked, level);
    }
}

using Godot;
using System;

public partial class PerformanceMenu : VBoxContainer
{
    [Export] public Label fpsLabel;

    public override void _Process(double delta)
    {
        fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }
}

using Godot;
using System;

public partial class ClockUI : Control
{
    [Export] private RichTextLabel label;

    public override void _Process(double delta)
    {
        label.Text = $"T + {Math.Round(ActiveSave.Instance.SaveTime)}s";
    }
}

using Godot;
using System;

public partial class MainmenuCategory : VBoxContainer
{
    [Export] public Label title;
    [Export] public VBoxContainer content;

    // Hide the category if none of its contents are visible at the moment (why have it?)
    public override void _Process(double delta)
    {
        Godot.Collections.Array<Node> contents = content.GetChildren();
        int invisibleControls = 0;
        foreach (Node node in contents)
        {
            Control control = (Control)node;
            if (!control.Visible) invisibleControls++;
        }
        Visible = contents.Count > invisibleControls;
    }
}

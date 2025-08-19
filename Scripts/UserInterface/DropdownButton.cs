using Godot;
using System;

public partial class DropdownButton : TextureButton
{
    [Export] public VBoxContainer container;

    public override void _Toggled(bool toggledOn)
    {
        container.Visible = !toggledOn;
    }
}

using Godot;
using System;

public partial class DraggablePanel : Panel
{
    bool mouseInWindow = false;
    bool dragging = false;
    Vector2 offset;

    public override void _Ready()
    {
        // Just set everything up here. No need for the editor.
        GuiInput += GuiEvent;
    }

    public void GuiEvent(InputEvent inpEvent)
    {
        if (inpEvent is InputEventMouseButton buttonEvent)
        {
            if (buttonEvent.ButtonIndex == MouseButton.Left)
            {
                if (buttonEvent.Pressed)
                {
                    dragging = true;
                    offset = GetGlobalMousePosition() - GlobalPosition;
                }else{
                    dragging = false;
                }
            }
        }else if (inpEvent is InputEventMouseMotion && dragging){
            GlobalPosition = GetGlobalMousePosition() - offset;
        }
    }
}

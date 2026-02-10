using Godot;
using Godot.Collections;
using System;

// GRRRRRRRRRRR
public partial class ColonyMenu : ContextMenu
{
    public Colony colonyInQuestion;
    [Export] public Label title;

    public override void Opened(Dictionary info)
    {
        if ((Node)info["colony"] is Colony colony)
        {
            colonyInQuestion = colony;
            title.Text = colony.name;
        }
    }

    public void EnterButtonPressed()
    {
        colonyInQuestion.Enter();

        Visible = false;
    }
}

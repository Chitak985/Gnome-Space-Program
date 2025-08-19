using Godot;
using System;

public partial class MainMenuOption : OptionButton
{
    public SaveParam param;
    public override void _Ready()
    {
        OnItemSelect(0);
    }
    public void OnItemSelect(int index)
    {
        param.inputData.currentSelection = GetItemText(index);
    }
}

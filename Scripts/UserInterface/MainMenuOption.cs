using Godot;
using System;

public partial class MainMenuOption : OptionButton
{
    public SaveParam param;
    public void OnItemSelect(int index)
    {
        param.inputData.currentSelection = GetItemText(index);
    }
}

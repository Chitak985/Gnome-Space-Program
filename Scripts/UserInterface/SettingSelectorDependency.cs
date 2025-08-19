using Godot;
using System;

public partial class SettingSelectorDependency : HBoxContainer
{
    public string key;
    public Variant value;
    public System.Collections.Generic.Dictionary<string, SaveParam> dictToCheck;
    public override void _Process(double delta)
    {
        Variant tocheck = dictToCheck[key].inputData.currentSelection;
        bool equals = false;

        switch (value.VariantType)
        {
            case Variant.Type.String:
                equals = (string)value == (string)tocheck;
                break;
            default:
                break;
        }
        Visible = equals;
    }
}
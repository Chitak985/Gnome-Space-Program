using Godot;
using Godot.Collections;
using System;

// GRRRRRRRRRRR
public partial class PlanetMenu : ContextMenu
{
    public CelestialBody cBodyInQuestion;
    [Export] public Label title;
    [Export] public Label radius;
    [Export] public Label mass;
    [Export] public Label gravity;

    public override void Opened(Dictionary info)
    {
        if ((Node)info["planet"] is CelestialBody cBody)
        {
            cBodyInQuestion = cBody;
            title.Text = cBody.cBodyName;
            radius.Text = $"Radius: {cBody.radius} m";
            mass.Text = $"Mass: {cBody.mass} kg";
            gravity.Text = $"Gee ASL: {cBody.geeASL} g";
        }
    }
}

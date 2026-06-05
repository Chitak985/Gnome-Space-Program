using Godot;
using System;
using System.Collections.Generic;

public partial class LightingManager : Node
{
    public static LightingManager Instance { get; private set; }
    public static readonly string classTag = "([color=yellow]LightingManager[color=white])";
    public List<CBodyLight> Lights { get; private set; } = [];

    [Export] private PackedScene lightPrefab;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        foreach (CBodyLight light in Lights)
        {
            light.UpdateLight();
        }
    }

    public CBodyLight CreateLight(CelestialBody cBody)
    {
        Logger.Print($"{classTag} Creating light for {cBody}");

        CBodyLight light = (CBodyLight)lightPrefab.Instantiate();
        AddChild(light);
        Lights.Add(light);
        light.Create(cBody);

        return light;
    }
}

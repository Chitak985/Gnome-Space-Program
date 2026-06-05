using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class CraftManager : Node
{
    public static readonly string classTag = "([color=#5f9fdf]CraftManager[color=white])";

    [Export] public double TimeEaseDuration = 1; // How long a craft must wait before reenabling its physics sim post time-accel
    public static CraftManager Instance { get; private set; }

    // List of all crafts currently instantiated with physics (NOT imaginary craft!)
    public List<Craft> LoadedCrafts { get; private set; } = [];

    [Signal] public delegate void CraftSpawnedEventHandler(Craft newCraft);

    public override void _Ready()
    {
        Instance = this;
    }

    public Craft CreateCraft(Dictionary partData, CelestialBody parentCBody)
    {
        Craft craft = new();
        ActiveSave.Instance.localSpace.AddChild(craft);
        craft.Init(partData, parentCBody);

        EmitSignal(SignalName.CraftSpawned, craft);

        return craft;
    }
}

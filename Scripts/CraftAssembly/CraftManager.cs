using Godot;
using Godot.Collections;
using System;

public partial class CraftManager : Node
{
    public static readonly string classTag = "([color=#5f9fdf]CraftManager[color=white])";
    public static CraftManager Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    public Craft SpawnCraft(Dictionary partData, Transform3D spawnTransform, bool focus = false)
    {
        Logger.Print($"{classTag} Spawning craft at {spawnTransform.Origin}");

        Craft craft = new();
        ActiveSave.Instance.localSpace.AddChild(craft);
        craft.Instantiate(partData);

        craft.GlobalPosition = spawnTransform.Origin;

        craft.Initialize();

        // We can not focus on the craft and stay in the editor if we absolutely want to
        if (focus)
        {
            BuildingManager.Instance.ExitBuildMode(false);
            craft.SnatchFocus();
        }

        // Unleash physics and let it do its thing
        craft.Anchor(false);

        return craft;
    }
}

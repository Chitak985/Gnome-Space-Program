using Godot;
using System;
using System.Collections.Generic;

// Class to easily compile settings for save creation 
// (because managing all these buttons and checkboxes always take a ton of space and looks bad and I HATE IT I HATE IT AHHH)
public partial class SaveSettingsManager : Panel
{
    public List<PlanetPack> planetPacks;
    [Export] public Dictionary<Variant, Variant> options;
    public override void _Ready()
    {
        planetPacks = SaveManager.GetPlanetPacks();
        GD.Print($"Got Planet Packs! Total: {planetPacks.Count}");
    }

    public void CreateOptionTree()
    {

    }

    //public SaveData CompileSaveData()
    //{

    //}
}

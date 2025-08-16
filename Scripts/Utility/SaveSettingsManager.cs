using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/* 
8/09/25
Class to easily compile settings for save creation 
(because managing all these buttons and checkboxes always take a ton of space and looks bad and I HATE IT I HATE IT AHHH)
8/16/25
There has been a slight deviation in my plans, I have built what I see as the worst save settings manager of all time !!
I also have a motherfucking HEADACHE from all of this, but BOW DOWN ANYWAYS! 
I have also started logging the date.. It's been getting hard to keep track of it.
*/
public partial class SaveSettingsManager : Panel
{
    public List<PlanetPack> planetPacks;
    [Export] public Tree saveParamTree;
    public override void _Ready()
    {
        planetPacks = SaveManager.GetPlanetPacks();
        GD.Print($"Got Planet Packs! Total: {planetPacks.Count}");

        Dictionary<string, SaveParam> saveSchemas = SaveManager.GetSaveSchemas();
        CreateOptionTree(saveSchemas);
    }

    // That's riiight! We don't just have "buttons" that we place willy nilly.. We BUILD THEM PROCEDURALLY!
    public void CreateOptionTree(Dictionary<string, SaveParam> saveSchema)
    {
        GD.Print(saveSchema.ElementAt(0));
        TreeItem root = saveParamTree.CreateItem();
        root.SetText(0, "huh");
        TreeItem test0 = saveParamTree.CreateItem(root);
        test0.SetText(0, "buh");
    }

    //public SaveData CompileSaveData()
    //{

    //}
}

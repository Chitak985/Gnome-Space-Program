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
    [Export] public VBoxContainer saveParamList;
    [Export] public PackedScene dropdownPrefab;
    public Dictionary<string, SaveParam> saveSchemas;
    public override void _Ready()
    {
        planetPacks = SaveManager.GetPlanetPacks();
        GD.Print($"Got Planet Packs! Total: {planetPacks.Count}");

        saveSchemas = SaveManager.GetSaveSchemas();
        CreateOptionTree(saveSchemas);
    }

    // That's riiight! We don't just have "buttons" that we place willy nilly.. We BUILD THEM PROCEDURALLY!
    public void CreateOptionTree(Dictionary<string, SaveParam> saveSchema)
    {
        GD.Print(saveSchema.ElementAt(0));

        Dictionary<string, VBoxContainer> categories = [];

        foreach (KeyValuePair<string, SaveParam> saveParam in saveSchema)
        {
            // Handle categories
            // If a category exists, then don't make a new one.
            SaveParam param = saveParam.Value;
            string category = param.category;

            if (categories.TryGetValue(category, out VBoxContainer catItem))
            {
                CreateOptionTreeItem(catItem, param);
            }else{
                // HOLY SHIT
                VBoxContainer newCatContItem = new();
                VBoxContainer newCatItem = new();
                HBoxContainer dropdown = new();
                DropdownButton droppy = (DropdownButton)dropdownPrefab.Instantiate();
                droppy.container = newCatItem;
                Label catLabel = new() {Text = category};
                dropdown.AddChild(droppy);
                dropdown.AddChild(catLabel);
                newCatContItem.AddChild(dropdown);
                newCatContItem.AddChild(newCatItem);
                saveParamList.AddChild(newCatContItem);

                categories.Add(category, newCatItem);
                CreateOptionTreeItem(newCatItem, param);
            }
        }
    }

    // Determine what to do and then do it. Fuck else am I supposed to say?
    public static void CreateOptionTreeItem(VBoxContainer cont, SaveParam param)
    {
        BoxContainer itemCont = new();
        Label margin = new() {Text = "    "};
        itemCont.AddChild(margin);

        Variant data = param.data;

        if (data.VariantType != Variant.Type.Array 
        && data.VariantType != Variant.Type.Dictionary)
        {
            GD.Print("not implement yet sowwy :(");
        }else{
            switch (param.inputData.selectorType)
            {
                case "optionSingle": // Only a single option can be chosen
                    OptionButton item = new();
                    item.AddItem(param.name);
                    item.AddSeparator();
                    foreach (Variant point in (Godot.Collections.Array)data)
                    {
                        item.AddItem(point.ToString());
                    }
                    itemCont.AddChild(item);
                    break;
                default:
                    break;
            }
        }

        cont.AddChild(itemCont);

        // Check if dependency key is empty or not
        if (param.dependency.key != "")
        {
            
        }
    }

    //public SaveData CompileSaveData()
    //{

    //}
}

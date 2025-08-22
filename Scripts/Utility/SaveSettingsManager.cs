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
8/22/25 [Chitak]
There is some stuff need to be fixed. I tried fixing some and accidentally fixed the entire game lol. You're welcome :)
First of all the mainMenu thing that couldn't be found... You used the wrong name "MainMenu" instead of "MainMenuUI" and also that's not how you get the root node.
I know how to do that in GDScript, but there is no @onready so it doesn't work. Also, JUST EXPORT IT!
Second, you weren't loading SaveManager, so instead of going through hell of accessing something through scenes I just autoladed it and it worked lmao
*/
public partial class SaveSettingsManager : Panel
{
	public Dictionary<string, PlanetPack> rootSystems;
	[Export] public VBoxContainer saveParamList;
	[Export] public CSharpScript settingSelectorDependency;
	[Export] public PackedScene categoryPrefab;
	[Export] public PackedScene optionPrefab;
	[Export] public PackedScene lineEditPrefab;

	[Export] public Control rootNode;
	public Dictionary<string, SaveParam> saveSchemas;

	public override void _Ready()
	{
		rootSystems = SaveManager.GetPlanetPacks("rootSystem");
		GD.Print($"Got Root Systems! Total: {rootSystems.Count}");

		saveSchemas = SaveManager.GetSaveSchemas();
		CreateOptionTree(saveSchemas);
	}

	public void OnCreateButton()
	{
		rootNode.Visible = false;
		Dictionary<string, Variant> creationParams = [];
		foreach (KeyValuePair<string, SaveParam> param in saveSchemas)
		{
			creationParams.Add(param.Key, param.Value.inputData.currentSelection);
		}
		// Create the save
		SaveManager.Instance.LoadSave(creationParams);
	}

	// That's riiight! We don't just have "buttons" that we place willy nilly.. We BUILD THEM PROCEDURALLY!
	public void CreateOptionTree(Dictionary<string, SaveParam> saveSchema)
	{
		Dictionary<string, MainmenuCategory> categories = [];

		foreach (KeyValuePair<string, SaveParam> saveParam in saveSchema)
		{
			// Handle categories
			// If a category exists, then don't make a new one.
			SaveParam param = saveParam.Value;
			string category = param.category;

			if (categories.TryGetValue(category, out MainmenuCategory catItem))
			{
				CreateOptionTreeItem(catItem.content, param);
			}else{
				// Ahh much better
				MainmenuCategory newCatCont = (MainmenuCategory)categoryPrefab.Instantiate();
				newCatCont.title.Text = category;
				saveParamList.AddChild(newCatCont);

				categories.Add(category, newCatCont);
				CreateOptionTreeItem(newCatCont.content, param);
			}
		}
	}

	// Determine what to do and then do it. Fuck else am I supposed to say?
	public void CreateOptionTreeItem(VBoxContainer cont, SaveParam param)
	{
		HBoxContainer itemCont = new();
		itemCont.SizeFlagsHorizontal = SizeFlags.Fill;
		Label margin = new() {Text = "    "};
		itemCont.AddChild(margin);

		Variant data = param.data;

		if (data.VariantType != Variant.Type.Array 
		&& data.VariantType != Variant.Type.Dictionary)
		{
			switch (data.VariantType)
			{
				case Variant.Type.String:
					LineEdit item = (LineEdit)lineEditPrefab.Instantiate();
					item.PlaceholderText = param.name;
					item.Text = (string)data;
					itemCont.AddChild(item);
					break;
				default:
					break;
			}
		}else{
			switch (param.inputData.selectorType)
			{
				case "optionSingle": // Only a single option can be chosen
					MainMenuOption item = (MainMenuOption)optionPrefab.Instantiate();
					item.param = param;
					//item.AddItem(param.name);
					//item.AddSeparator();
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

		// Check if dependency exists
		if (param.dependency != null)
		{
			// Godot fucking disposes the object so we have to do this bullshit
			ulong cuntId = itemCont.GetInstanceId();
			// Replaces old C# instance with a new one. Old C# instance is disposed.
			itemCont.SetScript(settingSelectorDependency);
			// Get the new C# instance
			SettingSelectorDependency setseldep = (SettingSelectorDependency)InstanceFromId(cuntId);
			setseldep.key = param.dependency.key;
			setseldep.value = param.dependency.value;
			setseldep.dictToCheck = saveSchemas;
			setseldep.SetProcess(true);
		}
	}

	//public SaveData CompileSaveData()
	//{

	//}
}

using Godot;
using Godot.Collections;

public partial class PartMenuHandler : Node
{
    public static PartMenuHandler Instance { get; private set; }
    [Export] public ContextMenus contextMenus;
    [Export] public PackedScene menuPrefab;

    // Bunch of generic UI items that can be used ig
    [Export] public PackedScene buttonPrefab;

    public override void _Ready()
    {
        Instance = this;
        SingletonRegistry.Register(this);
    }

    public void ToggleMenu(Part part)
    {
        ulong partInstanceID = part.GetInstanceId();
        string partName = $"{partInstanceID}_MENU";

        Dictionary info = [];
        info.Add("mousePos", GetViewport().GetMousePosition());

        contextMenus.OpenMenu(partName, info);
    }

    public PartMenu CreateMenu(Part part)
    {
        ulong partInstanceID = part.GetInstanceId();
        string partName = $"{partInstanceID}_MENU";

        PartMenu partMenu = (PartMenu)menuPrefab.Instantiate();
        partMenu.Name = partName;
        partMenu.menus = contextMenus;
        partMenu.part = part;
        contextMenus.AddChild(partMenu);
        contextMenus.menus.Add(partMenu);

        partMenu.Visible = false;

        return partMenu;
    }

    // Create buttons and stuff
    public static Button CreateButton(Control parent, string label)
    {
        Button button = (Button)Instance.buttonPrefab.Instantiate();
        button.Text = label;
        parent.AddChild(button);
        return button;
    }
}

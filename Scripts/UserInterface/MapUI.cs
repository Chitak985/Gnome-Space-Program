using Godot;
using System;

// Singleton for all the map ui bs
public partial class MapUI : Control
{
    [Export] public ContextMenus contextMenus;

    [Export] private DraggablePanel dragPanel;
    [Export] private float undockMult = 0.5f;

    private bool docked = true;

    public void ToggleDock(bool toggle)
    {
        docked = toggle;
        dragPanel.draggable = !toggle;

        Vector2 vpSize = GetViewport().GetVisibleRect().Size;
        if (toggle)
        {
            dragPanel.Size = vpSize;
            dragPanel.Position = Vector2.Zero;
            dragPanel.ZIndex = -10;
        }else{
            Vector2 undockedSize = dragPanel.Size;
            if (undockedSize == vpSize) undockedSize = GetViewport().GetVisibleRect().Size * undockMult;

            dragPanel.Size = undockedSize;
            dragPanel.Position = (vpSize - undockedSize) / 2;
            dragPanel.ZIndex = 10;
        }
    }

    public void OnDockButtonPressed()
    {
        ToggleDock(!docked);
    }
}

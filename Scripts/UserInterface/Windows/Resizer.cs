using Godot;

// One-stop shop for resize handles
public partial class Resizer : Control
{
    [Export] private Control target;
    public override void _Ready()
    {
        if (target == null && GetParent() is Control control) target = control;

        foreach (Node node in GetChildren())
        {
            if (node is ResizeHandle resizeHandle)
            {
                resizeHandle.target = target;
            }
        }
    }
}

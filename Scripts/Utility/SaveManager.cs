using Godot;
using System;

public partial class SaveManager : Node
{
    public static SaveManager Instance;

    [Export] public double timeSpeed = 1;

    // In milliseconds
    public double saveTime;

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        // Increment time since save creation (for orbital calculations mostly)
        saveTime += delta * 1000 * timeSpeed / 1000;
    }
}
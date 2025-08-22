using Godot;
using System;
using System.Collections.Generic;

/* 
BOOYAH!
Technically this class encompasses both colony AND ship parts, as I intend for them to be used interchangeably.
Why? Because I want players to have the freedom to get up to any sort of shenanigans with these systems.
*/
public partial class Part : Node
{
    [Export] public bool enabled = false;

    public List<PartModule> partModules = [];

    public override void _Ready()
    {
        GD.Print($"({Name}) Getting part modules...");
        partModules = GetPartModules(this);
        GD.Print($"({Name}) Got all part modules! Count: {partModules.Count}");
    }

    // Recursive function to find every part module
    public List<PartModule> GetPartModules(Node parent)
    {
        List<PartModule> modules = [];
        foreach (Node node in parent.GetChildren())
        {
            if (node.IsClass("PartModule"))
            {
                modules.Add((PartModule)node);
            }
            if(node.GetChildCount() > 0) modules.AddRange(GetPartModules(node));
        }
        return modules;
    }

    public void InitPart()
    {
        
    }
}

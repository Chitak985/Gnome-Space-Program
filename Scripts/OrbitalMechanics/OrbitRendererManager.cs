using Godot;
using System.Collections.Generic;

// Central class to easily reference every orbit renderer 
// (because they're all parented to their respective celestial body)

public partial class OrbitRendererManager : Node
{
    [Export] public PackedScene rendererPrefab;
    public static OrbitRendererManager Instance { get; private set; }
    public List<OrbitRenderer> orbitRenderers = [];

    public override void _Ready()
    {
        Instance = this;
    }

    public void UpdateOrbitRenderers()
    {
        foreach (OrbitRenderer renderer in orbitRenderers)
        {
            renderer.Update();
        }
    }

    public void ToggleRenderers(bool toggle)
    {
        foreach (OrbitRenderer renderer in orbitRenderers)
        {
            renderer.Visible = toggle;
        }
    }
}

using Godot;
using System.Collections.Generic;

// Central class to easily reference every orbit renderer 
// (because they're all parented to their respective celestial body)

public partial class OrbitRendererManager : Node
{
    [Export] public double orbitPrecision;
    [Export] public PackedScene rendererPrefab;
    public static OrbitRendererManager Instance { get; private set; }
    public List<OrbitRenderer> orbitRenderers = [];

    public override void _Ready()
    {
        Instance = this;
    }

    // Create renderer for celestial bodies
    public OrbitRenderer CreateOrbitRenderer(CelestialBody cBody)
    {
        OrbitRenderer renderer = (OrbitRenderer)rendererPrefab.Instantiate();
        orbitRenderers.Add(renderer);
        renderer.orbit = cBody.OrbitDriver.orbit;
        cBody.OrbitDriver.parent.mapObject.AddChild(renderer);
        renderer.enabled = true;
        return renderer;
    }

    // Create renderer for crafts
    public OrbitRenderer CreateOrbitRenderer(Craft craft)
    {
        OrbitRenderer renderer = (OrbitRenderer)rendererPrefab.Instantiate();
        orbitRenderers.Add(renderer);
        renderer.orbit = craft.OrbitDriver.orbit;
        craft.OrbitDriver.parent.mapObject.AddChild(renderer);
        renderer.enabled = true;
        return renderer;
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

using Godot;

// Class for handing craft motion (which is why it's in the FlightLogic folder and not the OrbitalMechanics folder)
public partial class OrbitDriver : Node
{
    [Export] public bool enabled;
    [Export] public Craft craft;

    public static readonly string classTag = "([color=cyan]OrbitDriver[color=white])";

    /* 
    Some things to note:
    1. Cartesian data should NOT be affected by floating origin
    2. Orbital parameters and cartesian data are relative to the parent body
    3. As per rule 2, parameters will need to be translated upon SOI change
    */
    public Orbit orbit = new();
    public CartesianData cartData = new();

    private PlanetSystem pSystem;

    public override void _Ready()
    {
        pSystem = PlanetSystem.Instance;
        cartData.position = Double3.Zero;
        cartData.velocity = Double3.Zero;
    }

    public void Initialize()
    {
        orbit.parent = craft.currentInfluence;
        cartData.parent = craft.currentInfluence;
        enabled = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (enabled)
        {
            // Use Newtonian motion (game engine physics) if time warp is under 10x
            cartData.position = Double3.ConvertToDouble3(craft.Position) - FloatingOrigin.Instance.offset;

            (CelestialBody cBody, Double3 newPosition) = PatchedConics.GetSOI(cartData);
            if (craft.currentInfluence != cBody)
            {
                GD.PrintRich($"{classTag} Craft {craft} has crossed SOI boundary from {craft.currentInfluence} to {cBody}");
                craft.currentInfluence = cBody;
                orbit.parent = cBody;
                cartData.parent = cBody;
                craft.Position = newPosition.ToFloat3();
            }
        }
    }

    public Double3 GetGravForce()
    {
        double force = PatchedConics.GravConstant * (craft.mass * orbit.parent.mass) / cartData.position.DistanceTo(orbit.parent.cartesianData.position);
        return cartData.position.DirectionTo(orbit.parent.cartesianData.position) * force;
    }
}

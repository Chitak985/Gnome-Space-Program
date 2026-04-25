using Godot;
using System;

/*
    This object handles object motion along a set orbit.
*/

public partial class OrbitDriver : Node
{
    // The node that is "driven" by the driver. This ideally should never change after instantiation.
    [Export] public Node3D Vehicle { get; private set; }

    // This will change if the vehicle enters another SOI (if patching is enabled)
    [Export] public CelestialBody ParentCBody { get; set; } // FIX THIS STUPID FUCK TO NOT BE SETTABLE WHEN PLANETS ARE REVAMPED

    public KeplerianState KeplerState { get; private set; }
    public CartesianState CartState { get; private set; }

    public bool Enabled = false;
    public bool OnRails;

    // Signal nonsense
    [Signal] public delegate void ElementsUpdatedEventHandler();

    public void Init(CelestialBody cBody, Node3D vehicle, bool startOnRails = false)
    {
        Name = "OrbitDriver";

        OnRails = startOnRails;

        KeplerState = new();
        AddChild(KeplerState);
        KeplerState.Name = "KeplerianState";

        CartState = new();
        AddChild(CartState);
        CartState.Name = "CartesianState";

        Vehicle = vehicle;
        SetParent(cBody);

        vehicle.AddChild(this);
    }

    public void Update()
    {
        if (Enabled)
        {
            if (OnRails)
            {
                PropagateFromKepler();
            } 
        }else{
            CartState.elements.position = Vector3.Zero;
        }
    }

    public void SetFromElements(KeplerianState.KeplerianElements elements, CelestialBody cBody = null)
    {
        // Fall back to parent celestial body if none is supplied
        cBody ??= ParentCBody;

        KeplerState.elements = elements;
        CartState.elements = Conics.ElemToCart(elements, cBody);

        EmitSignal(SignalName.ElementsUpdated);
    }

    public void SetFromElements(CartesianState.CartesianElements elements, CelestialBody cBody = null)
    {
        // Fall back to parent celestial body if none is supplied
        cBody ??= ParentCBody;

        CartState.elements = elements;
        KeplerState.elements = Conics.CartToElem(elements, cBody);

        EmitSignal(SignalName.ElementsUpdated);
    }

    public void SetParent(CelestialBody cBody)
    {
        ParentCBody = cBody;
    }

    // Propagates the orbit along the given Keplerian state. 
    // For when there are no (or, in the case of planets, very very little) meaningful perturbation on the vehicle.
    private void PropagateFromKepler()
    {
        // Increment true anomaly
        KeplerState.elements.trueAnomaly = Conics.TimeToTrueAnomaly(KeplerState.elements, ParentCBody, ActiveSave.Instance.SaveTime, 0);

        // Update the cartesian
        CartState.elements = Conics.ElemToCart(KeplerState.elements, ParentCBody);

        // Positioning is handled inside the respective vehicle
        //Vehicle.Position = CartState.elements.position;
    }

    // For when the driver is NOT on rails and the vehicle is being influenced by ordinary physics
    private void UpdateFromCartesian()
    {
        // Update keplerian from the cartesian
        KeplerState.elements = Conics.CartToElem(CartState.elements, ParentCBody);
    }
}

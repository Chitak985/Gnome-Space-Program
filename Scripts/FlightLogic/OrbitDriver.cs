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
    [Export] public CelestialBody ParentCBody { get; private set; } // FIX THIS STUPID FUCK TO NOT BE SETTABLE WHEN PLANETS ARE REVAMPED

    public KeplerianState KeplerState { get; private set; }
    public CartesianState CartState { get; private set; }

    public bool Enabled = false;
    public bool OnRails { get; private set; }

    // Time when the driver switched to or from an "on-rails" state
    // 0 for all celestial bodies.
    public double OnRailsSwithTime { get; private set; }

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

    public void ToggleOnRailsOrbit(bool toggle, bool saveMeanAnomaly = false)
    {
        OnRails = toggle;

        OnRailsSwithTime = ActiveSave.Instance.SaveTime;

        if (saveMeanAnomaly)
        {
            KeplerState.elements.meanAnomalyAtEpoch = Conics.TrueAnomalyToMeanAnomaly(KeplerState.elements.trueAnomaly, KeplerState.elements.eccentricity);
        }
    }

    // Propagates the orbit along the given Keplerian state. 
    // For when there are no meaningful perturbations on the vehicle.
    private void PropagateFromKepler()
    {
        // Increment true anomaly
        KeplerState.elements.trueAnomaly = Conics.TimeToTrueAnomaly(KeplerState.elements, ParentCBody, ActiveSave.Instance.SaveTime, OnRailsSwithTime);

        // Update the cartesian
        CartState.elements = Conics.ElemToCart(KeplerState.elements, ParentCBody);
    }

    // For when the driver is NOT on rails and the vehicle is being influenced by ordinary physics
    private void UpdateFromCartesian()
    {
        // Update keplerian from the cartesian
        KeplerState.elements = Conics.CartToElem(CartState.elements, ParentCBody);
    }

    public override string ToString()
    {
        string referenceFrame = $"Reference: {ParentCBody}\n";
        string cartesianText = $"\nState Vectors: \n\n{CartState}\n";
        string orbitText = $"\nOrbital Elements: \n\n{KeplerState}";
        return referenceFrame + cartesianText + orbitText;
    }
}

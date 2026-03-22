using Godot;

/*
    Class for handing craft and celestial motion
    Each respective class controls this in its own way.
*/

public partial class OrbitDriver : Node
{
    public bool enabled = true;
    public CelestialBody parent;
    public Orbit orbit;
    public CartesianData cartesian;

    // Always true for planets
    public bool OnRails { get; private set; } = true;

    // Time when the driver switched to an "on-rails" state
    // 0 for all celestial bodies.
    public double TimeAtRailsEntry { get; private set; }

    public void Update()
    {
        if (enabled)
        {
            cartesian.parent = parent;
            if (orbit != null)
            {
                orbit.parent = parent;

                if (!OnRails)
                {
                    GenerateOrbit();
                }else{
                    PropagateOrbit();
                }
            }
        }
    }

    // For on-rails objects, propagates the orbit and sets the cartesian state vectors
    public void PropagateOrbit()
    {
        // Propagate the true anomaly forwards in time
        orbit.trueAnomaly = 
            Conics.TimeToTrueAnomaly(orbit, ActiveSave.Instance.SaveTime, TimeAtRailsEntry);

        // Simply follow the orbit
        CartesianData newCartesian = Conics.ElemToCart(orbit);
        cartesian = newCartesian;
    }

    // For dynamic objects, generates the orbit based on the cartesian state vectors
    public void GenerateOrbit()
    {
        orbit = Conics.CartToElem(cartesian);
    }

    // Modifies the orbit to have the mean anomaly
    public void InitCraftPropagator()
    {
        orbit.meanAnomalyAtEpoch = Conics.TrueAnomalyToMeanAnomaly(orbit.trueAnomaly, orbit.eccentricity);
        TimeAtRailsEntry = ActiveSave.Instance.SaveTime;
    }

    public void ToggleOnRailsOrbit(bool toggle)
    {
        OnRails = toggle;
        // Perhaps more stuff if we need
    }

    public override string ToString()
    {
        string referenceFrame = $"Reference: {parent}\n";
        string cartesianText = $"\nState Vectors: \n\n{cartesian}\n";
        string orbitText = $"\nOrbital Elements: \n\n{orbit}";
        return referenceFrame + cartesianText + orbitText;
    }
}

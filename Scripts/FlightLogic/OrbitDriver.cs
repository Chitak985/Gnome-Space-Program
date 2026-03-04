using Godot;

/*
    Class for handing craft motion (which is why it's in the FlightLogic folder and not the OrbitalMechanics folder)
    This class sits dormantly when a craft is subject to ordinary newtonian physics. 
    It only comes into play during timewarp or when the craft is unloaded.
*/

public class OrbitDriver
{
    public CelestialBody parent; // For easy access. This is also stored over in the orbit and cartesian data
    public Orbit orbit;
    public CartesianData cartesian;
    public OrbitRenderer renderer;

    public void Update()
    {
        orbit.parent = parent;
        cartesian.parent = parent;
        if (renderer != null)
        {
            renderer.orbit = null;
            renderer.orbit = orbit; // Update the orbit
        }
    }
}

using Godot;

/*
    Class for handing craft and celestial motion
    Each respective class controls this in its own way.
*/

public partial class OrbitDriver : Node
{
    public CelestialBody parent;
    public Orbit orbit;
    public CartesianData cartesian;

    public void Update()
    {
        if (orbit != null)
        {
            orbit.parent = parent;
        }
       
        cartesian.parent = parent;
    }
}

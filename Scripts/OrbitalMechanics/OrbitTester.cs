using Godot;
using System;
using System.Collections.Generic;

public partial class OrbitTester : Node3D
{
    public List<CelestialBody> cBodyList = [];

    public override void _Ready()
    {
        CelestialBody rootBody = new CelestialBody{
            mass = 10000000000000,
        };
        cBodyList.Add(rootBody);

        cBodyList.Add(new CelestialBody{
            mass = 10000,
            orbit = new Orbit{
                parent = rootBody,
                semiMajorAxis = 10,
                inclination = 0,
                eccentricity = .5,
                argumentOfPeriapsis = 0,
                longitudeOfAscendingNode = 0,
                trueAnomaly = 0
            }
        });
        
        foreach (CelestialBody cBody in cBodyList)
        {
            cBody.CreateDebugOrb(this);
        }
    }

    public override void _Process(double delta)
    {
        foreach (CelestialBody cBody in cBodyList)
        {
            if (cBody.orbit != null)
            {
                (Double3 position, Double3 velocity) = PatchedConics.KOEtoECI(cBody.orbit, cBody.orbit.parent, Time.GetUnixTimeFromSystem(), 0);
                GD.Print(position.X + " " + position.Y + " " + position.Z);
                cBody.debugOrb.GlobalPosition = new Vector3((float)position.X,(float)position.Y,(float)position.Z);
            }
        }
    }
}

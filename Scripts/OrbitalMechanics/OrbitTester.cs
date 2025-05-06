using Godot;
using System;
using System.Collections.Generic;

public partial class OrbitTester : Node3D
{
    [Export] public double testBodySMA = 2;
    [Export] public double testBodyECC = 0;
    [Export] public double timeSpeed = 1;
    [Export] public double time;
    public List<CelestialBody> cBodyList = [];

    public CelestialBody anotherBody;

    public override void _Ready()
    {
        CelestialBody rootBody = new CelestialBody{
            mass = 10000000000000,
            orbit = new Orbit{
                position = new Double3(0,0,0)
            }
        };
        cBodyList.Add(rootBody);

        anotherBody = new CelestialBody{
            mass = 1000000000000,
            orbit = new Orbit{
                parent = rootBody,
                semiMajorAxis = 10,
                inclination = 0,
                eccentricity = 1.1,
                argumentOfPeriapsis = 0,
                longitudeOfAscendingNode = 0,
                trueAnomaly = 0
            }
        };
        cBodyList.Add(anotherBody);

        CelestialBody moon = new CelestialBody{
            mass = 10000,
            orbit = new Orbit{
                parent = anotherBody,
                semiMajorAxis = 4,
                inclination = 90,
                eccentricity = 0,
                argumentOfPeriapsis = 0,
                longitudeOfAscendingNode = 0,
                trueAnomaly = 0
            }
        };
        //cBodyList.Add(moon);
        
        foreach (CelestialBody cBody in cBodyList)
        {
            cBody.CreateDebugOrb(this);
        }
    }

    public override void _Process(double delta)
    {
        anotherBody.orbit.semiMajorAxis = testBodySMA*10;
        anotherBody.orbit.eccentricity = testBodyECC;
        foreach (CelestialBody cBody in cBodyList)
        {
            if (cBody.orbit.parent != null)
            {
                (Double3 position, Double3 velocity) = PatchedConics.KOEtoECI(cBody.orbit, cBody.orbit.parent, Time.GetUnixTimeFromSystem()*timeSpeed, 0);

                cBody.orbit.position = position;
                Double3 finalPos = position + cBody.orbit.parent.orbit.position;
                
                cBody.debugOrb.GlobalPosition = new Vector3((float)finalPos.X,(float)finalPos.Y,(float)finalPos.Z);
            }
        }
    }
}

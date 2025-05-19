using Godot;
using System;
using System.Collections.Generic;

public partial class OrbitTester : Node3D
{
    [Export] public bool paused = false;
    [Export] public bool updateOrbitParams = false;
    [Export] public bool updateCartParams = false;
    [Export] public bool useVelocity = false;
    [Export] public Vector3 orbitPos;
    [Export] public Vector3 orbitVel;
    [Export] public double testBodySMA = 2;
    [Export] public double testBodyECC = 0;
    [Export] public double testBodyINC = 0;
    [Export] public double timeSpeed = 1;
    [Export] public double time;
    [Export] public OrbitRenderer testBodyRenderer;
    public List<CelestialBody> cBodyList = [];

    public CelestialBody anotherBody;

    public override void _Ready()
    {
        CelestialBody rootBody = new CelestialBody{
            mass = 10000000000000,
            cartesianData = new CartesianData{
                position = new Double3(0,0,0)
            }
        };
        cBodyList.Add(rootBody);

        anotherBody = new CelestialBody{
            mass = 1000000000000,
            orbit = new Orbit{
                parent = rootBody,
                semiMajorAxis = 10,
                inclination = 0.1,
                eccentricity = 1.1,
                argumentOfPeriapsis = 0,
                longitudeOfAscendingNode = 0,
                trueAnomaly = 0
            },
            cartesianData = new CartesianData{
                parent = rootBody,
                position = Double3.Zero,
                velocity = Double3.Zero
            }
        };
        anotherBody.cartesianData.position = new Double3(orbitPos.X, orbitPos.Y, orbitPos.Z);
        anotherBody.cartesianData.velocity = new Double3(orbitVel.X, orbitVel.Y, orbitVel.Z);
        cBodyList.Add(anotherBody);
        testBodyRenderer.cBody = anotherBody;
        
        foreach (CelestialBody cBody in cBodyList)
        {
            cBody.CreateDebugOrb(this);
            if (cBody.orbit != null)
            {
                _ = cBody.orbit.ComputeMU();
                _ = cBody.orbit.ComputePeriod();
            }
           
        }
    }

    public override void _Process(double delta)
    {
        if (updateOrbitParams)
        {
            anotherBody.orbit.semiMajorAxis = testBodySMA*10;
            anotherBody.orbit.eccentricity = testBodyECC;
            anotherBody.orbit.inclination = testBodyINC;
        }

        if (updateCartParams)
        {
            anotherBody.cartesianData.position = new Double3(orbitPos.X, orbitPos.Y, orbitPos.Z);
            anotherBody.cartesianData.velocity = new Double3(orbitVel.X, orbitVel.Y, orbitVel.Z);
        }

        foreach (CelestialBody cBody in cBodyList)
        {
            if (cBody.orbit != null && !paused)
            {
                if (useVelocity)
                {
                    (Orbit orbit, double t) = PatchedConics.ECItoKOE(cBody.cartesianData, cBody.orbit.parent, time);

                    (Double3 position, Double3 velocity) = PatchedConics.KOEtoECI(orbit, orbit.parent, t, 0);

                    GD.Print(orbit.inclination);
                    GD.Print(position.X + " " + position.Y + " " + position.Z);
                    cBody.orbit = orbit;
                    //cBody.cartesianData.position = position;
                    //cBody.cartesianData.velocity = velocity;

                    cBody.debugOrb.GlobalPosition = new Vector3((float)cBody.cartesianData.position.X,(float)cBody.cartesianData.position.Y,(float)cBody.cartesianData.position.Z);
                }else{
                    orbitPos = anotherBody.cartesianData.position.ToFloat3();
                    orbitVel = anotherBody.cartesianData.velocity.ToFloat3();

                    (Double3 position, Double3 velocity) = PatchedConics.KOEtoECI(cBody.orbit, cBody.orbit.parent, time, 0);//Time.GetUnixTimeFromSystem()*timeSpeed, 0);
                    testBodyRenderer.time = time;

                    cBody.cartesianData.position = position;
                    cBody.cartesianData.velocity = velocity;

                    Double3 finalPos = position + cBody.orbit.parent.cartesianData.position;
                    
                    cBody.debugOrb.GlobalPosition = new Vector3((float)finalPos.X,(float)finalPos.Y,(float)finalPos.Z);
                }
            }
        }
    }
}

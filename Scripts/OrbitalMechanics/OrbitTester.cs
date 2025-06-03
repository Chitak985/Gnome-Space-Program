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
    [Export] public Vector3 orbitAccel;
    [Export] public double testBodySMA = 2;
    [Export] public double testBodyECC = 0;
    [Export] public double testBodyINC = 0;
    [Export] public double timeSpeed = 1;
    [Export] public double time;
    [Export] public OrbitRenderer testBodyRenderer;
    [Export] public OrbitDisplay orbitDisplay;
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
                inclination = 0,
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
        orbitDisplay.cBody = anotherBody;
        
        foreach (CelestialBody cBody in cBodyList)
        {
            cBody.CreateDebugOrb(this);
            if (cBody.orbit != null)
            {
                _ = cBody.orbit.ComputeMU();
                _ = cBody.orbit.ComputePeriod();
            }
           
        }

        //anotherBody.cartesianData.position = new Double3(orbitPos.X, orbitPos.Y, orbitPos.Z);
        //anotherBody.cartesianData.velocity = new Double3(orbitVel.X, orbitVel.Y, orbitVel.Z);
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

            orbitAccel = Vector3.Zero;

            if (Input.IsKeyPressed(Key.W))
            {
                orbitAccel.Y = 0.2f;
            }
            if (Input.IsKeyPressed(Key.A))
            {
                orbitAccel.X = -0.2f;
            }
            if (Input.IsKeyPressed(Key.S))
            {
                orbitAccel.Y = -0.2f;
            }
            if (Input.IsKeyPressed(Key.D))
            {
                orbitAccel.X = 0.2f;
            }
        }

        foreach (CelestialBody cBody in cBodyList)
        {
            if (cBody.orbit != null && !paused)
            {
                if (useVelocity)
                {
                    Orbit orbit = PatchedConics.ECItoKOE(cBody.cartesianData);

                    (Double3 position, _) = PatchedConics.KOEtoECI(orbit);
                    //GD.Print(position.X + " " + position.Y + " " + position.Z);
                    cBody.orbit = orbit;
                    //cBody.cartesianData.position = position;
                    //cBody.cartesianData.velocity = velocity;

                    //cBody.orbit.trueAnomaly = PatchedConics.TimeToTrueAnomaly(cBody.orbit, time, 0);

                    //cBody.orbit = PatchedConics.AccelerateOrbit(orbit, time, new Double3(0,0,0));
                    
                    //GD.Print(cBody.cartesianData.velocity.Y);

                    //Double3 position = cBody.cartesianData.position;

                    cBody.debugOrb.GlobalPosition = new Vector3((float)position.X,(float)position.Y,(float)position.Z);
                }else{
                    orbitPos = anotherBody.cartesianData.position.ToFloat3();
                    orbitVel = anotherBody.cartesianData.velocity.ToFloat3();

                    cBody.orbit.trueAnomaly = PatchedConics.TimeToTrueAnomaly(cBody.orbit, SaveManager.Instance.saveTime, 0);
                    GD.Print(cBody.orbit.period);

                    (Double3 position, Double3 velocity) = PatchedConics.KOEtoECI(cBody.orbit);//Time.GetUnixTimeFromSystem()*timeSpeed, 0);

                    cBody.cartesianData.position = position;
                    cBody.cartesianData.velocity = velocity;

                    Double3 finalPos = position + cBody.orbit.parent.cartesianData.position;
                    
                    cBody.debugOrb.GlobalPosition = new Vector3((float)finalPos.X,(float)finalPos.Y,(float)finalPos.Z);
                }

                //cBody.orbit.DumpOrbitParams();
            }
        }
    }
}

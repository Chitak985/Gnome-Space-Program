using Godot;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

// All-encompassing class for everything patched-conics related
/*
Sources used:
https://en.wikipedia.org/wiki/Earth-centered_inertial
https://www.sciencedirect.com/topics/engineering/patched-conic
https://ai-solutions.com/_freeflyeruniversityguide/patched_conics_transfer.htm#calculatingapatchedconicsproblem
https://www.mathworks.com/help/aerotbx/ug/keplerian2ijk.html
https://space.stackexchange.com/questions/19322/converting-orbital-elements-to-cartesian-state-vectors
*/
public partial class PatchedConics : Node
{
    public static float GravConstant {get;} = 6.674e-11F;
    // Gets body-centered coordinates from orbit parameters
    // Keplerian orbital elements to earth centered whateverthefuck
    public static (Double3, Double3) KOEtoECI(Orbit orbit, CelestialBody parent, double time, double initialTIme) //, Dr Freeman? Is it really that time again?
    {
        // Mathematicians are allergic to meaningful variable names so god knows what this is supposed to be
        double MU = GravConstant * parent.mass;

        // Compile our favourite Keplerian orbit elements
        double SMA = orbit.semiMajorAxis;
        double ECC = orbit.eccentricity;
        double INC = orbit.inclination;
        double AOP = orbit.argumentOfPeriapsis;
        double LAN = orbit.longitudeOfAscendingNode;
        double TRA = orbit.trueAnomaly;

        // Compute the mean anomaly
        double PRD = 2 * Mathf.Pi * Mathf.Sqrt(SMA * SMA * SMA / MU); //Orbital period
        double n = Math.Sqrt(MU/Math.Pow(SMA,3));
        double M = n*(time-PRD);

        double EA = GetEccentricAnomaly(M, ECC);
        double MA = EA - ECC * Math.Sin(EA);

        // What the fuck??
        double V = 2*Math.Atan(Math.Sqrt((1+ECC)/(1-ECC)) * Math.Tan(EA/2)); // V
        // Step four
        double radius = SMA*(1 - ECC*Math.Cos(EA));
        // Step five
        double H = Math.Sqrt(MU*SMA * (1 - Math.Pow(ECC,2)));
        // Step SIX!?!??
        // I know I just stole this shit from stackexchange but why the fuck are you doing this?
        double Om = LAN;
        double w =  AOP;

        double X = radius*(Math.Cos(Om)*Math.Cos(w+V) - Math.Sin(Om)*Math.Sin(w+V)*Math.Cos(INC));
        double Y = radius*(Math.Sin(Om)*Math.Cos(w+V) + Math.Cos(Om)*Math.Sin(w+V)*Math.Cos(INC));
        double Z = radius*(Math.Sin(INC)*Math.Sin(w+V));

        // STEP SEVEN!!!!
        double p = SMA*(1-Math.Pow(ECC,2)); // The hell is p???? Imagine a world where mathematicians explained what the fuck these variables are

        double V_X = X*H*ECC/(radius*p)*Math.Sin(V) - H/radius*(Math.Cos(Om)*Math.Sin(w+V) + Math.Sin(Om)*Math.Cos(w+V)*Math.Cos(INC));
        double V_Y = Y*H*ECC/(radius*p)*Math.Sin(V) - H/radius*(Math.Sin(Om)*Math.Sin(w+V) - Math.Cos(Om)*Math.Cos(w+V)*Math.Cos(INC));
        double V_Z = Z*H*ECC/(radius*p)*Math.Sin(V) + H/radius*(Math.Cos(w+V)*Math.Sin(INC));

        return (new Double3(X,Y,Z), new Double3(V_X,V_Y,V_Z));
    }

    // Keplerian method of calculating eccentric anomaly apparently
    public static double GetEccentricAnomaly(double meanAnomaly, double eccentricity, double tolerance = 1e-6, int maxIter = 100)
    {
        double E;

        if (eccentricity < 0.8) E = meanAnomaly; else E = Math.PI;
        for (int i = 0; i < maxIter; i++)
        {
            double delta = (E - eccentricity * Math.Sin(E) - meanAnomaly) / (1 - eccentricity * Math.Cos(E));
            E -= delta;
            if (Math.Abs(delta) < tolerance)
            {
                break;
            }
        }
            
        return E;
    }
}

// Orbit without defining time.
public class Orbit
{
    public CelestialBody parent;

    public double semiMajorAxis;
    public double eccentricity;
    public double inclination;
    public double argumentOfPeriapsis;
    public double longitudeOfAscendingNode;
    public double trueAnomaly;
}
